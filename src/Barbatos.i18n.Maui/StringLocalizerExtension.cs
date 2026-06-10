using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Barbatos.i18n;

namespace Barbatos.i18n.Maui;

[ContentProperty(nameof(Text))]
public class StringLocalizerExtension : IMarkupExtension<BindingBase>
{
    public string? Text { get; set; }
    public string? Namespace { get; set; }
    public string ProviderKey { get; set; } = string.Empty;
    public object? Arg { get; set; } = null;
    public object? Arg2 { get; set; } = null;
    public object? Arg3 { get; set; } = null;
    public object? Arg4 { get; set; } = null;
    public object? Arg5 { get; set; } = null;
    public BindingBase? BindArg { get; set; } = null;
    public BindingBase? BindArg2 { get; set; } = null;
    public BindingBase? BindArg3 { get; set; } = null;
    public BindingBase? BindArg4 { get; set; } = null;
    public BindingBase? BindArg5 { get; set; } = null;
    public string? StringFormat { get; set; } = null;

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        bool useBinding = BindArg is not null || BindArg2 is not null || BindArg3 is not null || BindArg4 is not null || BindArg5 is not null;

        if (useBinding)
        {
            var multiBinding = new MultiBinding
            {
                Converter = new StringLocalizerConverter(Text, Namespace, ProviderKey),
                StringFormat = StringFormat
            };

            if (BindArg is not null) multiBinding.Bindings.Add(BindArg);
            else if (Arg is not null) multiBinding.Bindings.Add(new Binding { Source = Arg });

            if (BindArg2 is not null) multiBinding.Bindings.Add(BindArg2);
            else if (Arg2 is not null) multiBinding.Bindings.Add(new Binding { Source = Arg2 });

            if (BindArg3 is not null) multiBinding.Bindings.Add(BindArg3);
            else if (Arg3 is not null) multiBinding.Bindings.Add(new Binding { Source = Arg3 });

            if (BindArg4 is not null) multiBinding.Bindings.Add(BindArg4);
            else if (Arg4 is not null) multiBinding.Bindings.Add(new Binding { Source = Arg4 });

            if (BindArg5 is not null) multiBinding.Bindings.Add(BindArg5);
            else if (Arg5 is not null) multiBinding.Bindings.Add(new Binding { Source = Arg5 });

            return multiBinding;
        }

        CultureInfo currentCulture =
            MauiLocalization.GetProvider(ProviderKey)?.GetCulture()
            ?? LocalizationProviderFactory.GetInstance(ProviderKey)?.GetCulture()
            ?? CultureInfo.CurrentUICulture;

        string? selectedNamespace = Namespace?.ToLowerInvariant();

        LocalizationSet? localizationSet = MauiLocalization.GetProvider(ProviderKey)?.GetLocalizationSet(currentCulture, selectedNamespace)
            ?? LocalizationProviderFactory.GetInstance(ProviderKey)?.GetLocalizationSet(currentCulture, selectedNamespace);

        string result = Text ?? string.Empty;
        if (localizationSet is not null && Text is not null)
        {
            List<object?>? args = null;
            if (Arg is not null) { args ??= new List<object?>(); args.Add(Arg); }
            if (Arg2 is not null) { args ??= new List<object?>(); args.Add(Arg2); }
            if (Arg3 is not null) { args ??= new List<object?>(); args.Add(Arg3); }
            if (Arg4 is not null) { args ??= new List<object?>(); args.Add(Arg4); }
            if (Arg5 is not null) { args ??= new List<object?>(); args.Add(Arg5); }

            result = localizationSet.Format(currentCulture, (LocalizationKey)Text, args?.ToArray() ?? null) ?? Text ?? string.Empty;
        }

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
