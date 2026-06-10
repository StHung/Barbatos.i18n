
namespace Barbatos.i18n.Maui;

public sealed class StringLocalizerConverter : IMultiValueConverter
{
    public string? Text { get; }
    public string? Namespace { get; }
    public string ProviderKey { get; }

    public StringLocalizerConverter(string? text, string? textNamespace, string providerKey)
    {
        Text = text;
        Namespace = textNamespace;
        ProviderKey = providerKey;
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (Text is null)
        {
            return string.Empty;
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
            return Text;
        }

        return localizationSet.Format(culture, (LocalizationKey)Text, values) ?? Text;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
