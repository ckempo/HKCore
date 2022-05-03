namespace HKCore.Model
{
    public class DirectoryConfig
    {
        public string ConfigName { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Mask { get; set; } = string.Empty;
        public int DaysToKeep { get; set; }
        public bool IncludeSubDirs { get; set; }
        public bool RemoveEmptyDirs { get; set; }
    }
}