// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace Barbatos.i18n.DependencyInjection;

/// <summary>
/// Provides automatic translation using Microsoft.Extensions.AI.
/// Uses a debouncer to batch requests and avoid API rate limits.
/// </summary>
public class AIAutoTranslationService : IAutoTranslationService, IDisposable
{
    private readonly IChatClient _chatClient;
    private readonly IServiceProvider _serviceProvider;
    
    private class TranslationRequest
    {
        public string Key { get; set; } = string.Empty;
        public string? DefaultText { get; set; }
        public string? Context { get; set; }
        public object?[]? Args { get; set; }
        public TaskCompletionSource<string?> Tcs { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
    
    // Grouped by: CacheKey = $"{providerKey}|{targetCulture.Name}"
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, TranslationRequest>> _pendingRequests = new();
    private readonly Timer _batchTimer;
    private readonly object _timerLock = new();
    private bool _isTimerRunning;

    /// <summary>
    /// Initializes a new instance of the <see cref="AIAutoTranslationService"/> class.
    /// </summary>
    public AIAutoTranslationService(IChatClient chatClient, IServiceProvider serviceProvider)
    {
        _chatClient = chatClient;
        _serviceProvider = serviceProvider;
        _batchTimer = new Timer(OnBatchTimerFired, null, Timeout.Infinite, Timeout.Infinite);
    }

    /// <inheritdoc />
    public Task<string?> TranslateAndSaveAsync(string providerKey, string key, string? defaultText, string? context, CultureInfo targetCulture, object?[]? args = null)
    {
        var cacheKey = $"{providerKey}|{targetCulture.Name}";
        var group = _pendingRequests.GetOrAdd(cacheKey, _ => new ConcurrentDictionary<string, TranslationRequest>());
        
        var request = new TranslationRequest
        {
            Key = key,
            DefaultText = defaultText,
            Context = context,
            Args = args
        };
        
        group[key] = request;

        lock (_timerLock)
        {
            // Debounce: Wait 500ms for more requests to arrive
            _batchTimer.Change(500, Timeout.Infinite);
            _isTimerRunning = true;
        }

        return request.Tcs.Task;
    }

    private void OnBatchTimerFired(object? state)
    {
        lock (_timerLock)
        {
            if (!_isTimerRunning) return;
            _isTimerRunning = false;
        }

        // Fire and forget processing
        _ = ProcessPendingBatchesAsync();
    }

    private async Task ProcessPendingBatchesAsync()
    {
        var snapshot = new Dictionary<string, ConcurrentDictionary<string, TranslationRequest>>();
        foreach (var kvp in _pendingRequests)
        {
            if (!kvp.Value.IsEmpty)
            {
                // Atomically swap the dictionary to start a new batch
                var oldDict = kvp.Value;
                _pendingRequests[kvp.Key] = new ConcurrentDictionary<string, TranslationRequest>();
                snapshot[kvp.Key] = oldDict;
            }
        }

        foreach (var batch in snapshot)
        {
            var parts = batch.Key.Split('|');
            var providerKey = parts[0];
            var cultureName = parts[1];
            var targetCulture = new CultureInfo(cultureName);
            var requests = batch.Value.Values.ToList();

            if (requests.Count == 0) continue;

            await ProcessBatchForCultureAsync(providerKey, targetCulture, requests);
        }
    }

    private async Task ProcessBatchForCultureAsync(string providerKey, CultureInfo targetCulture, List<TranslationRequest> requests)
    {
        try
        {
            var promptBuilder = new System.Text.StringBuilder();
            promptBuilder.AppendLine($"You are an expert translator. Please translate the following UI texts into {targetCulture.NativeName} ({targetCulture.Name}).");
            promptBuilder.AppendLine("Return ONLY a flat JSON object where the keys are the original Keys and the values are your translations.");
            promptBuilder.AppendLine("Do NOT use markdown fences (```json ... ```), just return the raw JSON braces.");
            promptBuilder.AppendLine("Below are the texts to translate (some have Context, Default Text, or Placeholders {0}, {1}...):");
            
            var inputDict = new Dictionary<string, object>();
            foreach (var req in requests)
            {
                var info = new Dictionary<string, object>();
                if (!string.IsNullOrWhiteSpace(req.DefaultText)) info["DefaultText"] = req.DefaultText;
                if (!string.IsNullOrWhiteSpace(req.Context)) info["Context"] = req.Context;
                
                if (req.Args != null && req.Args.Length > 0)
                {
                    var placeholders = new List<string>();
                    for (int i = 0; i < req.Args.Length; i++)
                    {
                        placeholders.Add($"{{{i}}} = {req.Args[i]}");
                    }
                    info["Placeholders"] = placeholders;
                }
                
                if (info.Count == 0)
                {
                    info["Note"] = "Infer a natural human-readable translation from the Key (PascalCase).";
                }
                
                inputDict[req.Key] = info;
            }
            
            promptBuilder.AppendLine(JsonSerializer.Serialize(inputDict, new JsonSerializerOptions { WriteIndented = true }));

            var response = await _chatClient.GetResponseAsync(new List<ChatMessage> { new ChatMessage(ChatRole.User, promptBuilder.ToString()) });
            var responseText = response.Text?.Trim() ?? string.Empty;
            
            // Clean up possible markdown wrappers
            if (responseText.StartsWith("```json")) responseText = responseText.Substring(7);
            if (responseText.StartsWith("```")) responseText = responseText.Substring(3);
            if (responseText.EndsWith("```")) responseText = responseText.Substring(0, responseText.Length - 3);
            responseText = responseText.Trim();

            JsonNode? jsonResponse = null;
            try
            {
                jsonResponse = JsonNode.Parse(responseText);
            }
            catch
            {
                // Parsing failed, we'll gracefully fallback
            }

            ILocalizationProvider? provider = null;
            if (_serviceProvider.GetService<ILocalizationProviderResolver>() is ILocalizationProviderResolver resolver)
            {
                provider = resolver.GetProvider(providerKey);
            }
            else
            {
                provider = LocalizationProviderFactory.GetInstance(providerKey);
            }

            var set = provider?.GetLocalizationSet(targetCulture, string.Empty) 
                   ?? provider?.GetLocalizationSets(targetCulture).FirstOrDefault();

            var writers = _serviceProvider.GetServices<ILocalizationResourceWriter>();
            var setName = targetCulture.Name;

            foreach (var req in requests)
            {
                string? translatedText = null;
                if (jsonResponse != null && jsonResponse[req.Key] != null)
                {
                    translatedText = jsonResponse[req.Key]?.ToString();
                }

                if (!string.IsNullOrWhiteSpace(translatedText))
                {
                    try
                    {
                        System.IO.File.AppendAllText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "AI_Translation_Log.txt"), $"{DateTime.Now}: Translated '{req.Key}' to '{translatedText}' (Batch size: {requests.Count})\n");
                    } catch { }

                    foreach (var writer in writers)
                    {
                        await writer.WriteAsync(targetCulture, setName, new LocalizationKey(req.Key), translatedText);
                    }

                    if (set != null)
                    {
                        set.AddOrUpdateString(new LocalizationKey(req.Key), translatedText);
                        if (provider is LocalizationProvider locProvider)
                        {
                            locProvider.RaiseKeyTranslated(targetCulture, new LocalizationKey(req.Key), translatedText);
                        }
                    }

                    req.Tcs.TrySetResult(translatedText);
                }
                else
                {
                    req.Tcs.TrySetResult(req.DefaultText ?? req.Key);
                }
            }
        }
        catch (Exception ex)
        {
            try
            {
                System.IO.File.AppendAllText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "AI_Translation_Error.log"), $"{DateTime.Now}: {ex.Message}\n{ex.StackTrace}\n");
            } catch { }

            foreach (var req in requests)
            {
                req.Tcs.TrySetResult(req.DefaultText ?? req.Key);
            }
        }
    }

    public void Dispose()
    {
        _batchTimer.Dispose();
        GC.SuppressFinalize(this);
    }
}
