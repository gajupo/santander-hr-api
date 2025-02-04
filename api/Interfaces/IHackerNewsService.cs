using santander_hr_api.Models;

namespace santander_hr_api.Interfaces
{
    public interface IHackerNewsService
    {
        Task<IEnumerable<StoryDto>> GetBestStoriesAsync(short count);
    }
}
