using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using WordFinder.ViewModel;
using System.IO;
using System.Windows.Shapes;
using System.Reflection.PortableExecutable;
using System.Windows;

namespace WordFinder.Model
{
    /// <summary>
    /// sorts through all files on pc and counts amount of forbidden words
    /// </summary>
    public class FoldersReader
    {
        private const string copyFolder = "folder with copies";
        private const string replacedFolder = "folder with stars";

        /// <summary>
        /// inits sorting through
        /// </summary>
        /// <param name="vm">vm with all needed properties</param>
        public async void Start(MainWindowVM vm)
        {
            CreateNeededDirectories(vm.InitFolder);
            List<FolderStatistics> res = new List<FolderStatistics>();
            //foreach (var drive in DriveInfo.GetDrives())
            //{
            //    res.Concat(await CheckFolderAsync(drive.Name, vm.ForbiddenWords.Split(), vm.InitFolder));
            //}
            var a = await CheckFolderAsync(@"C:\Users\samos\OneDrive\Рабочий стол\worka", vm.ForbiddenWords.Split(), vm.InitFolder);
            res = res.Concat(a).ToList();
            CreateLog(res, vm.InitFolder + "\\LOG.txt");
            MessageBox.Show("mb finished");
        }

        private void CreateLog(List<FolderStatistics> stats, string pathToLog)
        {
            File.Create(pathToLog).Dispose();
            // adding info about directory and banned words
            File.AppendAllLines(pathToLog, stats.Select(n => n.FilePath + " " + BytesToString(new FileInfo(n.FilePath).Length) + " " + n.ForbiddenWordCount.Sum(n => n.Value)));

            Dictionary<string, int> total = new();
            foreach (FolderStatistics stat in stats)
            {
                foreach (var pair in stat.ForbiddenWordCount)
                {
                    if (!total.TryAdd(pair.Key, pair.Value))
                    {
                        total[pair.Key] += pair.Value;
                    }
                }
            }
            KeyValuePair<string, int>? best = total.Where(n => n.Value == total.Max(n => n.Value)).FirstOrDefault();
            if(best != null)
                File.AppendAllLines(pathToLog, new string[1] {$"best word was found {best.Value.Key} with amount {best.Value.Value}"});
        }

        private string BytesToString(long bytes)
        {
            List<string> result = new List<string>();
            string[] codes = new string[] { "KB", "MB", "GB", "TB" };
            for(int i = 0; i < codes.Length && bytes > 0; i++)
            {
                result.Add((bytes % 1000).ToString() + codes[i] + ' ');
                bytes /= (long)1000;
            }

            StringBuilder sb = new();
            for (int i = result.Count - 1; i >= 0; i--)
            {
                sb.Append(result[i]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// creates needed directories for a project
        /// </summary>
        /// <param name="initFolder">start folder name</param>
        private void CreateNeededDirectories(string initFolder)
        {
            Directory.CreateDirectory(initFolder);
            if (Directory.Exists(initFolder + "\\" + replacedFolder))
                Directory.Delete(initFolder + "\\" + replacedFolder, true);
            Directory.CreateDirectory(initFolder + "\\" + replacedFolder);

            if (Directory.Exists(initFolder + "\\" + copyFolder))
                Directory.Delete(initFolder + "\\" + copyFolder, true);
            Directory.CreateDirectory(initFolder + "\\" + copyFolder);
        }

        /// <summary>
        /// recursively checks given folder and returns full statistic about it
        /// </summary>
        /// <param name="folderPath">path to this folder</param>
        /// <param name="forbiddens">forbidden words to find</param>
        /// <param name="workingDir">directory we work with</param>
        /// <returns>fill stat about folder</returns>
        private async Task<List<FolderStatistics>> CheckFolderAsync(string folderPath, string[] forbiddens, string workingDir)
        {
            List<FolderStatistics> res = new();
            string[] subdirs = Directory.GetDirectories(folderPath);
            foreach(var subdir in subdirs)
            {
                try
                {
                    res = res.Concat(await CheckFolderAsync(subdir, forbiddens, workingDir)).ToList();
                }
                catch { }
            }

            foreach(var file in Directory.GetFiles(folderPath))
            {
                res.Add(await CheckFile(file, forbiddens, workingDir));
            }
            return res;
        }

        /// <summary>
        /// normalizing word by removing containing dots and other
        /// </summary>
        /// <param name="word">word to normalize</param>
        /// <returns>normalized word</returns>
        private string NormalizeWord(string word)
        {
            StringBuilder sb = new();
            foreach (char ch in word)
            {
                if ('a' <= ch && ch <= 'z' || 'A' <= ch && ch <= 'Z')
                {
                    sb.Append(ch);
                }
            }
            return sb.ToString();
        }


        /// <summary>
        /// prepares all operations with 1 single file
        /// </summary>
        /// <param name="pathToFile">path to operationable file</param>
        /// <param name="forbiddens">forbidden words</param>
        /// <param name="workingDir">directory, where should we copy</param>
        /// <returns></returns>
        private async Task<FolderStatistics> CheckFile(string pathToFile, string[] forbiddens, string workingDir)
        {
            FolderStatistics coincidences = new();
            await Task.Run(() =>
            {
                try
                {
                    coincidences = FindWrongWordsInFile(pathToFile, forbiddens);
                    if (coincidences.ForbiddenWordCount.Count > 0)
                    {
                        string pathToCopy = workingDir + "\\" + copyFolder + "\\" + pathToFile.Replace('\\', '-').Replace(':', ' ');
                        File.Copy(pathToFile, pathToCopy);

                        string pathToCensored = workingDir + "\\" + replacedFolder + "\\" + pathToFile.Replace('\\', '-').Replace(':', ' ');
                        CopyFileWithStars(pathToFile, forbiddens, pathToCensored);
                    }
                }
                catch
                {

                }
            }); 
            return coincidences;
        }

        /// <summary>
        /// sorts through one single file
        /// </summary>
        /// <param name="pathToFile">path to needed file</param>
        /// <param name="forbiddens">list of forbidden words</param>
        /// <returns>returns dictionary. KEY - forbidden word. VALUE - amount found</returns>
        private FolderStatistics FindWrongWordsInFile(string pathToFile, string[] forbiddens)
        {
            FolderStatistics res = new FolderStatistics(pathToFile);

            using (StreamReader sr = new StreamReader(pathToFile))
            {
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    foreach (string w in line.Split())
                    {
                        string curW = NormalizeWord(w);
                        if (forbiddens.Contains(curW))
                        {
                            if (res.ForbiddenWordCount.ContainsKey(curW))
                                res.ForbiddenWordCount[curW] += 1;
                            else
                                res.ForbiddenWordCount[curW] = 1;
                        }
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// copies given file with replaced forbidden words in it
        /// </summary>
        /// <param name="pathToFile"></param>
        /// <param name="forbiddens"></param>
        /// <param name="fileToCopy"></param>
        private void CopyFileWithStars(string pathToFile, string[] forbiddens, string fileToCopy)
        {
            File.Create(fileToCopy).Dispose();
            string[] allLines = File.ReadAllLines(pathToFile);
            for(int i = 0; i < allLines.Length; i++)
            {
                StringBuilder sb = new();
                foreach(var word in allLines[i].Split())
                {
                    if (forbiddens.Contains(NormalizeWord(word)))
                        sb.Append("*******");
                    else
                        sb.Append(word);
                    sb.Append(' ');
                }
                allLines[i] = sb.ToString();
            }
            File.AppendAllLines(fileToCopy, allLines);
        }
    }
}
