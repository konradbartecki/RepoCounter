using Newtonsoft.Json;
using System.Collections.Generic;

namespace Me.Bartecki.RepoCounter.Domain.Model
{
    public class UserStatistics
    {
        public string Owner { get; set; }

        public Dictionary<char, int> Letters { get; set; } = new Dictionary<char, int>();

        [JsonProperty("avgStargazers")]
        public double AverageStargazers { get; set; }

        [JsonProperty("avgWatchers")]
        public double AverageWatchers { get; set; }

        [JsonProperty("avgForks")]
        public double AverageForks { get; set; }

        [JsonProperty("avgSize")]
        public double AverageSize { get; set; }
    }
}