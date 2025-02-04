using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace santander_hr_api.Models
{
    public class Story
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string By { get; set; }
        public string Url { get; set; }
        public short Score { get; set; }
        public int Time { get; set; }
        public string Type { get; set; }
        public List<int> Kids { get; set; }
    }
}
