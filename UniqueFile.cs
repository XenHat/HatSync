// This is an independent project of an individual developer. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.IO;

namespace HatSync
{
    /// <summary>
    /// if I want to make it easy to dedupe things, I need to be able to find the data within the storage
    /// by basically any value (hash, file name, size, etc)
    /// </summary>
    ///
    internal class UniqueFile : IEquatable<UniqueFile>
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

        public string GetHashAsString()
        {
            return SimpleHasher.ByteArrayToString(_hash);
        }

        public override bool Equals(object other)
        {
            return Equals(other as UniqueFile);
        }

        public bool Equals(UniqueFile other)
        {
            return _hash == other.Hash;
        }

        private readonly string _fullpath;

        private readonly byte[] _hash;

        private readonly string _name;
    }
}
