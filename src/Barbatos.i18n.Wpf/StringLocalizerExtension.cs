// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Wpf;

/// <summary>
/// Provides a markup extension that localizes strings in XAML.
/// </summary>
/// <remarks>
/// This class extends <see cref="MarkupExtension"/> and overrides the <see cref="ProvideValue"/> method to return localized strings.
/// </remarks>
[ContentProperty(nameof(Text))]
[MarkupExtensionReturnType(typeof(string))]
public class StringLocalizerExtension : MarkupExtension
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StringLocalizerExtension"/> class.
    /// </summary>
    public StringLocalizerExtension() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringLocalizerExtension"/> class with the specified text.
    /// </summary>
    /// <param name="text">The text to be localized.</param>
    public StringLocalizerExtension(string? text)
    {
        Text = EscapeText(text);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringLocalizerExtension"/> class with the specified text and namespace.
    /// </summary>
    /// <param name="text">The text to be localized.</param>
    /// <param name="textNamespace">The namespace of the text to be localized.</param>
    public StringLocalizerExtension(string? text, string? textNamespace)
    {
        Text = EscapeText(text);
        Namespace = textNamespace;
    }

    /// <summary>
    /// Gets or sets the text to be localized.
    /// </summary>
    public string? Text
    {
        get;
        set => field = EscapeText(value);
    }

    /// <summary>
    /// Gets or sets the namespace of the text to be localized.
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Gets or sets the default text to display if the localization key is missing.
    /// </summary>
    public string? DefaultText { get; set; }

    /// <summary>
    /// Gets or sets the context of where this key is used, helping the auto-translator understand the meaning.
    /// </summary>
    public string? Context { get; set; }

    /// <summary>
    /// Provider key.
    /// </summary>
    public string ProviderKey { get; set; } = string.Empty;

    /// <summary>
    /// Optional argument 1 for string formatting.
    /// </summary>
    public object? Arg { get; set; } = null;

    /// <summary>
    /// Optional argument 2 for string formatting.
    /// </summary>
    public object? Arg2 { get; set; } = null;

    /// <summary>
    /// Optional argument 3 for string formatting.
    /// </summary>
    public object? Arg3 { get; set; } = null;

    /// <summary>
    /// Optional argument 4 for string formatting.
    /// </summary>
    public object? Arg4 { get; set; } = null;

    /// <summary>
    /// Optional argument 5 for string formatting.
    /// </summary>
    public object? Arg5 { get; set; } = null;

    /// <summary>
    /// Optional dynamic argument 1.
    /// </summary>
    public BindingBase? BindArg { get; set; } = null;

    /// <summary>
    /// Optional dynamic argument 2.
    /// </summary>
    public BindingBase? BindArg2 { get; set; } = null;

    /// <summary>
    /// Optional dynamic argument 3.
    /// </summary>
    public BindingBase? BindArg3 { get; set; } = null;

    /// <summary>
    /// Optional dynamic argument 4.
    /// </summary>
    public BindingBase? BindArg4 { get; set; } = null;

    /// <summary>
    /// Optional dynamic argument 5.
    /// </summary>
    public BindingBase? BindArg5 { get; set; } = null;

    /// <summary>
    /// Optional string format to apply to the final localized string.
    /// </summary>
    public string? StringFormat { get; set; } = null;

    /// <summary>
    /// Returns a localized string for the <see cref="Text"/> property.
    /// </summary>
    /// <param name="serviceProvider">An object that provides services for the markup extension.</param>
    /// <returns>The localized string, or the original text if no localization is found.</returns>
    public override object? ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(Text))
        {
            return string.Empty;
        }

        bool useBinding = BindArg is not null || BindArg2 is not null || BindArg3 is not null || BindArg4 is not null || BindArg5 is not null;

        CultureInfo currentCulture =
            WpfLocalization.GetProvider(ProviderKey)?.GetCulture()
            ?? LocalizationProviderFactory.GetInstance(ProviderKey)?.GetCulture()
            ?? CultureInfo.CurrentUICulture;

        string? selectedNamespace = Namespace?.ToLowerInvariant() ?? null;

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

        List<object?>? args = null;

        if (Arg is not null)
        {
            args ??= new List<object?>();
            args.Add(Arg);
        }

        if (Arg2 is not null)
        {
            args ??= new List<object?>();
            args.Add(Arg2);
        }

        if (Arg3 is not null)
        {
            args ??= new List<object?>();
            args.Add(Arg3);
        }

        if (Arg4 is not null)
        {
            args ??= new List<object?>();
            args.Add(Arg4);
        }

        if (Arg5 is not null)
        {
            args ??= new List<object?>();
            args.Add(Arg5);
        }

        bool isMissing = localizationSet is null || localizationSet[new LocalizationKey(Text)] is null;
        
        if (isMissing)
        {
            if (serviceProvider.GetService(typeof(System.Windows.Markup.IProvideValueTarget)) is System.Windows.Markup.IProvideValueTarget target)
            {
                var targetObject = target.TargetObject as DependencyObject;
                var targetProperty = target.TargetProperty as DependencyProperty;

                if (targetObject != null && targetProperty != null)
                {
                    var autoTranslationService = WpfLocalization.ServiceProvider?.GetService(typeof(IAutoTranslationService)) as IAutoTranslationService;
                    if (autoTranslationService != null)
                    {
                        _ = System.Threading.Tasks.Task.Run(async () =>
                        {
                            var translated = await autoTranslationService.TranslateAndSaveAsync(ProviderKey, Text, DefaultText, Context, currentCulture, args?.ToArray());
                            if (translated != null)
                            {
                                targetObject.Dispatcher.InvokeAsync(() =>
                                {
                                    try
                                    {
                                        System.IO.File.AppendAllText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "AI_Translation_Log.txt"), $"[Dispatcher] Updating {targetObject.GetType().Name}.{targetProperty.Name} (useBinding={useBinding})\n");
                                        if (useBinding)
                                        {
                                            var expr = System.Windows.Data.BindingOperations.GetMultiBindingExpression(targetObject, targetProperty);
                                            System.IO.File.AppendAllText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "AI_Translation_Log.txt"), $"[Dispatcher] expr is null? {expr == null}\n");
                                            expr?.UpdateTarget();
                                        }
                                        else
                                        {
                                            string formattedTranslated = translated;
                                            if (args is not null)
                                            {
                                                try { formattedTranslated = string.Format(currentCulture, formattedTranslated, args.ToArray()); } catch { }
                                            }
                                            if (StringFormat is not null)
                                            {
                                                try { formattedTranslated = string.Format(currentCulture, StringFormat, formattedTranslated); } catch { }
                                            }
                                            targetObject.SetValue(targetProperty, formattedTranslated);
                                            System.IO.File.AppendAllText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "AI_Translation_Log.txt"), $"[Dispatcher] SetValue called with '{formattedTranslated}'\n");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        System.IO.File.AppendAllText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "AI_Translation_Error.log"), $"[Dispatcher Exception]: {ex.Message}\n");
                                    }
                                });
                            }
                        });
                    }
                }
            }
        }

        if (useBinding)
        {
            var stringFormats = new string?[]
            {
                (BindArg as Binding)?.StringFormat,
                (BindArg2 as Binding)?.StringFormat,
                (BindArg3 as Binding)?.StringFormat,
                (BindArg4 as Binding)?.StringFormat,
                (BindArg5 as Binding)?.StringFormat
            };

            var multiBinding = new MultiBinding
            {
                Converter = new StringLocalizerConverter(Text, Namespace, ProviderKey, stringFormats, DefaultText, Context),
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

            return multiBinding.ProvideValue(serviceProvider);
        }

        if (isMissing)
        {
            string fallbackText = DefaultText ?? Text;
            
            string formattedFallback = fallbackText;
            if (args is not null)
            {
                try { formattedFallback = string.Format(currentCulture, formattedFallback, args.ToArray()); } catch { }
            }
            if (StringFormat is not null)
            {
                try { formattedFallback = string.Format(currentCulture, StringFormat, formattedFallback); } catch { }
            }
            return formattedFallback;
        }

        string result = localizationSet!.Format(currentCulture, Text, args?.ToArray() ?? null)!;
        if (StringFormat is not null)
        {
            return string.Format(StringFormat, result);
        }
        return result;
    }

    /// <summary>
    /// Escapes special characters in a string.
    /// </summary>
    /// <param name="text">The text to escape.</param>
    /// <returns>The escaped text.</returns>
    public static string EscapeText(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        if (text.IndexOf('&') < 0)
        {
            return text.Trim();
        }

        return new System.Text.StringBuilder(text)
            .Replace("&amp;", "&")
            .Replace("&lt;", "<")
            .Replace("&gt;", ">")
            .Replace("&quot;", "\"")
            .Replace("&apos;", "'")
            .ToString()
            .Trim();
    }
}
