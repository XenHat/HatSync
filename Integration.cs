using MaxwellGPUIdle.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace MaxwellGPUIdle
{
    internal class Integration
    {
        private static readonly ProcessStartInfo PowerCfgStartInfo = new ProcessStartInfo
        {
            FileName = "powercfg.exe",
            Arguments = "-setactive 381b4222-f694-41f0-9685-ff5bb260df2e",
            CreateNoWindow = true
        };

        public static void AddToStartup()
        {
            string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            // Empty a few directories. Yes. If your shit is missing, this is the line that does it.
            Type t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8"));
            if (t != null)
            {
                dynamic shell = Activator.CreateInstance(t);
                try
                {
                    if (shell != null)
                    {
                        File.Delete(startupFolder + "\\" + Program.ProductName + ".lnk");
                        if (Settings.Default.AutomaticStartup)
                        {
                            dynamic startupEntry = shell.CreateShortcut(startupFolder + "\\" + Program.ProductName + ".lnk");
                            try
                            {
                                var currentPathToExe = Directory.GetCurrentDirectory() + "\\" + Program.ProductName + ".exe";
                                startupEntry.TargetPath = currentPathToExe;
                                startupEntry.IconLocation = currentPathToExe;
                                startupEntry.Save();
                            }
                            finally
                            {
                                Marshal.FinalReleaseComObject(startupEntry);
                            }
                        }
                    }
                }
                finally
                {
                    if (shell != null)
                    {
                        Marshal.FinalReleaseComObject(shell);
                    }
                }
            }
            //LogStats();
        }

        public static void SetPowerPlanToOnDemand()
        {
            Process powercfgProcess = new Process
            {
                StartInfo = PowerCfgStartInfo
            };
            powercfgProcess.Start();
        }
    }
}
