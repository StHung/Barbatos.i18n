using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Barbatos.i18n;

namespace Barbatos.i18n.Mcp;

/// <summary>
/// Provides extension methods to add Barbatos.i18n localization tools to an MCP server.
/// </summary>
public static class McpServerLocalizationExtensions
{
    /// <summary>
    /// Adds Barbatos.i18n tools to the MCP server builder.
    /// This allows LLMs to interact with the localization engine via the MCP protocol.
    /// </summary>
    /// <param name="builder">The MCP server builder.</param>
    /// <returns>The MCP server builder.</returns>
    public static IMcpServerBuilder AddLocalizationTools(this IMcpServerBuilder builder)
    {
        var readTranslationTool = McpServerTool.Create(ReadTranslationToolAsync, new McpServerToolCreateOptions
        {
            Name = "GetTranslation",
            Description = "Reads an existing translation from the Barbatos.i18n engine. Provide the key and culture (e.g. 'en-US', 'vi-VN')."
        });

        var getCulturesTool = McpServerTool.Create(GetCulturesToolAsync, new McpServerToolCreateOptions
        {
            Name = "GetAvailableCultures",
            Description = "Returns a list of available cultures registered in the Barbatos.i18n engine."
        });

        var automateTranslationTool = McpServerTool.Create(AutomateTranslationToolAsync, new McpServerToolCreateOptions
        {
            Name = "AutomateTranslation",
            Description = "Requests a translation of dynamic content from the AI model."
        });

        builder.WithTools([readTranslationTool, getCulturesTool, automateTranslationTool]);

        return builder;
    }

    internal static async Task<string> ReadTranslationToolAsync(string key, string culture)
    {
        var provider = LocalizationProviderFactory.GetInstance();
        if (provider == null)
        {
            return $"Error: LocalizationProvider is not initialized.";
        }

        var cultureInfo = new System.Globalization.CultureInfo(culture);
        var sets = provider.GetLocalizationSets(cultureInfo);

        if (sets == null || !sets.Any())
        {
            return $"Error: Culture '{culture}' not found.";
        }

        var locKey = new LocalizationKey(key);
        foreach (var set in sets)
        {
            var value = set[locKey];
            if (value != null)
            {
                return value;
            }
        }

        return $"Error: Key '{key}' not found for culture '{culture}'.";
    }

    internal static async Task<string> GetCulturesToolAsync()
    {
         var provider = LocalizationProviderFactory.GetInstance();
         if (provider == null) return "Error: LocalizationProvider is not initialized.";

         var sets = provider.GetLocalizationSets();
         var cultures = sets.Select(s => s.Culture.Name).Distinct().ToList();
         return string.Join(", ", cultures);
    }

    internal static async Task<string> AutomateTranslationToolAsync(string content, string sourceCulture, string targetCulture)
    {
        await Task.CompletedTask;
        return $"Instruction for LLM: Please translate the following content from {sourceCulture} to {targetCulture}. Content: {content}";
    }
}


