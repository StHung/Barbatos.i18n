using System;
using Microsoft.Maui.Controls;
using Barbatos.i18n;

namespace Barbatos.i18n.Maui;

public static class MauiLocalization
{
    public static IServiceProvider? ServiceProvider { get; private set; }
    private static ILocalizationProvider? _directProvider;

    public static void Initialize(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public static void Initialize(ILocalizationProvider provider)
    {
        _directProvider = provider;
    }

    public static ILocalizationProvider? GetProvider(string? key = null)
    {
        if (_directProvider != null) return _directProvider;

        if (ServiceProvider == null) return null;

        var resolver = ServiceProvider.GetService(typeof(ILocalizationProviderResolver)) as ILocalizationProviderResolver;
        if (resolver != null)
        {
            return resolver.GetProvider(key);
        }

        return null;
    }
}
