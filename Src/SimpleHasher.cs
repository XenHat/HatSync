// This is an independent project of an individual developer. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HatSync
{

    internal static class SimpleHasher
    {
        public static class GetHashAsync
        {
            private static byte[] lastHash;

            public static byte[] Go(string file)
            {
                DoWork(file);
                return lastHash;
            }

            private static async void DoWork(string file)
            {
                await Task.Run(() => GetHash(file));
            }

            private static void GetHash(string file)
            {
                lastHash = DoHashFileDirect(file);
            }
        }
        public static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "");
        }

        public static byte[] DoHashFileDirect(string path)
        {
            byte[] hash = null;
            try
            {
                //using (FileStream fs = new FileStream(folder, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (BufferedStream fs = new BufferedStream(File.OpenRead(path), 1200000))
                using (System.Security.Cryptography.HashAlgorithm algo = SauceControl.Blake2Fast.Blake2b.CreateHashAlgorithm(DigestSize))
                {
                    hash = algo.ComputeHash(fs);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.ToString());
                return null;
            }
            return hash;
        }

        // This is not recursive. Yet.
        public static System.Collections.Generic.List<string> FolderEnumeratorFirstLevel(string folder)
        {
            System.Collections.Generic.List<string> contents = new System.Collections.Generic.List<string>();
            // Todo: Return an array maybe?
            if (Directory.Exists(folder))
            {
                foreach (var path in Directory.EnumerateFiles(folder))
                {
                    contents.Add(path);
                    Log.WriteLine("Found '" + path + "'");
                }
            }
            return contents;
        }

        public static string GetFileHash(string path)
        {
            byte[] result = DoHashFileDirect(path);
            if (result != null)
            {
                var hashString = path + " *" + ByteArrayToString(result);
                Log.WriteLine(hashString);
                return hashString;
            }
            return null;
        }

        public static async Task<List<UniqueFile>> HashFolder(string folder)
        {
            return await StartFolderHash(folder);
        }

            public static async Task<System.Collections.Generic.List<UniqueFile>> StartFolderHash(string folder)
            {
                System.Collections.Generic.List<UniqueFile> files =
                    new System.Collections.Generic.List<UniqueFile>();
                await Task.Run(() =>
                {
                    // TODO: Plug memory leak.
                    // this function increases the memory usage every time it is run
                    Log.WriteLine("Hashing " + folder);

                    try
                    {

                        var folders = EnumerationIOUtility.GetAllFilesFromFolder(folder, true);
                        foreach (var file in folders)
                        {
                            UniqueFile tentative = new UniqueFile(file);

                            if (tentative.Hash != null)
                            {
                                files.Add(tentative);
                                Log.WriteLine(string.Format("{0} *{1}", ByteArrayToString(tentative.Hash),
                                    tentative.Location));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(ex.ToString());
                    }

                    Log.WriteLine("Finished hashing " + folder);
                });
                return files;
            }

        private static readonly int DigestSize = 32; // 256 Bits, 512 is overkill a little
    }
}
