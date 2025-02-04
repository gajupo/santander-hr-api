using Microsoft.Extensions.Caching.Memory;
using santander_hr_api.Interfaces;
using santander_hr_api.Models;
using Serilog;

namespace santander_hr_api.Infraestructure
{
    public class HackerNewsClient : IHackerNewsClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HackerNewsClient> _logger;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;

        public HackerNewsClient(
            IHttpClientFactory httpClientFactory,
            ILogger<HackerNewsClient> logger,
            IMemoryCache cache,
            IConfiguration configuration
        )
        {
            _httpClient = httpClientFactory.CreateClient("hackerNews");
            _logger = logger;
            _cache = cache;
            _configuration = configuration;
        }

        public async Task<int[]> GetBestStoriesAsync()
        {
            var bestStoryIds = await _httpClient.GetFromJsonAsync<int[]>("beststories.json");

            return bestStoryIds ?? [];
        }

        public async Task<Story?> GetStoryByIdAsync(int id)
        {
            string storyCacheKey = $"STORY_{id}";

            // chek if the story is in the cache
            if (_cache.TryGetValue(storyCacheKey, out Story? story))
            {
                return story;
            }

            story = await _httpClient.GetFromJsonAsync<Story?>($"item/{id}.json");

            if (story != null)
            {
                _logger.LogInformation(
                    "Story {Id}, type: {Type}, title: {Title}",
                    story.Id,
                    story.Type,
                    story.Title
                );
                _cache.Set(
                    storyCacheKey,
                    story,
                    TimeSpan.FromMinutes(_configuration.GetValue<int>("StoryCacheMinutes"))
                );
            }

            return story;
        }
    }
}
