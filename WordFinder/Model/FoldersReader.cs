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

            Directory.CreateDirectory(vm.InitFolder);

            //CheckFile(@"C:\Users\samos\OneDrive\Рабочий стол\baba.txt", vm.ForbiddenWords.Split(), vm.InitFolder);
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
                if (Directory.Exists(workingDir + "\\" + copyFolder)) 
                    Directory.Delete(workingDir + "\\" + copyFolder, true);
                Directory.CreateDirectory(workingDir + "\\" + copyFolder);
                File.Copy(pathToFile, pathToCopy);

                string pathToCensored = workingDir + "\\" + replacedFolder + "\\" + pathToFile.Replace('\\', '-').Replace(':', ' ');
                if (Directory.Exists(workingDir + "\\" + replacedFolder))
                    Directory.Delete(workingDir + "\\" + replacedFolder, true);
                Directory.CreateDirectory(workingDir + "\\" + replacedFolder);
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
