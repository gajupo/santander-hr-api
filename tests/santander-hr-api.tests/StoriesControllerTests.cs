using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using santander_hr_api.Controllers;
using santander_hr_api.Interfaces;
using santander_hr_api.Models;

namespace santander_hr_api.tests
{
    public class StoriesControllerTests
    {
        private Mock<IHackerNewsService> _mockHackerNewsService;
        private Mock<ILogger<StoriesController>> _mockLogger;
        private StoriesController _controller;
        private List<StoryDto> _testStories;

        [SetUp]
        public void Setup()
        {
            _mockHackerNewsService = new Mock<IHackerNewsService>();
            _mockLogger = new Mock<ILogger<StoriesController>>();
            _controller = new StoriesController(_mockHackerNewsService.Object, _mockLogger.Object);

            _testStories = new List<StoryDto>
            {
                new StoryDto
                {
                    Title = "Test Story 1",
                    Score = 100,
                    PostedBy = "user1",
                    Url = "http://test1.com",
                    Time = 1234567890,
                    CommentCount = 10
                }
            };
        }

        [Test]
        public async Task Get_WithValidCount_ReturnsOkWithStories()
        {
            // Arrange
            _mockHackerNewsService
                .Setup(x => x.GetBestStoriesAsync(It.IsAny<short>()))
                .ReturnsAsync(_testStories);

            // Act
            var result = await _controller.Get(10);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            var stories = okResult.Value as IEnumerable<StoryDto>;
            Assert.That(stories, Is.EqualTo(_testStories));
        }

        [Test]
        public async Task Get_WithNegativeCount_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Get(-1);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            // the result is an action result, so we need to cast it to ObjectResult to access the StatusCode property
            var objectResult = result.Result as ObjectResult;
            var returnedValue = objectResult?.Value as ProblemDetails;
            Assert.That(returnedValue?.Title, Is.EqualTo("Count must be greater than 0"));
            Assert.That(objectResult?.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task Get_WhenServiceThrowsHttpException_ReturnsCorrectStatusCode()
        {
            // Arrange
            var httpException = new HttpRequestException(
                "API Error",
                null,
                HttpStatusCode.BadGateway
            );
            _mockHackerNewsService
                .Setup(x => x.GetBestStoriesAsync(It.IsAny<short>()))
                .ThrowsAsync(httpException);

            // Act
            var result = await _controller.Get(10);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            var objectResult = result.Result as ObjectResult;
            Assert.That(objectResult.StatusCode, Is.EqualTo((int)HttpStatusCode.BadGateway));
        }

        [Test]
        public async Task Get_WhenServiceThrowsException_Returns500()
        {
            // Arrange
            _mockHackerNewsService
                .Setup(x => x.GetBestStoriesAsync(It.IsAny<short>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.Get(10);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
            var objectResult = result.Result as ObjectResult;
            Assert.That(objectResult.StatusCode, Is.EqualTo(500));
        }
    }
}
