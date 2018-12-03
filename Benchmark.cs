// This is an independent project of an individual developer. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

namespace HatSync
{
    [BenchmarkDotNet.Attributes.ClrJob(true)]
    [BenchmarkDotNet.Attributes.RPlotExporter]
    public class Blake2PVsBlake2S : System.IDisposable
    {
        [BenchmarkDotNet.Attributes.Params(20, 500, 1000, 100000)]
        // ReSharper disable once UnassignedField.Global
        public int N;

        [BenchmarkDotNet.Attributes.Benchmark]
        public byte[] Blake2P()
        {
            return _b2P.ComputeHash(_data);
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public byte[] Blake2S()
        {
            return _b2S.ComputeHash(_data);
        }

        public void Dispose()
        {
            _b2P.Dispose();
            _b2S.Dispose();
        }

        [BenchmarkDotNet.Attributes.GlobalSetup]
        public void Setup()
        {
            _data = new byte[N];
            new System.Random(42).NextBytes(_data);
        }

        private readonly System.Security.Cryptography.HashAlgorithm _b2P = SauceControl.Blake2Fast.Blake2b.CreateHashAlgorithm();
        private readonly System.Security.Cryptography.HashAlgorithm _b2S = SauceControl.Blake2Fast.Blake2s.CreateHashAlgorithm();
        private byte[] _data;
    }

    //[DryJob]
    //[ClrJob(baseline: true)]
    //[SimpleJob(RunStrategy.ColdStart, launchCount: 1, warmupCount: 2, targetCount: 5, id: "FastAndDirtyJob")]
    [BenchmarkDotNet.Attributes.ClrJob(true)]
    [BenchmarkDotNet.Attributes.RPlotExporter, BenchmarkDotNet.Attributes.RankColumn]
    public class Md5VsSha256VsBlake2 : System.IDisposable
    {
        [BenchmarkDotNet.Attributes.Params(256, 512, 1024, 2048, 4096, 8192)]
        // ReSharper disable once UnassignedField.Global
        public int N;

        [BenchmarkDotNet.Attributes.Benchmark]
        public byte[] Blake2P()
        {
            return _b2P.ComputeHash(_data);
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public byte[] Blake2S()
        {
            return _b2S.ComputeHash(_data);
        }

        public void Dispose()
        {
            _b2P.Dispose();
            _b2S.Dispose();
            _md5.Dispose();
            _sha256.Dispose();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public byte[] Md5()
        {
            return _md5.ComputeHash(_data);
        }

        [BenchmarkDotNet.Attributes.GlobalSetup]
        public void Setup()
        {
            _data = new byte[N];
            new System.Random(42).NextBytes(_data);
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public byte[] Sha256()
        {
            return _sha256.ComputeHash(_data);
        }

        private readonly System.Security.Cryptography.HashAlgorithm _b2P = SauceControl.Blake2Fast.Blake2b.CreateHashAlgorithm();
        private readonly System.Security.Cryptography.HashAlgorithm _b2S = SauceControl.Blake2Fast.Blake2s.CreateHashAlgorithm();
        private byte[] _data;
        private readonly System.Security.Cryptography.MD5 _md5 = System.Security.Cryptography.MD5.Create();
        private readonly System.Security.Cryptography.SHA256 _sha256 = System.Security.Cryptography.SHA256.Create();
    }
}
