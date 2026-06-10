
namespace Barbatos.i18n.Maui;

[ContentProperty(nameof(Count))]
public class PluralStringLocalizerExtension : IMarkupExtension<BindingBase>
{
    public int? Count { get; set; }
    public string? Text { get; set; }
    public string? PluralText { get; set; }
    public string? Namespace { get; set; }
    public string ProviderKey { get; set; } = string.Empty;
    public BindingBase? BindCount { get; set; } = null;
    public string? StringFormat { get; set; } = null;

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(Text) && string.IsNullOrEmpty(PluralText))
        {
            return new Binding { Source = string.Empty };
        }

        if (BindCount is not null)
        {
            var multiBinding = new MultiBinding
            {
                Converter = new PluralStringLocalizerConverter(Text, PluralText, Namespace, ProviderKey),
                StringFormat = StringFormat
            };
            multiBinding.Bindings.Add(BindCount);
            return multiBinding;
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
            string fallback = Count > 1 ? (PluralText ?? string.Empty) : (Text ?? string.Empty);
            return new Binding { Source = fallback };
        }

        string localizedString;

        if (Count > 1)
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

        string result = string.Format(currentCulture, localizedString, Count ?? 0);
        if (StringFormat is not null)
        {
            result = string.Format(StringFormat, result);
        }
        return new Binding { Source = result };
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
    {
        return ProvideValue(serviceProvider);
    }
}
