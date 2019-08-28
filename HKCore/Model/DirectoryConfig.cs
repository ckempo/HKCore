using Newtonsoft.Json;
namespace HKCore
{
    public class DirectoryConfig
    {
        [JsonProperty("ConfigName")]
        public string ConfigName { get; set; }

        [JsonProperty("Path")]
        public string Path { get; set; }

        [JsonProperty("Mask")]
        public string Mask { get; set; }

        [JsonProperty("DaysToKeep")]
        public int DaysToKeep { get; set; }

        [JsonProperty("IncludeSubDirs")]
        public bool IncludeSubDirs { get; set; }

        [JsonProperty("RemoveEmptyDirs")]
        public bool RemoveEmptyDirs { get; set; }
    }
}