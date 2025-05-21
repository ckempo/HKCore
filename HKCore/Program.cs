using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HKCore.Model;
using Microsoft.Extensions.Configuration;

namespace HKCore
{
    internal class Program
    {
        const string CONFIGFILE = "appsettings.json";
        private static int _totalFilesCount;
        private static int _totalDirsCount;
        private static bool _isSimulation;

        private static void Main(string[] args)
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(CONFIGFILE);

            var config = configBuilder.Build();

            var appConfig = config.GetSection("application").Get<Application>();
            if (appConfig == null)
                return;

            _isSimulation = appConfig.SimulationMode;

            if (_isSimulation)
            {
                Console.WriteLine("Simulation mode is ENABLED; nothing will be deleted.");
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Parallel.ForEach(appConfig.DirectoryConfig, dirConfig =>
            {
                var logger = new StringBuilder();
                var fileCount = 0;
                var dirCount = 0;

                var configStopwatch = new Stopwatch();
                configStopwatch.Start();

                logger.AppendLine($"Reading config for {dirConfig.ConfigName}");

                // Process each directory in config
                var counter = ProcessDir(dirConfig, logger);
                fileCount += counter.RemovedFilesCount;
                dirCount += counter.RemovedDirsCount;

                configStopwatch.Stop();

                // Report totals
                logger.AppendLine($"Completed processing {dirConfig.ConfigName}:");
                logger.AppendLine($"{fileCount.ToString()} files removed");
                logger.AppendLine($"{dirCount.ToString()} directories removed");
                logger.AppendLine($"Took {configStopwatch.Elapsed.Seconds.ToString()} seconds");

                Console.WriteLine(logger.ToString());
            });

            stopwatch.Stop();
            Console.WriteLine("Completed processing - total deletions:");
            Console.WriteLine("     {0} files removed ", _totalFilesCount);
            Console.WriteLine("     {0} directories removed", _totalDirsCount);
            Console.WriteLine("     Took {0} seconds", stopwatch.Elapsed.Seconds.ToString());

            if (_isSimulation)
            {
                Console.WriteLine("(Simulation mode was ENABLED; nothing was deleted really.)");
            }
        }

        private static ProcessDirResult ProcessDir(DirectoryConfig dirConf, StringBuilder logger)
        {
            var result = ProcessDir(dirConf.Path,
                dirConf.Mask,
                dirConf.DaysToKeep,
                dirConf.IncludeSubDirs,
                dirConf.RemoveEmptyDirs,
                logger);

            return result;
        }

        private static ProcessDirResult ProcessDir(string dirPath, string mask, int daysToKeep, bool doSubDirs,
            bool removeEmptyDirs, StringBuilder logger)
        {
            var delFilesCount = 0;
            var delDirsCount = 0;
            var nowDate = DateTime.Now;
            var di = new DirectoryInfo(dirPath);

            Parallel.ForEach(di.GetDirectories(), dir =>
            {
                var result = ProcessDir(dir.FullName, mask, daysToKeep, doSubDirs, removeEmptyDirs, logger);
                Interlocked.Add(ref delFilesCount, result.RemovedFilesCount);
                Interlocked.Add(ref delDirsCount, result.RemovedDirsCount);

                //Don't leave behind empty dirs if configured to remove them
                if (Directory.GetFileSystemEntries(dir.FullName).Length == 0 && removeEmptyDirs)
                {
                    if (!_isSimulation)
                    {
                        Directory.Delete(dir.FullName);
                    }

                    logger.AppendLine($"Deleting directory {dir.FullName}");
                    Interlocked.Increment(ref delDirsCount);
                    Interlocked.Increment(ref _totalDirsCount);
                }
            });

            foreach (var file in di.EnumerateFiles(mask, SearchOption.AllDirectories))
            {
                if (file.LastWriteTime.Date < nowDate.Date)
                {
                    try
                    {
                        if (!_isSimulation)
                        {
                            file.Delete();
                        }

                        logger.AppendLine($"Deleting {file.FullName}");
                        Interlocked.Increment(ref delFilesCount);
                        Interlocked.Increment(ref _totalFilesCount);
                    }
                    catch (IOException ex)
                    {
                        logger.AppendLine($"IOException: {ex}");
                    }
                    catch (Exception ex)
                    {
                        logger.AppendLine($"Unhandled Exception: {ex}");
                    }
                }
            }

            return new ProcessDirResult { RemovedFilesCount = delFilesCount, RemovedDirsCount = delDirsCount };
        }
    }
}
