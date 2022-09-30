using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordFinder.Model
{
    public class FolderStatistics
    {
        public string FilePath { get; set; }
        public Dictionary<string, int> ForbiddenWordCount { get; set; }

        public FolderStatistics(string folderPath = "")
        {
            FilePath = folderPath;
            ForbiddenWordCount = new();
        }
    }
}
