// Based on https://www.codeproject.com/Articles/290013/Formless-System-Tray-Application
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace MaxwellGPUIdle
{
    internal static class Helper
    {
        public static System.Collections.Generic.List<string> Convert(System.Collections.Specialized.StringCollection collection)
        {
            System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();
            foreach (string item in collection)
            {
                list.Add(item);
            }
            return list;
        }

        public static System.Collections.Specialized.StringCollection Convert(System.Collections.Generic.List<string> list)
        {
            System.Collections.Specialized.StringCollection collection = new System.Collections.Specialized.StringCollection();
            foreach (string item in list)
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

    internal static class Program
    {
        public static ProcessIcon currentIconInstance = new ProcessIcon();
        public static bool isTimerRunning = false;
        public static string ProductName = ((AssemblyProductAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), true)[0]).Product;

        public static void ExceptionHandler(Exception exception)
        {
            // Meep.
            System.Windows.Forms.MessageBox.Show(
                exception.ToString(), ":( Sortahandled Exception! - " + ((AssemblyProductAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), true)[0]).Product.ToString(),
                System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
        }

        public static void DoIdleTasks()
        {
            if (Properties.Settings.Default.KillOnIdle)
            {
                // NOTE: I would prefer to check the process' CPU usage but this appears to be
                //       difficult to obtain.
                //if (CPUStats.currentCPUUsage < 30.0)
                {
                    ProcessDestroyer.KillCompilerProcesses();
                }
            }
        }
        public static void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            DoIdleTasks();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // Quit if already running https://stackoverflow.com/a/6392264
            Mutex mutex = new System.Threading.Mutex(false, ProductName);
            try
            {
                if (mutex.WaitOne(0, false))
                {
                    // Show the system tray icon.
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(true);
                    currentIconInstance.Display();

                    System.Timers.Timer aTimer = new System.Timers.Timer();
                    aTimer.Elapsed += Program.OnTimedEvent;
                    aTimer.Interval = 1000 * 7200; // 120 minutes
                    aTimer.Enabled = true;
                    Stopwatch sw = Stopwatch.StartNew();
                    isTimerRunning = true;

                    SettingsManager.Refresh();
                    SettingsManager.LoadProcessesList();

                    // Make sure the application runs!
                    Application.Run();
                }
            }
            finally
            {
                if (mutex != null)
                {
                    mutex.Close();
                    mutex = null;
                }
            }
        }

        private class CPUStats
        {
            public static float currentCPUUsage;
            protected static PerformanceCounter cpuCounter;
            protected static PerformanceCounter ramCounter;
            private static List<float> AvailableCPU = new List<float>();
            private static List<float> AvailableRAM = new List<float>();
            private static int cores = 0;
            private static List<PerformanceCounter> cpuCounters = new List<PerformanceCounter>();

            public static void ConsumeCPU()
            {
                int percentage = 60;
                if (percentage < 0 || percentage > 100)
                    throw new ArgumentException("percentage");
                Stopwatch watch = new Stopwatch();
                watch.Start();
                while (true)
                {
                    // Make the loop go on for "percentage" milliseconds then sleep the remaining
                    // percentage milliseconds. So 40% utilization means work 40ms and sleep 60ms
                    if (watch.ElapsedMilliseconds > percentage)
                    {
                        Thread.Sleep(100 - percentage);
                        watch.Reset();
                        watch.Start();
                    }
                }
            }

            public static void TimerElapsed(object source, ElapsedEventArgs e)
            {
                float cpu = cpuCounter.NextValue();
                float sum = 0;
                foreach (PerformanceCounter c in cpuCounters)
                {
                    sum = sum + c.NextValue();
                }
                sum = sum / (cores);
                float ram = ramCounter.NextValue();
                Console.WriteLine(string.Format("CPU Value 1: {0}, cpu value 2: {1} ,ram value: {2}", sum, cpu, ram));
                AvailableCPU.Add(sum);
                AvailableRAM.Add(ram);
                currentCPUUsage = sum;
            }

            private static void Main(string[] args)
            {
                cpuCounter = new PerformanceCounter();
                cpuCounter.CategoryName = "Processor";
                cpuCounter.CounterName = "% Processor Time";
                cpuCounter.InstanceName = "_Total";

                foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get())
                {
                    cores = cores + int.Parse(item["NumberOfCores"].ToString());
                }

                ramCounter = new PerformanceCounter("Memory", "Available MBytes");

                int procCount = System.Environment.ProcessorCount;
                for (int i = 0; i < procCount; i++)
                {
                    System.Diagnostics.PerformanceCounter pc = new System.Diagnostics.PerformanceCounter("Processor", "% Processor Time", i.ToString());
                    cpuCounters.Add(pc);
                }

                Thread c = new Thread(ConsumeCPU);
                c.IsBackground = true;
                c.Start();

                try
                {
                    System.Timers.Timer t = new System.Timers.Timer(1200);
                    t.Elapsed += new ElapsedEventHandler(TimerElapsed);
                    t.Start();
                    Thread.Sleep(10000);
                }
                catch (Exception e)
                {
                    ExceptionHandler(e);
                }
                Console.ReadLine();
            }
        }
    }
}
