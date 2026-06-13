// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n;

/// <summary>
/// Defines a service for automatically translating missing localization keys at runtime.
/// </summary>
public interface IAutoTranslationService
{
    /// <summary>
    /// Translates the specified missing key and saves it to the underlying resource storage.
    /// </summary>
    /// <param name="providerKey">The specific provider key this key belongs to (can be empty for default).</param>
    /// <param name="key">The missing localization key.</param>
    /// <param name="defaultText">The default text or fallback value, if any.</param>
    /// <param name="context">The contextual information where the key is used, if any.</param>
    /// <param name="targetCulture">The target culture to translate to.</param>
    /// <param name="args">The runtime arguments used in the localized string.</param>
    /// <returns>The translated text, or the original default text if translation fails.</returns>
    Task<string?> TranslateAndSaveAsync(string providerKey, string key, string? defaultText, string? context, CultureInfo targetCulture, object?[]? args = null);
}
