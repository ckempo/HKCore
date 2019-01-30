using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Binder;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace HKCore
{
    class Program
    {
        static int totalFilesCount = 0;
        static int totalDirsCount = 0;

        static void Main(string[] args)
        {
            const string CONFIGFILE = "appsettings.json";
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(CONFIGFILE);

            var config = configBuilder.Build();

            var appConfig = config.GetSection("application").Get<Application>();

            foreach (DirectoryConfig dirConfig in appConfig.DirectoryConfig)
            {
                int fileCount = 0;
                int dirCount = 0;

                Debug.WriteLine(string.Format("Reading config for {0}", dirConfig.ConfigName));

                // Process each directory in config
                var counter = ProcessDir(dirConfig);
                fileCount += counter.RemovedFilesCount;
                dirCount += counter.RemovedDirsCount;

                // Report totals  
                Console.WriteLine("Completed processing {0}:", dirConfig.ConfigName);
                Console.WriteLine("{0} files removed", fileCount.ToString());
                Console.WriteLine("{0} directories removed", dirCount.ToString());
            }

            Console.WriteLine("Completed processing - total deletions:");
            Console.WriteLine("     {0} files removed ", totalFilesCount);
            Console.WriteLine("     {0} directories removed", totalDirsCount);
        }

        static ProcessDirResult ProcessDir(DirectoryConfig dirConf)
        {
            var result = ProcessDir(dirConf.Path,
                       dirConf.Mask,
                       dirConf.DaysToKeep,
                       dirConf.IncludeSubDirs,
                       dirConf.RemoveEmptyDirs);

            return result;
        }
        static ProcessDirResult ProcessDir(string dirPath, string mask, int daysToKeep, bool doSubDirs, bool removeEmptyDirs)
        {
            int delFilesCount = 0;
            int delDirsCount = 0;
            DateTime nowDate = DateTime.Now;
            DirectoryInfo di = new DirectoryInfo(dirPath);

            Parallel.ForEach(di.GetDirectories(), dir =>
            {
                var result = ProcessDir(dir.FullName, mask, daysToKeep, doSubDirs, removeEmptyDirs);
                Interlocked.Add(ref delFilesCount, result.RemovedFilesCount);
                Interlocked.Add(ref delDirsCount, result.RemovedDirsCount);

                //Don't leave behind empty dirs if configured to remove them
                if (Directory.GetFileSystemEntries(dir.FullName).Length == 0 && removeEmptyDirs)
                {
                    //Directory.Delete(dir.FullName);
                    Console.WriteLine("Deleting directory {0}", dir.FullName);
                    Interlocked.Increment(ref delDirsCount);
                    Interlocked.Increment(ref totalDirsCount);
                }
            });

            foreach (var file in di.EnumerateFiles(mask, SearchOption.AllDirectories))
            {
                if (file.LastWriteTime.Date < nowDate.Date)
                {
                    try
                    {
                        //file.Delete();
                        Console.WriteLine("Deleting {0}", file.FullName);
                        Interlocked.Increment(ref delFilesCount);
                        Interlocked.Increment(ref totalFilesCount);
                    }
                    catch (System.IO.IOException ex)
                    {
                        Console.WriteLine("IOException: {0}", ex);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Unhandled Exception: {0}", ex);
                    }
                }
            }

            return new ProcessDirResult { RemovedFilesCount = delDirsCount, RemovedDirsCount = delDirsCount };
        }
    }

    public class ProcessDirResult
    {
        public int RemovedFilesCount { get; set; }
        public int RemovedDirsCount { get; set; }
    }
}
