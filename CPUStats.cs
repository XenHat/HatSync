// This is an independent project of an individual developer. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

// Based on https://www.codeproject.com/Articles/290013/Formless-System-Tray-Application

// TODO: Remove, and use DotNet Core methods for Linux client portability.
// See https://adrientorris.github.io/aspnet-core/how-to-implement-timer-netcoreapp1-0-netcoreapp1-1.html

namespace HatSync
{
    /*
    internal static partial class Program
    {
        private static class CpuStats
        {
            private static float CurrentCpuUsage;

            private static void ConsumeCpu()
            {
                const int percentage = 60;
                if (percentage < 0 || percentage > 100) //-V3022
                {
                    throw new System.ArgumentException("percentage");
                }

                System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                watch.Start();
                while (true)
                {
                    // Make the loop go on for "percentage" milliseconds then sleep the remaining
                    // percentage milliseconds. So 40% utilization means work 40ms and sleep 60ms
                    if (watch.ElapsedMilliseconds > percentage)
                    {
                        System.Threading.Thread.Sleep(100 - percentage);
                        watch.Reset();
                        watch.Start();
                    }
                }
            }

            private static void TimerElapsed(object source, System.Timers.ElapsedEventArgs e)
            {
                var cpu = CpuCounter.NextValue();
                float sum = 0;
                foreach (System.Diagnostics.PerformanceCounter c in CpuCounters)
                {
                    sum = sum + c.NextValue();
                }
                sum = sum / _cores;
                var ram = RamCounter.NextValue();
                Log.WriteLine(string.Format("CPU Value 1: {0}, cpu value 2: {1} ,ram value: {2}", sum, cpu, ram));
                _availableCpu.Add(sum);
                _availableRam.Add(ram);
                CurrentCpuUsage = sum;
            }

            private static System.Diagnostics.PerformanceCounter CpuCounter;
            private static System.Diagnostics.PerformanceCounter RamCounter;
            private static readonly System.Collections.Generic.List<float> _availableCpu = new System.Collections.Generic.List<float>();
            private static readonly System.Collections.Generic.List<float> _availableRam = new System.Collections.Generic.List<float>();
            private static int _cores = 0;
            private static readonly System.Collections.Generic.List<System.Diagnostics.PerformanceCounter> CpuCounters = new System.Collections.Generic.List<System.Diagnostics.PerformanceCounter>();

            private static void Main(string[] args)
            {
                CpuCounter = new System.Diagnostics.PerformanceCounter
                {
                    CategoryName = "Processor",
                    CounterName = "% Processor Time",
                    InstanceName = "_Total"
                };

                foreach (System.Management.ManagementBaseObject item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get())
                {
                    _cores = _cores + int.Parse(item["NumberOfCores"].ToString());
                }

                RamCounter = new System.Diagnostics.PerformanceCounter("Memory", "Available MBytes");

                var procCount = System.Environment.ProcessorCount;
                for (var i = 0; i < procCount; i++)
                {
                    System.Diagnostics.PerformanceCounter pc = new System.Diagnostics.PerformanceCounter("Processor", "% Processor Time", i.ToString());
                    CpuCounters.Add(pc);
                }

                System.Threading.Thread c = new System.Threading.Thread(ConsumeCpu)
                {
                    IsBackground = true
                };
                c.Start();

                try
                {
                    System.Timers.Timer t = new System.Timers.Timer(1200);
                    t.Elapsed += TimerElapsed;
                    t.Start();
                    System.Threading.Thread.Sleep(10000);
                }
                catch (System.Exception e)
                {
                    ExceptionHandler(e);
                }

                System.Console.ReadLine();
            }
        }
    }
    */
}
