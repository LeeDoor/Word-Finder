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
        public void Start(MainWindowVM vm)
        {
            //var a = FindWrongWordsInFile(@"C:\Users\samos\OneDrive\Рабочий стол\baba.txt", new string[] { "orsgorsgko", "getkpohgpoketgko" });//, @"C:\Users\samos\OneDrive\Рабочий стол\worka");
            //StringBuilder sb = new();
            //foreach(var pair in a)
            //{
            //    sb.AppendLine($"{pair.Key} was meeted {pair.Value} times");
            //}
            //MessageBox.Show(sb.ToString());

            //CopyFileWithStars(@"C:\Users\samos\OneDrive\Рабочий стол\baba.txt", new string[] { "orsgorsgko", "getkpohgpoketgko" }, @"D:\file.txt");

            if (Directory.Exists(vm.InitFolder + "\\" + replacedFolder))
                Directory.Delete(vm.InitFolder + "\\" + replacedFolder, true);
            Directory.CreateDirectory(vm.InitFolder + "\\" + replacedFolder);

            if (Directory.Exists(vm.InitFolder + "\\" + copyFolder))
                Directory.Delete(vm.InitFolder + "\\" + copyFolder, true);
            Directory.CreateDirectory(vm.InitFolder + "\\" + copyFolder);

            Directory.CreateDirectory(vm.InitFolder);
            CheckFolder(@"C:\Users\samos\OneDrive\Рабочий стол\worka", vm.ForbiddenWords.Split(), vm.InitFolder);
            //CheckFile(@"C:\Users\samos\OneDrive\Рабочий стол\baba.txt", vm.ForbiddenWords.Split(), vm.InitFolder);
        }

        /// <summary>
        /// recursively checks given folder and returns full statistic about it
        /// </summary>
        /// <param name="folderPath">path to this folder</param>
        /// <param name="forbiddens">forbidden words to find</param>
        /// <param name="workingDir">directory we work with</param>
        /// <returns>fill stat about folder</returns>
        private Dictionary<string, int> CheckFolder(string folderPath, string[] forbiddens, string workingDir)
        {
            Dictionary<string, int> res = new();
            string[] subdirs = Directory.GetDirectories(folderPath);
            foreach(var subdir in subdirs)
            {
                var dic = CheckFolder(subdir, forbiddens, workingDir);
                res.Concat(dic);
            }

            foreach(var file in Directory.GetFiles(folderPath))
            {
                var dic = CheckFile(file, forbiddens, workingDir);
                res.Concat(dic);
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
        private Dictionary<string, int> CheckFile(string pathToFile, string[] forbiddens, string workingDir)
        {
            Dictionary<string, int> coincidences = FindWrongWordsInFile(pathToFile, forbiddens);
            if(coincidences.Count > 0)
            {
                string pathToCopy = workingDir + "\\" + copyFolder + "\\" + pathToFile.Replace('\\', '-').Replace(':', ' ');
                File.Copy(pathToFile, pathToCopy);

                string pathToCensored = workingDir + "\\" + replacedFolder + "\\" + pathToFile.Replace('\\', '-').Replace(':', ' ');
                CopyFileWithStars(pathToFile, forbiddens, pathToCensored);
            }
            return coincidences;
        }

        /// <summary>
        /// sorts through one single file
        /// </summary>
        /// <param name="pathToFile">path to needed file</param>
        /// <param name="forbiddens">list of forbidden words</param>
        /// <returns>returns dictionary. KEY - forbidden word. VALUE - amount found</returns>
        private Dictionary<string, int> FindWrongWordsInFile(string pathToFile, string[] forbiddens)
        {
            Dictionary<string, int> res = new();

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
                            if (res.ContainsKey(curW))
                                res[curW] += 1;
                            else
                                res[curW] = 1;
                        }
                    }
                }
            }
            return res;
        }

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
