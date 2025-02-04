using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace santander_hr_api.Models
{
    public class StoryDto
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string PostedBy { get; set; }
        public short Score { get; set; }
        public int Time { get; set; }
        public short CommentCount { get; set; }
    }
}
