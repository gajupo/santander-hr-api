using System.Net;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using santander_hr_api.Infraestructure;
using santander_hr_api.Models;

namespace santander_hr_api.tests
{
    [TestFixture]
    public class HackerNewsClientTests
    {
        private Mock<ILogger<HackerNewsClient>> _loggerMock;
        private IMemoryCache _cacheMock = null!;
        private IConfiguration _configurationMock;
        private HackerNewsClient _sut;
        private const string BaseUrl = "https://hacker-news.firebaseio.com/v0/";

        private string LoadBestStoriesJson()
        {
            return File.ReadAllText("beststories.json");
        }

        private (IHttpClientFactory, Mock<HttpMessageHandler>) MockIHttpClientFactory(
            HttpStatusCode statusCode,
            string content
        )
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var response = new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response)
                .Verifiable();

            var client = new HttpClient(handlerMock.Object) { BaseAddress = new Uri(BaseUrl) };

            var factory = new Mock<IHttpClientFactory>();
            factory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

            return (factory.Object, handlerMock);
        }

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<HackerNewsClient>>();
            _cacheMock = new MemoryCache(new MemoryCacheOptions());

            var inMemorySettings = new Dictionary<string, string> { { "StoryCacheMinutes", "5" } };
            _configurationMock = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var bestStoriesJson = LoadBestStoriesJson();
            var httpClientFactory = MockIHttpClientFactory(HttpStatusCode.OK, bestStoriesJson);

            _sut = new HackerNewsClient(
                httpClientFactory.Item1,
                _loggerMock.Object,
                _cacheMock,
                _configurationMock
            );
        }

        [Test]
        public async Task GetBestStoriesAsync_ShouldReturnStories_WhenApiCallSucceeds()
        {
            // Act
            var result = await _sut.GetBestStoriesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.EqualTo(200));
        }

        [Test]
        public async Task GetStoryByIdAsync_ShouldReturnStory_WhenApiCallSucceeds()
        {
            // Arrange
            var storyId = 8863;
            var storyJson = File.ReadAllText("story.json");
            var httpClientFactory = MockIHttpClientFactory(HttpStatusCode.OK, storyJson);

            _sut = new HackerNewsClient(
                httpClientFactory.Item1,
                _loggerMock.Object,
                _cacheMock,
                _configurationMock
            );

            // Act
            var result = await _sut.GetStoryByIdAsync(storyId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(storyId));
        }

        [Test]
        public async Task GetStoryByIdAsync_CachesResult()
        {
            // Arrange
            var storyId = 8863;
            var storyJson = File.ReadAllText("story.json");
            var httpClientFactory = MockIHttpClientFactory(HttpStatusCode.OK, storyJson);

            _sut = new HackerNewsClient(
                httpClientFactory.Item1,
                _loggerMock.Object,
                _cacheMock,
                _configurationMock
            );

            // Act
            var result = await _sut.GetStoryByIdAsync(storyId);
            var cachedResult = _cacheMock.Get<Story>($"STORY_{storyId}");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(cachedResult, Is.Not.Null);
            Assert.That(cachedResult.Id, Is.EqualTo(storyId));
            // verify the logger was called
            _loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        It.IsAny<Exception>(),
                        (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                    ),
                Times.Once
            );
        }

        [Test]
        public async Task GetStoryByIdAsync_ShouldUseCache_WhenStoryIsCached()
        {
            // Arrange
            var httpClientFactory = MockIHttpClientFactory(HttpStatusCode.OK, "[]");
            var storyId = 42797260;
            var cacheKey = $"STORY_{storyId}";
            var cachedStory = new Story
            {
                Id = storyId,
                Title = "Cached Story",
                Score = 100
            };
            // the story is added to the cache
            _cacheMock.Set(cacheKey, cachedStory, TimeSpan.FromMinutes(5));

            _sut = new HackerNewsClient(
                httpClientFactory.Item1,
                _loggerMock.Object,
                _cacheMock,
                _configurationMock
            );

            // Act
            var result = await _sut.GetStoryByIdAsync(storyId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(storyId));
            httpClientFactory
                .Item2.Protected()
                .Verify(
                    "SendAsync",
                    Times.Never(),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                );
        }

        [TearDown]
        public void TearDown()
        {
            //dispose cached data
            _cacheMock.Dispose();
        }
    }
}
