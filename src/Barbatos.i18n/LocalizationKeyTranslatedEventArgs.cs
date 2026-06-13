// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n;

/// <summary>
/// Provides data for the <see cref="ILocalizationProvider.OnKeyTranslated"/> event.
/// </summary>
public class LocalizationKeyTranslatedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the culture for which the key was translated.
    /// </summary>
    public CultureInfo Culture { get; }

    /// <summary>
    /// Gets the localization key.
    /// </summary>
    public LocalizationKey Key { get; }

    /// <summary>
    /// Gets the translated value.
    /// </summary>
    public string? Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationKeyTranslatedEventArgs"/> class.
    /// </summary>
    public LocalizationKeyTranslatedEventArgs(CultureInfo culture, LocalizationKey key, string? value)
    {
        Culture = culture;
        Key = key;
        Value = value;
    }
}
