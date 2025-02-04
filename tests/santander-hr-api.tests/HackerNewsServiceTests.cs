using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using santander_hr_api.Interfaces;
using santander_hr_api.Models;
using santander_hr_api.Services;

namespace santander_hr_api.tests
{
    [TestFixture]
    public class HackerNewsServiceTests
    {
        private Mock<IHackerNewsClient> _mockClient;
        private Mock<ILogger<HackerNewsService>> _mockLogger;
        private Mock<IMapper> _mockMapper;
        private HackerNewsService _service;

        [SetUp]
        public void Setup()
        {
            _mockClient = new Mock<IHackerNewsClient>();
            _mockLogger = new Mock<ILogger<HackerNewsService>>();
            _mockMapper = new Mock<IMapper>();
            _service = new HackerNewsService(
                _mockClient.Object,
                _mockLogger.Object,
                _mockMapper.Object
            );
        }

        [Test]
        public async Task GetBestStoriesAsync_ReturnsCorrectNumberOfStories()
        {
            // Arrange
            var storyIds = new[] { 1, 2, 3 };
            var stories = new[]
            {
                new Story { Id = 1, Score = 100 },
                new Story { Id = 2, Score = 200 },
                new Story { Id = 3, Score = 300 }
            };
            var expectedDtos = new[]
            {
                new StoryDto { Score = 300 },
                new StoryDto { Score = 200 },
                new StoryDto { Score = 100 }
            };

            _mockClient.Setup(x => x.GetBestStoriesAsync()).ReturnsAsync(storyIds);
            _mockClient
                .Setup(x => x.GetStoryByIdAsync(It.IsAny<int>()))
                .Returns<int>(id => Task.FromResult(stories.FirstOrDefault(s => s.Id == id)));
            _mockMapper
                .Setup(x => x.Map<IEnumerable<StoryDto>>(It.IsAny<IEnumerable<Story>>()))
                .Returns(expectedDtos);

            // Act
            var result = await _service.GetBestStoriesAsync(3);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(3));
            _mockLogger.Verify(
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
        public async Task GetBestStoriesAsync_OrdersStoriesByScore()
        {
            // Arrange
            var storyIds = new[] { 1, 2 };
            var stories = new[]
            {
                new Story { Id = 1, Score = 100 },
                new Story { Id = 2, Score = 200 }
            };
            var expectedDtos = new[]
            {
                new StoryDto { Score = 200 },
                new StoryDto { Score = 100 }
            };

            _mockClient.Setup(x => x.GetBestStoriesAsync()).ReturnsAsync(storyIds);
            _mockClient
                .Setup(x => x.GetStoryByIdAsync(It.IsAny<int>()))
                .Returns<int>(id => Task.FromResult(stories.FirstOrDefault(s => s.Id == id)));
            _mockMapper
                .Setup(x => x.Map<IEnumerable<StoryDto>>(It.IsAny<IEnumerable<Story>>()))
                .Returns(expectedDtos);

            // Act
            var result = await _service.GetBestStoriesAsync(2);

            // Assert
            Assert.That(result.First().Score, Is.EqualTo(200));
        }

        [Test]
        public async Task GetBestStoriesAsync_FiltersNullStories()
        {
            // Arrange
            var storyIds = new[] { 1, 2, 3 };
            _mockClient.Setup(x => x.GetBestStoriesAsync()).ReturnsAsync(storyIds);
            _mockClient
                .Setup(x => x.GetStoryByIdAsync(It.Is<int>(id => id == 2)))
                .ReturnsAsync((Story)null);
            _mockClient
                .Setup(x => x.GetStoryByIdAsync(It.Is<int>(id => id != 2)))
                .ReturnsAsync(new Story { Score = 100 });

            // Act
            await _service.GetBestStoriesAsync(3);

            // Assert
            _mockMapper.Verify(
                x =>
                    x.Map<IEnumerable<StoryDto>>(
                        It.Is<IEnumerable<Story>>(stories => stories.Count() == 2)
                    ),
                Times.Once
            );
        }

        [Test]
        public async Task GetBestStoriesAsync_HandlesEmptyResponse()
        {
            // Arrange
            _mockClient.Setup(x => x.GetBestStoriesAsync()).ReturnsAsync(Array.Empty<int>());

            // Act
            var result = await _service.GetBestStoriesAsync(5);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetBestStoriesAsync_HandlesConcurrentOperations()
        {
            // Arrange
            var storyIds = Enumerable.Range(1, 100).ToArray();
            var stories = storyIds
                .Select(id => new Story { Id = id, Score = (short)(id * 10) })
                .ToArray();

            _mockClient.Setup(x => x.GetBestStoriesAsync()).ReturnsAsync(storyIds);
            _mockClient
                .Setup(x => x.GetStoryByIdAsync(It.IsAny<int>()))
                .Returns<int>(id => Task.FromResult(stories.FirstOrDefault(s => s.Id == id)));

            // Act
            var result = await _service.GetBestStoriesAsync(50);

            // Assert
            _mockClient.Verify(x => x.GetStoryByIdAsync(It.IsAny<int>()), Times.Exactly(50));
        }

        [Test]
        public void GetBestStoriesAsync_ThrowsException_WhenApiCallFails()
        {
            // Arrange
            _mockClient
                .Setup(x => x.GetBestStoriesAsync())
                .ThrowsAsync(new Exception("API Error"));

            // Act & Assert
            Assert.ThrowsAsync<Exception>(() => _service.GetBestStoriesAsync(5));
        }
    }
}
