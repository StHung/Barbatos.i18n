
namespace Barbatos.i18n.Maui;

public sealed class PluralStringLocalizerConverter : IMultiValueConverter
{
    public string? Text { get; }
    public string? PluralText { get; }
    public string? Namespace { get; }
    public string ProviderKey { get; }

    public PluralStringLocalizerConverter(string? text, string? pluralText, string? textNamespace, string providerKey)
    {
        Text = text;
        PluralText = pluralText;
        Namespace = textNamespace;
        ProviderKey = providerKey;
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (string.IsNullOrEmpty(Text) && string.IsNullOrEmpty(PluralText))
        {
            return string.Empty;
        }

        int count = 0;
        if (values.Length > 0 && values[0] is int countValue)
        {
            count = countValue;
        }

        CultureInfo currentCulture =
            MauiLocalization.GetProvider(ProviderKey)?.GetCulture()
            ?? LocalizationProviderFactory.GetInstance(ProviderKey)?.GetCulture()
            ?? CultureInfo.CurrentUICulture;

        string? selectedNamespace = Namespace?.ToLowerInvariant();

        LocalizationSet? localizationSet = MauiLocalization.GetProvider(ProviderKey)?.GetLocalizationSet(currentCulture, selectedNamespace)
            ?? LocalizationProviderFactory.GetInstance(ProviderKey)?.GetLocalizationSet(currentCulture, selectedNamespace);

        if (localizationSet is null)
        {
            return count > 1 ? (PluralText ?? string.Empty) : (Text ?? string.Empty);
        }

        string localizedString;

        if (count > 1)
        {
            var keyStr = PluralText ?? string.Empty;
            localizedString =
                localizationSet.Strings.FirstOrDefault(s => s.Key.ToString() == keyStr).Value
                ?? PluralText
                ?? string.Empty;
        }
        else
        {
            var keyStr = Text ?? string.Empty;
            localizedString =
                localizationSet.Strings.FirstOrDefault(s => s.Key.ToString() == keyStr).Value
                ?? Text
                ?? string.Empty;
        }

        return string.Format(currentCulture, localizedString, count);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
