// This is an independent project of an individual developer. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

// Based on https://www.codeproject.com/Articles/290013/Formless-System-Tray-Application

// TODO: Remove, and use DotNet Core methods for Linux client portability.
// See https://adrientorris.github.io/aspnet-core/how-to-implement-timer-netcoreapp1-0-netcoreapp1-1.html

using System.Collections.Generic;

namespace HatSync
{
    public static class ConsoleHelper
    {
        public static void CreateConsole()
        {
            AllocConsole();

            // stdout's handle seems to always be equal to 7
            System.IntPtr defaultStdout = new System.IntPtr(7);
            System.IntPtr currentStdout = GetStdHandle(StdOutputHandle);

            if (currentStdout != defaultStdout)
            {
                // reset stdout
                SetStdHandle(StdOutputHandle, defaultStdout);
            }

            // reopen stdout
            System.IO.TextWriter writer = new System.IO.StreamWriter(System.Console.OpenStandardOutput())
            { AutoFlush = true };
            System.Console.SetOut(writer);
        }

        // P/Invoke required:
        private const uint StdOutputHandle = 0xFFFFFFF5;

        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern bool AllocConsole();

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern System.IntPtr GetStdHandle(uint nStdHandle);

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern void SetStdHandle(uint nStdHandle, System.IntPtr handle);
    }

    public static class Log
    {
        internal static void WriteLine(string input)
        {
            // Do nothing because logging is only working in debug mode
#if DEBUG
            System.Console.WriteLine(input);
#endif
        }
    }

    internal static class Helper
    {
        public static System.Collections.Generic.List<string> Convert(System.Collections.Specialized.StringCollection collection)
        {
            System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
            foreach (var item in collection)
            {
                list.Add(item);
            }
            return list;
        }

        public static System.Collections.Specialized.StringCollection Convert(IEnumerable<string> list)
        {
            System.Collections.Specialized.StringCollection collection = new System.Collections.Specialized.StringCollection();
            foreach (var item in list)
            {
                collection.Add(item);
            }
            return collection;
        }

        public static bool ValidateExecutableName(string url)
        {
            return !url.EndsWith(".exe");
        }
    }

    //internal static partial class Program
    internal static class Program
    {
        private static readonly string ProductName = ((System.Reflection.AssemblyProductAttribute)System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyProductAttribute), true)[0]).Product;

        /// <summary>
        /// Static instance of the Tray Icon
        /// </summary>
        public static readonly System.Windows.Forms.NotifyIcon STrayIcon = new System.Windows.Forms.NotifyIcon();

        private static void DoIdleTasks()
        {
            // Do not run sendemail here, it has its own timer.
            //IPUpdater.SendEmail();
        }

        private static void ExceptionHandler(System.Exception exception)
        {
            // Meep.
            System.Windows.Forms.MessageBox.Show(
                exception.ToString(), ":( Sortahandled Exception! - " + ((System.Reflection.AssemblyProductAttribute)System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Reflection.AssemblyProductAttribute), true)[0]).Product, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern System.IntPtr FindWindow(string lpClassName, string lpWindowName);

        private static void OnTimedEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            DoIdleTasks();
        }

        public static void SetConsoleWindowVisibility(bool visible, string title)
        {
            System.IntPtr hWnd = FindWindow(null, title);
            if (hWnd != System.IntPtr.Zero)
            {
                if (!visible)
                {
                    ShowWindow(hWnd, 0);
                }
                else
                {
                    ShowWindow(hWnd, 1);
                }
            }
        }

        [System.Runtime.InteropServices.DllImport("msvcrt.dll")]
        public static extern int system(string cmd);

        private const int MyCodePage = 437;

        private const int StdOutputHandle = -11;

        [System.Runtime.InteropServices.DllImport("kernel32.dll",
            EntryPoint = "AllocConsole",
            SetLastError = true,
            CharSet = System.Runtime.InteropServices.CharSet.Auto,
            CallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall)]
        private static extern int AllocConsole();

        private static void ConsoleMain()
        {
            Log.WriteLine("HI FROM CONSOLEMAIN");
            //throw new NotImplementedException();
        }

        [System.Runtime.InteropServices.DllImport("kernel32.dll",
           EntryPoint = "GetStdHandle",
           SetLastError = true,
           CharSet = System.Runtime.InteropServices.CharSet.Auto,
           CallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall)]
        private static extern System.IntPtr GetStdHandle(int nStdHandle);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [System.STAThread]
        private static void Main(string[] args)
        {
            // Quit if already running https://stackoverflow.com/a/6392264
            System.Threading.Mutex mutex = new System.Threading.Mutex(false, ProductName);
            var runBenchmark = false;
            var runHashTest = false;
            try
            {
                if (mutex.WaitOne(0, false))
                {
                    if (args != null && args.Length > 0)
                    {
                        for (var i = 0; i < args.Length; i++)
                        {
                            var s = args[i];
                            if (s == "--debug")
                            {
                                // Throw everything inside a new console window. This code works.
                                AllocConsole();
                                System.IntPtr stdHandle = GetStdHandle(StdOutputHandle);
                                Microsoft.Win32.SafeHandles.SafeFileHandle safeFileHandle = new Microsoft.Win32.SafeHandles.SafeFileHandle(stdHandle, true);
                                System.IO.FileStream fileStream = new System.IO.FileStream(safeFileHandle, System.IO.FileAccess.Write);
                                System.Text.Encoding encoding = System.Text.Encoding.GetEncoding(MyCodePage);
                                System.IO.StreamWriter standardOutput = new System.IO.StreamWriter(fileStream, encoding)
                                {
                                    AutoFlush = true
                                };
                                System.Console.SetOut(standardOutput);
                            }
                            if (s == "--benchmark")
                            {
                                runBenchmark = true;
                            }
                            if (s == "--test")
                            {
                                runHashTest = true;
                            }
                        }
                    }
                    if (!runBenchmark)
                    {
                        // Show the system tray icon.
                        System.Windows.Forms.Application.EnableVisualStyles();
                        //Application.SetCompatibleTextRenderingDefault(true);

                        System.Timers.Timer aTimer = new System.Timers.Timer();
                        aTimer.Elapsed += OnTimedEvent;
                        aTimer.Interval = 1000 * 7200; // 120 minutes
                        aTimer.Enabled = true;
                        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

                        // Put the icon in the system tray
                        STrayIcon.Icon = Properties.Resources.HatSync;
                        STrayIcon.Text = "HatSync";
                        STrayIcon.Visible = true;

                        // Run once at startup
                        IpUpdater.SetUpTimer();

                        // Attach a context menu.
                        MenuGenerator.ContextMenus.RegenerateMenu();

                        if (runHashTest)
                        {
                            //==============================================================
                            // TEST Hash a file. The value should always be the same
                            // otherwise something went wrong with the implementation.

                            // Create dummy file
                            const string data = "Hello, world!";
                            const string name = "test.bin";
                            System.IO.File.WriteAllText(name, data);
                            // Hash it
                            UniqueFile result = new UniqueFile(name);
                            Log.WriteLine("Hash of test file: " + result.GetHashAsString());

                            if (result.GetHashAsString() == "B5DA441CFE72AE042EF4D2B17742907F675DE4DA57462D4C3609C2E2ED755970")
                            {
                                Log.WriteLine("Test Sucessful");
                            }
                            else
                            {
                                Log.WriteLine("Shit.");
                            }

                            if (System.IO.File.Exists(name))
                            {
                                System.IO.File.Delete(name);
                            }
                        }

                        System.Windows.Forms.Application.Run();
                    }
                    else
                    {
                        BenchmarkDotNet.Reports.Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<Md5VsSha256VsBlake2>();
                        //var summary = BenchmarkRunner.Run<Blake2pVsBlake2s>();

                        //Log.WriteLine(summary.ToString());
                        //==============================================================
                        // Make sure the application runs!
                    }
                }
            }
            finally
            {
                mutex.Close();
                mutex = null;
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(System.IntPtr hWnd, int nCmdShow);
    }
}
