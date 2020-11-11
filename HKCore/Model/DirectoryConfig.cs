using Newtonsoft.Json;

namespace HKCore.Model
{
	public class DirectoryConfig
	{
		[JsonProperty("ConfigName")]
		public string ConfigName { get; set; } = string.Empty;

		[JsonProperty("Path")]
		public string Path { get; set; } = string.Empty;

		[JsonProperty("Mask")]
		public string Mask { get; set; } = string.Empty;

		[JsonProperty("DaysToKeep")]
		public int DaysToKeep { get; set; }

		[JsonProperty("IncludeSubDirs")]
		public bool IncludeSubDirs { get; set; }

		[JsonProperty("RemoveEmptyDirs")]
		public bool RemoveEmptyDirs { get; set; }
	}
}