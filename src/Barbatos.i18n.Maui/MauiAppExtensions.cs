// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Maui;

/// <summary>
/// Provides extension methods for the <see cref="MauiApp"/> class.
/// </summary>
public static class MauiAppExtensions
{
    /// <summary>
    /// Connects the MAUI application to the provided Dependency Injection container.
    /// </summary>
    /// <param name="app">The MAUI application.</param>
    /// <returns>The MAUI application, for chaining.</returns>
    public static MauiApp UseMauiLocalization(this MauiApp app)
    {
        MauiLocalization.Initialize(app.Services);
        return app;
    }

    /// <summary>
    /// Sets the localization culture for all providers registered in the DI container.
    /// </summary>
    /// <param name="app">The MAUI application.</param>
    /// <param name="culture">The culture to set.</param>
    /// <returns>The MAUI application, for chaining.</returns>
    public static MauiApp SetLocalizationCulture(this MauiApp app, CultureInfo culture)
    {
        if (app.Services.GetService(typeof(ILocalizationCultureManager)) is ILocalizationCultureManager cultureManager)
        {
            cultureManager.SetCulture(culture);
        }
        else
        {
            // Fallback for non-DI applications or missing registration
            var fallbackManager = new LocalizationCultureManager();
            fallbackManager.SetCulture(culture);
        }

        return app;
    }
}
