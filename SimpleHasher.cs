// This is an independent project of an individual developer. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace HatSync
{
    /// <summary>
    /// if I want to make it easy to dedupe things, I need to be able to find the data within the storage
    /// by basically any value (hash, file name, size, etc)
    /// </summary>
    ///
    internal struct UniqueFile
    {
        public UniqueFile(string a)
        {
            _fullpath = null;
            _name = null;
            _hash = null;
            try
            {
                _fullpath = a;
                _name = Path.GetFileName(_fullpath);
                _hash = SimpleHasher.DoHashFileDirect(_fullpath);
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.ToString());
            }
        }

        public byte[] Hash => _hash;

        public string Location => _fullpath;

        public string Name => _name;

        // What do I do with this?
        //private static Dictionary<UniqueFile, string> _FileHashMap;
        //
        //static UniqueFile()
        //{
        //    _FileHashMap = new Dictionary<UniqueFile, string>();
        //}
        //
        public string GetHashAsString()
        {
            return SimpleHasher.ByteArrayToString(_hash);
        }

        private readonly string _fullpath;

        private readonly byte[] _hash;

        private readonly string _name;
    }

    internal class CompareHashingMethods
    {
        //[Benchmark]
        protected static void Main()
        {
            Log.WriteLine("Running Hashing benchmark");
            SimpleHasher.DoHashFileDirect(@"C:\test\blob1");
            Log.WriteLine("Done.");
        }

        private const string File = @"C:\test\blob1";
    }

    internal static class SimpleHasher
    {
        public static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "");
        }

        public static byte[] DoHashFileDirect(string path)
        {
            byte[] hash = null;
            try
            {
                //using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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

        public static System.Collections.Generic.List<UniqueFile> HashFolder(string folder)
        {
            // TODO: Plug memory leak.
            // this function increases the memory usage every time it is run
            Log.WriteLine("Hashing " + folder);
            System.Collections.Generic.List<UniqueFile> files = new System.Collections.Generic.List<UniqueFile>();
            try
            {

                var folders = Enumerator.GetAllFilesFromFolder(folder, true);
                foreach (var file in folders)
                {
                    //Log.WriteLine("Processing " + file);
                    UniqueFile tentative = new UniqueFile(file);

                    if (tentative.Hash != null)
                    {
                        files.Add(tentative);
                        Log.WriteLine(string.Format("{0} *{1}", ByteArrayToString(tentative.Hash), tentative.Location));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.ToString());
            }

            Log.WriteLine("Finished hashing " + folder);
            GC.Collect();

            return files;
        }

        private static readonly int DigestSize = 32; // 256 Bits, 512 is overkill a little
    }
}
