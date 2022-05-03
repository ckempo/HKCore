using System.Collections.Generic;

namespace HKCore.Model
{
    public class Application
    {
        public bool SimulationMode { get; set; }
        public List<DirectoryConfig> DirectoryConfig { get; } = new List<DirectoryConfig>();
    }
}