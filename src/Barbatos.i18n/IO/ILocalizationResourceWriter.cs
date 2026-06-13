// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n;

/// <summary>
/// Defines a writer for updating or adding localization strings to a resource file.
/// </summary>
public interface ILocalizationResourceWriter
{
    /// <summary>
    /// Gets the name of the parser associated with this writer.
    /// </summary>
    string ParserName { get; }

    /// <summary>
    /// Writes the specified key-value pair to the localization resource for the given culture.
    /// </summary>
    /// <param name="culture">The culture of the localization set.</param>
    /// <param name="setName">The name of the localization set (e.g., file base name).</param>
    /// <param name="key">The localization key.</param>
    /// <param name="value">The translated value.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    Task WriteAsync(CultureInfo culture, string? setName, LocalizationKey key, string value);
}
