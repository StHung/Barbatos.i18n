// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Wpf;

/// <summary>
/// Provides a multi value converter that localizes strings in XAML.
/// </summary>
public sealed class StringLocalizerConverter : IMultiValueConverter
{
    private readonly string?[]? _stringFormats;

    /// <summary>
    /// Gets or sets the text to be localized.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets or sets the namespace of the text to be localized.
    /// </summary>
    public string? Namespace { get; }

    /// <summary>
    /// Provider key.
    /// </summary>
    public string ProviderKey { get; }
    
    public string? DefaultText { get; }
    public string? Context { get; }

    public StringLocalizerConverter(string text, string? textNamespace, string providerKey, string?[]? stringFormats = null, string? defaultText = null, string? context = null)
    {
        Text = text;
        Namespace = textNamespace;
        ProviderKey = providerKey;
        _stringFormats = stringFormats;
        DefaultText = defaultText;
        Context = context;
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (Text is null)
        {
            return string.Empty;
        }

        CultureInfo currentCulture =
            WpfLocalization.GetProvider(ProviderKey)?.GetCulture()
            ?? LocalizationProviderFactory.GetInstance(ProviderKey)?.GetCulture()
            ?? CultureInfo.CurrentUICulture;

        string? selectedNamespace = Namespace?.ToLowerInvariant();

        ILocalizationProvider? provider = WpfLocalization.GetProvider(ProviderKey) ?? LocalizationProviderFactory.GetInstance(ProviderKey);
        LocalizationSet? localizationSet = null;

        if (provider != null)
        {
            if (selectedNamespace != null)
            {
                localizationSet = provider.GetLocalizationSet(currentCulture, selectedNamespace);
            }
            else
            {
                var sets = provider.GetLocalizationSets(currentCulture);
                foreach (var set in sets)
                {
                    if (set[new LocalizationKey(Text)] != null)
                    {
                        localizationSet = set;
                        break;
                    }
                }

                if (localizationSet == null)
                {
                    localizationSet = provider.GetLocalizationSet(currentCulture, null);
                }
            }
        }

        if (localizationSet is null)
        {
            return DefaultText ?? StringLocalizerExtension.EscapeText(Text);
        }

        // Apply StringFormat to individual values if provided
        var formatValues = new object?[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            var val = values[i];
            if (val == DependencyProperty.UnsetValue)
            {
                formatValues[i] = string.Empty;
                continue;
            }

            var format = _stringFormats is not null && i < _stringFormats.Length ? _stringFormats[i] : null;
            if (!string.IsNullOrEmpty(format))
            {
                formatValues[i] = string.Format(currentCulture, format, val);
            }
            else
            {
                formatValues[i] = val;
            }
        }

        return localizationSet.Format(culture, Text, formatValues) ?? DefaultText ?? StringLocalizerExtension.EscapeText(Text);
    }

    /// <summary>
    /// Not supported.
    /// </summary>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
