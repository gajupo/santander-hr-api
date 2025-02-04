using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using santander_hr_api.Interfaces;
using santander_hr_api.Models;
using Serilog;

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

            var storyTasks = bestStories
                .Take(count)
                .Select(id => _hackerNewsClient.GetStoryByIdAsync(id));

            // get details for each story in parallel
            var stories = await Task.WhenAll(storyTasks);

            var noNullStories = stories
                .Where(story => story != null)
                .OrderByDescending(story => story!.Score);

            return _mapper.Map<IEnumerable<StoryDto>>(noNullStories);
        }
    }
}
