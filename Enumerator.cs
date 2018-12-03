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

        /*
        public static IEnumerable<KeyValuePair<DirectoryInfo, List<FileSystemInfo>>> GetFileSystemInfosRecursive(
            DirectoryInfo dir, bool depthFirst)
        {
            var stack = depthFirst ? new Stack<DirectoryInfo>() : null;
            var queue = depthFirst ? null : new Queue<DirectoryInfo>();
            if (depthFirst)
            {
                stack.Push(dir);
            }
            else
            {
                queue.Enqueue(dir);
            }

            for (var list = new List<FileSystemInfo>(); (depthFirst ? stack.Count : queue.Count) > 0; list.Clear())
            {
                dir = depthFirst ? stack.Pop() : queue.Dequeue();
                FileSystemInfo[] children;
                try
                {
                    children = dir.GetFileSystemInfos();
                }
                catch (UnauthorizedAccessException)
                {
                    children = null;
                }
                catch (IOException)
                {
                    children = null;
                }

                if (children != null)
                {
                    list.AddRange(children);
                }

                yield return new KeyValuePair<DirectoryInfo, List<FileSystemInfo>>(dir, children != null ? list : null);
                if (depthFirst)
                {
                    list.Reverse();
                }

                foreach (var child in list)
                {
                    var asdir = child as DirectoryInfo;
                    if (asdir != null)
                    {
                        if (depthFirst)
                        {
                            stack.Push(asdir);
                        }
                        else
                        {
                            queue.Enqueue(asdir);
                        }
                    }
                }
            }
        }*/
    }
}
