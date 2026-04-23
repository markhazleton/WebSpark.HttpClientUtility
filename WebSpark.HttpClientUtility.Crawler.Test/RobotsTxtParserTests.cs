using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using WebSpark.HttpClientUtility.Crawler;

namespace WebSpark.HttpClientUtility.Test.Crawler;

[TestClass]
public class RobotsTxtParserTests
{
    private Mock<IHttpClientFactory>? _factory;
    private Mock<ILogger>? _logger;

    [TestInitialize]
    public void Setup()
    {
        _factory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        _logger = new Mock<ILogger>();
    }

    [TestMethod]
    public async Task ProcessRobotsTxtAsync_WithDisallowPrefix_BlocksMatchingPaths()
    {
        // Arrange
        var content = """
            User-agent: *
            Disallow: /private
            """;
        var parser = CreateParserWithContent(content);

        // Act
        await parser.ProcessRobotsTxtAsync("https://example.com/start");

        // Assert
        Assert.IsFalse(parser.IsAllowed("https://example.com/private/page"));
        Assert.IsTrue(parser.IsAllowed("https://example.com/public/page"));
    }

    [TestMethod]
    public async Task ProcessRobotsTxtAsync_WithWildcardPatterns_BlocksExpectedUrls()
    {
        // Arrange
        var content = """
            User-agent: *
            Disallow: /admin*
            Disallow: *.zip
            Disallow: /a*/b
            """;
        var parser = CreateParserWithContent(content);

        // Act
        await parser.ProcessRobotsTxtAsync("https://example.com/root");

        // Assert
        Assert.IsFalse(parser.IsAllowed("https://example.com/admin/settings"));
        Assert.IsFalse(parser.IsAllowed("https://example.com/download/file.zip"));
        Assert.IsFalse(parser.IsAllowed("https://example.com/a-test/b"));
        Assert.IsTrue(parser.IsAllowed("https://example.com/docs/readme.txt"));
    }

    [TestMethod]
    public async Task ProcessRobotsTxtAsync_WithSpecificUserAgent_UsesMatchingSection()
    {
        // Arrange
        var content = """
            User-agent: googlebot
            Disallow: /google-only

            User-agent: custombot
            Disallow: /custom-only
            """;
        var parser = CreateParserWithContent(content, "custombot");

        // Act
        await parser.ProcessRobotsTxtAsync("https://example.com/root");

        // Assert
        Assert.IsTrue(parser.IsAllowed("https://example.com/google-only/page"));
        Assert.IsFalse(parser.IsAllowed("https://example.com/custom-only/page"));
    }

    [TestMethod]
    public async Task ProcessRobotsTxtAsync_WhenDomainAlreadyProcessed_DoesNotFetchAgain()
    {
        // Arrange
        var callCount = 0;
        var handler = new DelegateHandler((_, _) =>
        {
            callCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("User-agent: *\nDisallow: /private")
            });
        });
        var client = new HttpClient(handler);

        _factory!
            .Setup(f => f.CreateClient("RobotsTxtParser"))
            .Returns(client);

        var parser = new RobotsTxtParser(_factory.Object, "custombot", _logger!.Object);

        // Act
        await parser.ProcessRobotsTxtAsync("https://example.com/one");
        await parser.ProcessRobotsTxtAsync("https://example.com/two");

        // Assert
        Assert.AreEqual(1, callCount);
    }

    [TestMethod]
    public async Task ProcessRobotsTxtAsync_WhenFetchFails_AllowsUrl()
    {
        // Arrange
        var handler = new DelegateHandler((_, _) => throw new HttpRequestException("network failure"));
        var client = new HttpClient(handler);

        _factory!
            .Setup(f => f.CreateClient("RobotsTxtParser"))
            .Returns(client);

        var parser = new RobotsTxtParser(_factory.Object, "custombot", _logger!.Object);

        // Act
        await parser.ProcessRobotsTxtAsync("https://example.com/root");

        // Assert
        Assert.IsTrue(parser.IsAllowed("https://example.com/private/page"));
    }

    private RobotsTxtParser CreateParserWithContent(string robotsContent, string userAgent = "custombot")
    {
        var handler = new DelegateHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(robotsContent)
        }));
        var client = new HttpClient(handler);

        _factory!
            .Setup(f => f.CreateClient("RobotsTxtParser"))
            .Returns(client);

        return new RobotsTxtParser(_factory.Object, userAgent, _logger!.Object);
    }

    private sealed class DelegateHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return handler(request, cancellationToken);
        }
    }
}
