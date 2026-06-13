// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Windows;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Barbatos.i18n.DependencyInjection;
using Barbatos.i18n.Wpf;
using Barbatos.i18n.Ini;
using Barbatos.i18n.Csv;
using Barbatos.i18n.Json;
using Barbatos.i18n.Yaml;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

namespace Barbatos.i18n.Sample.Wpf;

public partial class App : Application
{
    public IServiceProvider ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        // Default Provider
        services.AddStringLocalizer(options => 
        {
            options.SyncFormattingCulture = false;
        }, builder =>
        {
            builder.FromIni("Locales.en-US.ini", new CultureInfo("en-US"));
            builder.FromIni("Locales.vi-VN.ini", new CultureInfo("vi-VN"));
            builder.FromIni("Locales.ko-KR.ini", new CultureInfo("ko-KR"));

            // Load from YAML (Built-in)
            builder.FromYaml("Locales.Settings-en-US.yaml", new CultureInfo("en-US"));
            builder.FromYaml("Locales.Settings-vi-VN.yaml", new CultureInfo("vi-VN"));
            builder.FromYaml("Locales.Settings-ko-KR.yaml", new CultureInfo("ko-KR"));

            // Load from JSON (v2.0)
            builder.FromJson("Locales.Validation-en-US.json", new CultureInfo("en-US"));
            builder.FromJson("Locales.Validation-vi-VN.json", new CultureInfo("vi-VN"));
            builder.FromJson("Locales.Validation-ko-KR.json", new CultureInfo("ko-KR"));

            // Load from RESX
            builder.FromResource<Locales.Strings>(new CultureInfo("en-US"));
            builder.FromResource<Locales.Strings>(new CultureInfo("vi-VN"));
            builder.FromResource<Locales.Strings>(new CultureInfo("ko-KR"));

            // Load Errors namespace
            builder.FromCsv("Locales.Errors.csv");
        });

        // Setup Secondary Provider
        services.AddStringLocalizer("SecondaryProvider", builder =>
        {
            builder.FromJson("Locales.Extra-en-US.json", new CultureInfo("en-US"));
            builder.FromJson("Locales.Extra-vi-VN.json", new CultureInfo("vi-VN"));
            builder.FromJson("Locales.Extra-ko-KR.json", new CultureInfo("ko-KR"));
        });



        var configBuilder = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        var configuration = configBuilder.Build();

        string? apiKey = configuration["AI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            apiKey = Environment.GetEnvironmentVariable("AI_API_KEY", EnvironmentVariableTarget.User) 
                ?? Environment.GetEnvironmentVariable("AI_API_KEY")
                ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY"); // keep backward compatibility
        }

        string modelId = configuration["AI:ModelId"] ?? "gpt-4o-mini";
        string provider = configuration["AI:Provider"] ?? "OpenAI";

        if (string.IsNullOrWhiteSpace(apiKey) && !provider.Equals("Ollama", StringComparison.OrdinalIgnoreCase))
        {
            throw new Barbatos.i18n.Mcp.McpRegistrationException("AI API Key is missing. Please configure 'AI:ApiKey' in appsettings.json or set the 'AI_API_KEY' environment variable before running the application.");
        }

        OpenAI.Chat.ChatClient openAiChatClient;
        if (provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
        {
            var options = new OpenAI.OpenAIClientOptions
            {
                Endpoint = new Uri("https://generativelanguage.googleapis.com/v1beta/openai/")
            };
            openAiChatClient = new OpenAI.Chat.ChatClient(modelId, new System.ClientModel.ApiKeyCredential(apiKey), options);
        }
        else if (provider.Equals("Ollama", StringComparison.OrdinalIgnoreCase))
        {
            var options = new OpenAI.OpenAIClientOptions
            {
                Endpoint = new Uri("http://localhost:11434/v1/")
            };
            // Ollama does not require a real API key, but the OpenAI client requires a non-empty string.
            string ollamaKey = string.IsNullOrWhiteSpace(apiKey) ? "ollama-local" : apiKey;
            openAiChatClient = new OpenAI.Chat.ChatClient(modelId, new System.ClientModel.ApiKeyCredential(ollamaKey), options);
        }
        else
        {
            openAiChatClient = new OpenAI.Chat.ChatClient(modelId, apiKey);
        }

        services.AddSingleton<Microsoft.Extensions.AI.IChatClient>(openAiChatClient.AsIChatClient());
        
        services.AddAIAutoTranslation();

        // Register writer to save directly to the Project Source folder (so changes are not lost on rebuild)
        string projectDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppContext.BaseDirectory, "../../../.."));
        string localesDir = System.IO.Path.Combine(projectDir, "Locales");
        services.AddSingleton<Barbatos.i18n.ILocalizationResourceWriter>(new Barbatos.i18n.Json.JsonLocalizationWriter(localesDir));

        ServiceProvider = services.BuildServiceProvider();

        ServiceProvider.UseWpfLocalization()
                       .SetLocalizationCulture(CultureInfo.CurrentUICulture);
    }
}
