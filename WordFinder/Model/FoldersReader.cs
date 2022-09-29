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

        }

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

        private Dictionary<string, int> CheckFile(string pathToFile, string[] forbiddens, string workingDir)
        {
            Dictionary<string, int> coincidences = FindWrongWordsInFile(pathToFile, forbiddens);
            if(coincidences.Count > 0)
            {
                string pathToCopy = workingDir + "\\" + copyFolder + "\\" + pathToFile.Replace('\\', '.');
                File.Copy(pathToFile, pathToCopy);
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
                        if (forbiddens.Contains(NormalizeWord(w)))
                        {
                            res[NormalizeWord(w)] += 1;
                        }
                    }
                }
            }
            return res;
        }

        private void CopyFileWithStars(string pathToFile, string[] forbiddens, string fileToCopy)
        {
            using (StreamReader sr = new StreamReader(pathToFile))
            {
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    StringBuilder sb = new();
                    foreach (string w in line.Split())
                    {
                        if (forbiddens.Contains(NormalizeWord(w)))
                            sb.Append("*******");
                        else
                            sb.Append(w);
                        sb.Append(' ');
                    }
                    File.AppendAllLines(fileToCopy, new string[] { sb.ToString() });
                }
            }
        }
    }
}
