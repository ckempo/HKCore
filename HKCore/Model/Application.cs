using System.Collections.Generic;
using Newtonsoft.Json;

namespace HKCore.Model
{
	[JsonObject("application")]
	public class Application
	{
		[JsonProperty("SimulationMode")]
		public bool SimulationMode { get; set; }

		[JsonProperty("DirectoryConfig")]
		public List<DirectoryConfig> DirectoryConfig { get; } = new List<DirectoryConfig>();
	}
}