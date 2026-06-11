// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

using System.Globalization;
using System.Windows.Data;

namespace Barbatos.i18n.Wpf;

public sealed class StringLocalizerConverter : IMultiValueConverter
{
    public string Text { get; }
    public string? Namespace { get; }
    public string ProviderKey { get; }
    private readonly string?[]? _stringFormats;

    public StringLocalizerConverter(string text, string? textNamespace, string providerKey, string?[]? stringFormats = null)
    {
        Text = text;
        Namespace = textNamespace;
        ProviderKey = providerKey;
        _stringFormats = stringFormats;
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

        LocalizationSet? localizationSet = WpfLocalization.GetProvider(ProviderKey)?.GetLocalizationSet(currentCulture, selectedNamespace)
            ?? LocalizationProviderFactory.GetInstance(ProviderKey)?.GetLocalizationSet(currentCulture, selectedNamespace);

        if (localizationSet is null)
        {
            return Text;
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

        return localizationSet.Format(culture, Text, formatValues) ?? string.Empty;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
