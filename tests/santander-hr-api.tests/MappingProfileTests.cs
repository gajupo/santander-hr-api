using AutoMapper;
using santander_hr_api.Config;
using santander_hr_api.Models;

namespace santander_hr_api.tests
{
    public class MappingProfileTests
    {
        private IMapper _mapper;

        [SetUp]
        public void Setup()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = config.CreateMapper();
        }

        [Test]
        public void StoryToStoryDtoMapping_ShouldMapCommentCount()
        {
            // Arrange
            var story = new Story
            {
                Id = 1,
                Title = "Test Story",
                Kids = new List<int> { 1, 2, 3 },
                Score = 10,
                By = "testuser",
                Time = 123456789,
                Url = "https://test.com"
            };

            // Act
            var dto = _mapper.Map<StoryDto>(story);

            // Assert
            Assert.That(dto.CommentCount, Is.EqualTo(3));
            Assert.That(dto.Title, Is.EqualTo(story.Title));
            Assert.That(dto.Score, Is.EqualTo(story.Score));
            Assert.That(dto.PostedBy, Is.EqualTo(story.By));
            Assert.That(dto.Time, Is.EqualTo(story.Time));
            Assert.That(dto.Url, Is.EqualTo(story.Url));
        }

        [Test]
        public void StoryToStoryDtoMapping_WithNullKids_ShouldMapToZeroComments()
        {
            // Arrange
            var story = new Story
            {
                Id = 1,
                Title = "Test Story",
                Kids = null
            };

            // Act
            var dto = _mapper.Map<StoryDto>(story);

            // Assert
            Assert.That(dto.CommentCount, Is.EqualTo(0));
        }
    }
}
