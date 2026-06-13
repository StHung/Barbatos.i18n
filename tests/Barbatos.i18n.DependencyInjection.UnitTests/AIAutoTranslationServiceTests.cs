using System.Globalization;
using Barbatos.i18n.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OpenAI;
using System.ClientModel;

namespace Barbatos.i18n.DependencyInjection.UnitTests;

public class AIAutoTranslationServiceTests
{
    [Fact]
    public async Task TranslateAndSaveAsync_WithRealModel_ShouldTranslateAndSave()
    {
        // Arrange
        // Read API key from environment variable
        string apiKey = Environment.GetEnvironmentVariable("AI_API_KEY") 
                     ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY") 
                     ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY") 
                     ?? "";
        
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            // If no API key is provided, log warning and exit successfully rather than failing the build
            Console.WriteLine("WARNING: AI_API_KEY is not set. Skipping real model translation test.");
            return;
        }

        // Setup real Gemini client via OpenAI Chat Client
        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri("https://generativelanguage.googleapis.com/v1beta/openai/")
        };
        var openAiChatClient = new OpenAI.Chat.ChatClient("gemini-2.5-flash", new ApiKeyCredential(apiKey), options);
        var chatClient = openAiChatClient.AsIChatClient();

        var services = new ServiceCollection();
        var providerMock = new Mock<ILocalizationProvider>();
        var set = new LocalizationSet("vi-VN", new CultureInfo("vi-VN"), new Dictionary<LocalizationKey, string?>());
        
        providerMock.Setup(p => p.GetLocalizationSet(It.IsAny<CultureInfo>(), It.IsAny<string>()))
            .Returns(set);
        
        // Setup LocalizationProviderFactory
        LocalizationProviderFactory.SetInstance(providerMock.Object, "TestProvider");

        // Mock writer
        var writerMock = new Mock<ILocalizationResourceWriter>();
        writerMock.Setup(w => w.WriteAsync(It.IsAny<CultureInfo>(), It.IsAny<string>(), It.IsAny<LocalizationKey>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
            
        services.AddSingleton(writerMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        var translationService = new AIAutoTranslationService(chatClient, serviceProvider);

        // Act
        var result = await translationService.TranslateAndSaveAsync(
            "TestProvider", 
            "GreetingWithName", 
            null, 
            "A greeting that takes a name as a parameter", 
            new CultureInfo("vi-VN"), 
            new object[] { "John" });

        // Assert
        Assert.NotNull(result);
        Assert.Contains("{0}", result); // Translation should maintain the {0} placeholder
        
        // Ensure the string was written to writer
        writerMock.Verify(w => w.WriteAsync(
            It.Is<CultureInfo>(c => c.Name == "vi-VN"), 
            "vi-VN", 
            It.Is<LocalizationKey>(k => k.Equals(new LocalizationKey("GreetingWithName"))), 
            result), Times.Once);
            
        // Ensure the string was added to memory set
        Assert.Equal(result, set[new LocalizationKey("GreetingWithName")]);
    }
}
