using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace SimpleBackupper
{
    class Program
    {
        static void Main(string[] args)
        {
            try {
                var targetBaseDir = ConfigurationManager.AppSettings["TargetBaseDir"];
                if (string.IsNullOrWhiteSpace(targetBaseDir))
                {
                    throw new Exception("targetBaseDir should be configured");
                }
                var systemName = ConfigurationManager.AppSettings["SystemName"];
                if (string.IsNullOrWhiteSpace(systemName))
                {
                    throw new Exception("systemName should be configured");
                }
                var dateString = DateTime.Now.Date.ToString("yyyy-MM-dd");

                var sourceDirs = GetSourceDirs();
                foreach (var sourceDir in sourceDirs)
                {
                    if (!sourceDir.Exists)
                    {
                        throw new Exception(string.Format("source dir not found: {0}", sourceDir.FullName));
                    }
                }

                foreach (var sourceDir in sourceDirs)
                {                    
                    Console.WriteLine(string.Format("detecting files in dir {0}", sourceDir));
                    var files = sourceDir.GetFiles("*", SearchOption.AllDirectories);
                    var fileCount = (float)files.Count();
                    Console.WriteLine(string.Format("detected {1} files in dir {0}", sourceDir, fileCount));

                    var consoleTopAtStart = Console.CursorTop;
                    Console.WriteLine();
                    for (int i = 1; i < fileCount; i++)
                    {
                        var file = files[i];
                        var fullOriginalPath = file.FullName;
                        var targetDirString = string.Format(@"{0}{1}\{2}\", targetBaseDir, systemName, dateString);
                        var fullTargetPath = fullOriginalPath.Replace(sourceDir.FullName, targetDirString);
                        var targetDir = new DirectoryInfo(Path.GetDirectoryName(fullTargetPath));
                        if (!targetDir.Exists)
                        {
                            targetDir.Create();
                        }

                        Console.SetCursorPosition(0, consoleTopAtStart);
                        var percentage = 0F;
                        if (fileCount != 0 && i != 0)
                        {
                            percentage = (i / fileCount) * 100;
                        }
                        Console.Write(string.Format("{0}%", percentage.ToString("0.00")));
                        if (new FileInfo(fullTargetPath).Exists)
                        {
                            WriteConsoleStatusLine(consoleTopAtStart + 1, string.Format("copying {0}/{1} File already exists on target location, skipping copy", i, fileCount));
                        }
                        else
                        {
                            WriteConsoleStatusLine(consoleTopAtStart + 1, string.Format("copying {0}/{1} {2} to {3}", i, fileCount, fullOriginalPath, fullTargetPath));
                            File.Copy(fullOriginalPath, fullTargetPath);
                        }
                    }
                }
                Console.WriteLine(""); //clear last line
                Console.WriteLine("Finished");
                Console.Read();
            }catch(Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(string.Format("Exception occured: {0}", e.Message));
                Console.Read();
            }
        }

        public static void WriteConsoleStatusLine(int consoleCursorTopStatusLine, string output)
        {
            //clear line            
            Console.SetCursorPosition(0, consoleCursorTopStatusLine);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, consoleCursorTopStatusLine);

            Console.Write(output);
        }

        public static IEnumerable<DirectoryInfo> GetSourceDirs()
        {
            var sourceDirsAsString = ConfigurationManager.AppSettings["SourceDirs"];
            var sourceDirs = sourceDirsAsString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(dirPath => new DirectoryInfo(dirPath));
            return sourceDirs;
        }
    }

}

