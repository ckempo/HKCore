using System.Collections.Generic;
using Newtonsoft.Json;
namespace HKCore
{
    [JsonObject("application")]
    public class Application
    {
        [JsonProperty("DirectoryConfig")]
        public List<DirectoryConfig> DirectoryConfig { get; set; }
    }
}