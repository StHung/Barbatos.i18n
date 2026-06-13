using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Barbatos.i18n;
using Barbatos.i18n.Mcp;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using System.Globalization;
using System.Collections.Generic;

namespace Barbatos.i18n.Mcp.UnitTests;

public class McpServerLocalizationExtensionsTests : IDisposable
{
    public McpServerLocalizationExtensionsTests()
    {
        // Reset the factory before each test
        LocalizationProviderFactory.SetInstance(null);
    }

    public void Dispose()
    {
        // Clean up after each test
        LocalizationProviderFactory.SetInstance(null);
    }

    [Fact]
    public void AddLocalizationTools_ShouldReturnBuilder()
    {
        var services = new ServiceCollection();
        var builder = services.AddMcpServer(options => { options.ServerInfo = new ModelContextProtocol.Protocol.Implementation { Name = "Test", Version = "1" }; });

        var result = builder.AddLocalizationTools();

        Assert.NotNull(result);
    }

    [Fact]
    public async Task ReadTranslationToolAsync_ShouldReturnError_WhenProviderNotInitialized()
    {
        var result = await McpServerLocalizationExtensions.ReadTranslationToolAsync("TestKey", "en-US");
        Assert.Contains("not initialized", result);
    }

    [Fact]
    public async Task ReadTranslationToolAsync_ShouldReturnError_WhenCultureNotFound()
    {
        var mockProvider = new Mock<ILocalizationProvider>();
        mockProvider.Setup(p => p.GetLocalizationSets(It.IsAny<CultureInfo>()))
                    .Returns((IEnumerable<LocalizationSet>)null!);
        LocalizationProviderFactory.SetInstance(mockProvider.Object);

        var result = await McpServerLocalizationExtensions.ReadTranslationToolAsync("TestKey", "fr-FR");
        Assert.Contains("Culture 'fr-FR' not found", result);
    }

    [Fact]
    public async Task ReadTranslationToolAsync_ShouldReturnError_WhenKeyNotFound()
    {
        var locSet = new LocalizationSet("Test", new CultureInfo("en-US"), new Dictionary<LocalizationKey, string?>());

        var mockProvider = new Mock<ILocalizationProvider>();
        mockProvider.Setup(p => p.GetLocalizationSets(It.IsAny<CultureInfo>()))
                    .Returns(new[] { locSet });
        
        LocalizationProviderFactory.SetInstance(mockProvider.Object);

        var result = await McpServerLocalizationExtensions.ReadTranslationToolAsync("NonExistentKey", "en-US");
        Assert.Contains("Key 'NonExistentKey' not found", result);
    }

    [Fact]
    public async Task ReadTranslationToolAsync_ShouldReturnTranslation_WhenKeyFound()
    {
        var dict = new Dictionary<LocalizationKey, string?>
        {
            { new LocalizationKey("Greeting"), "Hello World" }
        };
        var locSet = new LocalizationSet("Test", new CultureInfo("en-US"), dict);

        var mockProvider = new Mock<ILocalizationProvider>();
        mockProvider.Setup(p => p.GetLocalizationSets(It.IsAny<CultureInfo>()))
                    .Returns(new[] { locSet });
        
        LocalizationProviderFactory.SetInstance(mockProvider.Object);

        var result = await McpServerLocalizationExtensions.ReadTranslationToolAsync("Greeting", "en-US");
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public async Task GetCulturesToolAsync_ShouldReturnError_WhenProviderNotInitialized()
    {
        var result = await McpServerLocalizationExtensions.GetCulturesToolAsync();
        Assert.Contains("not initialized", result);
    }

    [Fact]
    public async Task GetCulturesToolAsync_ShouldReturnAvailableCultures()
    {
        var locSet1 = new LocalizationSet("Test1", new CultureInfo("en-US"), new Dictionary<LocalizationKey, string?>());
        var locSet2 = new LocalizationSet("Test2", new CultureInfo("vi-VN"), new Dictionary<LocalizationKey, string?>());

        var mockProvider = new Mock<ILocalizationProvider>();
        mockProvider.Setup(p => p.GetLocalizationSets())
                    .Returns(new[] { locSet1, locSet2 });
        
        LocalizationProviderFactory.SetInstance(mockProvider.Object);

        var result = await McpServerLocalizationExtensions.GetCulturesToolAsync();
        Assert.Contains("en-US", result);
        Assert.Contains("vi-VN", result);
    }

    [Fact]
    public async Task AutomateTranslationToolAsync_ShouldReturnInstruction()
    {
        var result = await McpServerLocalizationExtensions.AutomateTranslationToolAsync("Hello", "en-US", "vi-VN");
        Assert.Contains("Instruction for LLM", result);
        Assert.Contains("Hello", result);
        Assert.Contains("en-US", result);
        Assert.Contains("vi-VN", result);
    }
}
