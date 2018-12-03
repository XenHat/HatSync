using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatSync
{
    class EnumerationIOUtility
    {
        public static IEnumerable<string> GetAllFilesFromFolder(string root, bool searchSubfolders)
        {
            Queue<string> folders = new Queue<string>();
            List<string> files = new List<string>();
            folders.Enqueue(root);
            while (folders.Count != 0)
            {
                string currentFolder = folders.Dequeue();
                try
                {
                    string[] filesInCurrent = Directory.GetFiles(currentFolder, "*.*",
                        SearchOption.TopDirectoryOnly);
                    files.AddRange(filesInCurrent);
                }
                catch
                {
                    // Do Nothing
                }

                try
                {
                    if (searchSubfolders)
                    {
                        string[] foldersInCurrent = Directory.GetDirectories(currentFolder, "*.*",
                            SearchOption.TopDirectoryOnly);
                        foreach (string current in foldersInCurrent)
                        {
                            folders.Enqueue(current);
                        }
                    }
                }
                catch
                {
                    // Do Nothing
                }
            }

            return files;
        }
    }
}
