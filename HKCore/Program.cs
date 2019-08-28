using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace HKCore
{
	internal class Program
	{
		private static int totalFilesCount = 0;
		private static int totalDirsCount = 0;
		private static bool isSimulation = false;

		private static void Main(string[] args)
		{
			const string CONFIGFILE = "appsettings.json";
			IConfigurationBuilder configBuilder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile(CONFIGFILE);

			IConfigurationRoot config = configBuilder.Build();

			Application appConfig = config.GetSection("application").Get<Application>();
			isSimulation = appConfig.SimulationMode;

			if (isSimulation)
			{
				Console.WriteLine("Simulation mode is ENABLED; nothing will be deleted.");
			}

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			Parallel.ForEach<DirectoryConfig>(appConfig.DirectoryConfig, dirConfig =>
			{
				var logger = new StringBuilder();
				var fileCount = 0;
				var dirCount = 0;

				var configStopwatch = new Stopwatch();
				configStopwatch.Start();

				logger.AppendLine($"Reading config for {dirConfig.ConfigName}");

				// Process each directory in config
				ProcessDirResult counter = ProcessDir(dirConfig, logger);
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
			Console.WriteLine("     {0} files removed ", totalFilesCount);
			Console.WriteLine("     {0} directories removed", totalDirsCount);
			Console.WriteLine("     Took {0} seconds", stopwatch.Elapsed.Seconds.ToString());
		}

		private static ProcessDirResult ProcessDir(DirectoryConfig dirConf, StringBuilder logger)
		{
			ProcessDirResult result = ProcessDir(dirConf.Path,
					   dirConf.Mask,
					   dirConf.DaysToKeep,
					   dirConf.IncludeSubDirs,
					   dirConf.RemoveEmptyDirs,
					   logger);

			return result;
		}

		private static ProcessDirResult ProcessDir(string dirPath, string mask, int daysToKeep, bool doSubDirs, bool removeEmptyDirs, StringBuilder logger)
		{
			var delFilesCount = 0;
			var delDirsCount = 0;
			DateTime nowDate = DateTime.Now;
			var di = new DirectoryInfo(dirPath);

			Parallel.ForEach(di.GetDirectories(), dir =>
			{
				ProcessDirResult result = ProcessDir(dir.FullName, mask, daysToKeep, doSubDirs, removeEmptyDirs, logger);
				Interlocked.Add(ref delFilesCount, result.RemovedFilesCount);
				Interlocked.Add(ref delDirsCount, result.RemovedDirsCount);

				//Don't leave behind empty dirs if configured to remove them
				if (Directory.GetFileSystemEntries(dir.FullName).Length == 0 && removeEmptyDirs)
				{
					if (!isSimulation)
					{
						Directory.Delete(dir.FullName);
					}

					logger.AppendLine($"Deleting directory {dir.FullName}");
					Interlocked.Increment(ref delDirsCount);
					Interlocked.Increment(ref totalDirsCount);
				}
			});

			foreach (FileInfo file in di.EnumerateFiles(mask, SearchOption.AllDirectories))
			{
				if (file.LastWriteTime.Date < nowDate.Date)
				{
					try
					{
						if (!isSimulation)
						{
							file.Delete();
						}

						logger.AppendLine($"Deleting {file.FullName}");
						Interlocked.Increment(ref delFilesCount);
						Interlocked.Increment(ref totalFilesCount);
					}
					catch (System.IO.IOException ex)
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

	public class ProcessDirResult
	{
		public int RemovedFilesCount { get; set; }
		public int RemovedDirsCount { get; set; }
	}
}
