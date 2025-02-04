using System;
using System.Collections.Concurrent;
using AutoMapper;
using santander_hr_api.Interfaces;
using santander_hr_api.Models;

namespace santander_hr_api.Services
{
    public class HackerNewsService : IHackerNewsService
    {
        private readonly IHackerNewsClient _hackerNewsClient;
        private readonly ILogger<HackerNewsService> _logger;
        private readonly IMapper _mapper;

        public HackerNewsService(
            IHackerNewsClient hackerNewsClient,
            ILogger<HackerNewsService> logger,
            IMapper mapper
        )
        {
            _hackerNewsClient = hackerNewsClient;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IEnumerable<StoryDto>> GetBestStoriesAsync(short count)
        {
            // get list of best stories
            var bestStories = await _hackerNewsClient.GetBestStoriesAsync();
            _logger.LogInformation("Retrieved {Count} best stories", bestStories.Length);

            count = Math.Min(count, (short)bestStories.Length);
            var stories = new ConcurrentBag<Story>();

            await Parallel.ForEachAsync(
                bestStories.Take(count),
                async (id, cancellationToken) =>
                {
                    var story = await _hackerNewsClient.GetStoryByIdAsync(id);
                    if (story != null)
                    {
                        stories.Add(story);
                    }
                }
            );

            var noNullStories = stories.OrderByDescending(story => story.Score);

            return _mapper.Map<IEnumerable<StoryDto>>(noNullStories);
        }
    }
}
