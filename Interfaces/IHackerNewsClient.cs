using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using santander_hr_api.Models;

namespace santander_hr_api.Interfaces
{
    public interface IHackerNewsClient
    {
        Task<Story?> GetStoryByIdAsync(int id);
        Task<int[]> GetBestStoriesAsync();
    }
}
