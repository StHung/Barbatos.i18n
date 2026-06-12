using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Barbatos.i18n;
using Barbatos.i18n.Mcp;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;

namespace Barbatos.i18n.Mcp.UnitTests;

public class McpServerLocalizationExtensionsTests
{
    [Fact]
    public void AddLocalizationTools_ShouldReturnBuilder()
    {
        var services = new ServiceCollection();
        var builder = services.AddMcpServer(options => { options.ServerInfo = new ModelContextProtocol.Protocol.Implementation { Name = "Test", Version = "1" }; });

        var result = builder.AddLocalizationTools();

        Assert.NotNull(result);
    }
}
