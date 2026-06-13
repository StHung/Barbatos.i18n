// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Barbatos.i18n.Json;

/// <summary>
/// Writes localization strings to a JSON file on disk.
/// Note: This is typically used during development or if resources are stored as physical files instead of embedded resources.
/// </summary>
public class JsonLocalizationWriter : ILocalizationResourceWriter
{
    private readonly string _baseDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonLocalizationWriter"/> class.
    /// </summary>
    /// <param name="baseDirectory">The physical directory path where the JSON resource files are located.</param>
    public JsonLocalizationWriter(string baseDirectory)
    {
        _baseDirectory = baseDirectory;
    }

    /// <inheritdoc />
    public string ParserName => "Json";

    /// <inheritdoc />
    public async Task WriteAsync(CultureInfo culture, string? setName, LocalizationKey key, string value)
    {
        if (string.IsNullOrWhiteSpace(_baseDirectory) || !Directory.Exists(_baseDirectory))
        {
            return; // Cannot write if directory doesn't exist
        }

        // Construct file name, e.g. "vi-VN.json" or "MainPage-vi-VN.json"
        string fileName = string.IsNullOrWhiteSpace(setName) 
            ? $"{culture.Name}.json" 
            : $"{setName}-{culture.Name}.json"; // Adjust convention as needed based on your project

        // Fallback convention
        string filePath = Path.Combine(_baseDirectory, fileName);
        if (!File.Exists(filePath))
        {
            filePath = Path.Combine(_baseDirectory, $"{culture.Name}.json");
            if (!File.Exists(filePath))
            {
                // Create a new one if it doesn't exist
                await File.WriteAllTextAsync(filePath, "{}");
            }
        }

        try
        {
            string jsonContent = await File.ReadAllTextAsync(filePath);
            JsonNode? rootNode = JsonNode.Parse(jsonContent);
            
            if (rootNode is JsonObject rootObject)
            {
                // We assume Version 2 (flat or hierarchical). For simplicity, we just add it to the root if it's not nested, 
                // or handle simple dotted keys by creating nested objects.
                
                string[] parts = key.ToString().Split('.');
                JsonObject currentObject = rootObject;

                for (int i = 0; i < parts.Length - 1; i++)
                {
                    string part = parts[i];
                    if (currentObject.TryGetPropertyValue(part, out JsonNode? childNode) && childNode is JsonObject childObj)
                    {
                        currentObject = childObj;
                    }
                    else
                    {
                        var newObj = new JsonObject();
                        currentObject[part] = newObj;
                        currentObject = newObj;
                    }
                }

                string lastPart = parts[^1];
                currentObject[lastPart] = value;

                var options = new JsonSerializerOptions { WriteIndented = true };
                string newJsonContent = rootObject.ToJsonString(options);

                await File.WriteAllTextAsync(filePath, newJsonContent);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to write to JSON localization file: {ex.Message}");
        }
    }
}
