// This is an independent project of an individual developer. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace HatSync
{
    public class IPUpdater
    {
        private static readonly string FetchURLv4 = "http://ifconfig.me/ip";
        private static readonly string FetchURLv6 = "https://ipv6.icanhazip.com"; // TODO: Find a better one.
        public static class CachedValues
        {
            static IPAddress _CachedExternalIpAddressv4;
            static IPAddress _CachedExternalIpAddressv6;
            static IPAddress _PreviousExternalIpAddressv4;
            static IPAddress _PreviousExternalIpAddressv6;

            public static IPAddress GetCachedExternalIpAddressv4()
            {
                return _CachedExternalIpAddressv4;
            }
            public static IPAddress GetCachedExternalIpAddressv6()
            {
                return _CachedExternalIpAddressv6;
            }
            public static IPAddress GetPreviousExternalIpAddressv4()
            {
                return _PreviousExternalIpAddressv4;
            }
            public static IPAddress GetPreviousExternalIpAddressv6()
            {
                return _PreviousExternalIpAddressv6;
            }
            public static void SetCachedExternalIpAddressv4(IPAddress input)
            {
                _CachedExternalIpAddressv4 = input;
            }
            public static void SetCachedExternalIpAddressv6(IPAddress input)
            {
                _CachedExternalIpAddressv6 = input;
            }
            public static void SetPreviousExternalIpAddressv4(IPAddress input)
            {
                _PreviousExternalIpAddressv4 = input;
            }
            public static void SetPreviousExternalIpAddressv6(IPAddress input)
            {
                _PreviousExternalIpAddressv6 = input;
            }

            public static IEnumerable<IPAddress> GetCachedIPs()
            {
                return new System.Collections.Generic.HashSet<System.Net.IPAddress> { _CachedExternalIpAddressv4, _CachedExternalIpAddressv6 };
            }
        }

        public static void CheckAndSendEmail()
        {
            CheckAndSendEmail(false);
        }
        public static void CheckAndSendEmail(bool forceSend)
        {
            if (forceSend)
            {
                EmailSender.SendEmail(CheckIpChange());
            }
            else
            {
                if (CheckIpChange())
                {
                    EmailSender.SendEmail();
                }
            }
        }

        private static System.Collections.Generic.HashSet<System.Net.IPAddress> GetNewIPs()
        {
            //// TODO: Use IEnumerable
            System.Collections.Generic.HashSet<System.Net.IPAddress> result = GetNewIPsFromOutside();
            return result;

        }

        private static System.Collections.Generic.HashSet<System.Net.IPAddress> GetNewIPsFromOutside()
        {
            // Requires internet. Makes sense since you can't get an INTERNET address without it.
            // TODO: Make sure the client works fine without these. It can and will happen.
            System.Collections.Generic.HashSet<System.Net.IPAddress> returnValue = new System.Collections.Generic.HashSet<System.Net.IPAddress>();

            try
            {
                System.Net.IPAddress v6 = null;
                System.Net.IPAddress v4 = null;
                try
                {
                    System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;
                }
                catch (System.Exception ex)
                {
                    Log.WriteLine(ex.ToString());
                }
                var fetchedV4 = "";
                var fetchedV6 = "";
                try
                {
                    Log.WriteLine("Fetching IPv6 Address...");
                    fetchedV6 = new ImpatientWebClient().DownloadString(IPUpdater.FetchURLv6).Trim();
                }
                catch (System.Exception ex)
                {
                    Log.WriteLine(ex.ToString());
                }
                finally
                {
                    var isValid = System.Net.IPAddress.TryParse(fetchedV6, out v6) && v6 != null;
                    if (isValid && v6.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        // ipv6 returned an ipv4 address. Save one lookup and use it in-place.
                        fetchedV4 = fetchedV6;
                        v6 = null;
                        Log.WriteLine("Recycling returned IPv4 address...");
                    }
                    else
                    {
                        Log.WriteLine("Fetching IPv6 failed");
                    }
                }
                if (string.IsNullOrEmpty(fetchedV4))
                {
                    try
                    {
                        Log.WriteLine("Fetching IPv4 Address...");
                        fetchedV4 = new ImpatientWebClient().DownloadString(IPUpdater.FetchURLv4).Trim();
                    }
                    catch (System.Exception ex)
                    {
                        Log.WriteLine("Fetching IPv4 failed");
                        Log.WriteLine("Exception:" + ex);
                    }
                    finally
                    {
                        Log.WriteLine("IPv4:" + fetchedV4);
                    }
                }

                System.Collections.Generic.HashSet<System.Net.IPAddress> set = new System.Collections.Generic.HashSet<System.Net.IPAddress>();
                // TODO: Better validation
                if (IPAddress.TryParse(fetchedV4,out v4))
                {
                    set.Add(v4);
                }
                if (IPAddress.TryParse(fetchedV6, out v6))
                {
                    set.Add(v6);
                }
                return set;
            }
            catch (System.Exception ex)
            {
                Log.WriteLine(ex.ToString());
            }
            return null;
        }

        public static void Main(string[] args)
        {
            SetUpTimer();
            while (true) { /*no-op*/}
        }

        public static void SetUpTimer()
        {
            SetUpTimer(false);
        }
        public static void SetUpTimer(bool retry)
        {
            if (!_alreadyStarted || retry)
            {
                // Load values FROM THE LATEST PREVIOUS VERSION inside the current settings.
                // Upgrading after saving will STILL get the PREVIOUS VERSION's settings
                // TODO: Detect if version changed before calling upgrade.

                var settingsIpv6 = Properties.Settings.Default.LastIPv6;
                Log.WriteLine("IPv6 from settings:" + settingsIpv6);
                var settingsIpv4 = Properties.Settings.Default.LastIPv4;
                Log.WriteLine("IPv4 from settings:" + settingsIpv4);
                if (!_alreadyStarted && string.IsNullOrEmpty(settingsIpv4) && string.IsNullOrEmpty(settingsIpv6))
                {
                    // No settings, try to upgrade.
                    Properties.Settings.Default.Upgrade();

                    // Try again
                    _alreadyStarted = true;
                    SetUpTimer(true);
                }
                else
                {
                    if (settingsIpv6 != null && System.Net.IPAddress.TryParse(settingsIpv6, out System.Net.IPAddress v6))
                    {
                        CachedValues.SetCachedExternalIpAddressv6(v6);
                        CachedValues.SetPreviousExternalIpAddressv6(v6);
                    }
                    if (settingsIpv4 != null && System.Net.IPAddress.TryParse(settingsIpv4, out System.Net.IPAddress v4))
                    {
                        CachedValues.SetCachedExternalIpAddressv4(v4);
                        CachedValues.SetPreviousExternalIpAddressv4(v4);
                    }

                    CheckAndSendEmail();
                    System.Timers.Timer aTimer = new System.Timers.Timer(60 * 60 * 1000); //one hour in milliseconds
                    aTimer.Elapsed += OnTimedIpUpdaterEvent;
                    _alreadyStarted = true;
                }
            }
        }

        private static bool _alreadyStarted;

        private static bool CheckIpChange(/*bool forced = false*/)
        {
            IEnumerable<IPAddress> cachedIps = CachedValues.GetCachedIPs();
            System.Collections.Generic.HashSet<System.Net.IPAddress> newIps = GetNewIPs();
            if (newIps == null)
            {
                Log.WriteLine("IP Fetcher returned null! Woah! We don't have offline mode right now so something is very wrong!");
                return false;
            }
            var changed = !newIps.SetEquals(cachedIps);
            // Clear out cached IPs, otherwise we would return invalid IPs if we lose either (stack disabled, etc)
            if (changed)
            {
                changed = false;
                if (CachedValues.GetPreviousExternalIpAddressv6() != null)
                {
                    Log.WriteLine("Previous IPv6: " + CachedValues.GetPreviousExternalIpAddressv6());
                }

                if (CachedValues.GetPreviousExternalIpAddressv4() != null)
                {
                    Log.WriteLine("Previous IPv4: " + CachedValues.GetPreviousExternalIpAddressv4());
                }

                CachedValues.SetPreviousExternalIpAddressv6(CachedValues.GetCachedExternalIpAddressv6());
                CachedValues.SetPreviousExternalIpAddressv4(CachedValues.GetCachedExternalIpAddressv4());
                CachedValues.SetCachedExternalIpAddressv6(null);
                CachedValues.SetCachedExternalIpAddressv4(null);
                Log.WriteLine("Fetched IPs:" + string.Join(",", newIps));
                foreach (System.Net.IPAddress address in newIps)
                {
                    switch (address.AddressFamily)
                    {
                        case System.Net.Sockets.AddressFamily.InterNetworkV6:
                            // we have IPv6
                            if (CachedValues.GetPreviousExternalIpAddressv6() != null)
                            {
                                if (!address.Equals(CachedValues.GetPreviousExternalIpAddressv6()))
                                {
                                    changed = true;
                                    Log.WriteLine("IPv6 Changed (" + CachedValues.GetPreviousExternalIpAddressv6() + " => " + address + ")");
                                }
                            }
                            else
                            {
                                changed = true;
                            }

                            CachedValues.SetCachedExternalIpAddressv6(address);
                            break;

                        case System.Net.Sockets.AddressFamily.InterNetwork:
                            // we have IPv4
                            if (CachedValues.GetPreviousExternalIpAddressv4() != null)
                            {
                                if (!address.Equals(CachedValues.GetPreviousExternalIpAddressv4()))
                                {
                                    changed = true;
                                    Log.WriteLine("IPv4 Changed (" + CachedValues.GetPreviousExternalIpAddressv4() + " => " + address + ")");
                                }
                            }
                            else
                            {
                                changed = true;
                            }

                            CachedValues.SetCachedExternalIpAddressv4(address);
                            break;

                        default:
                            // Do nothing. Security wants this.
                            throw new NotSupportedException();
                    }
                }
            }
            if (changed)
            {
                Properties.Settings.Default.LastIPv4 = CachedValues.GetCachedExternalIpAddressv4()?.ToString();
                Properties.Settings.Default.LastIPv6 = CachedValues.GetCachedExternalIpAddressv6()?.ToString();
                Properties.Settings.Default.Save();
            }
            return changed;
        }

        private static System.Collections.Generic.HashSet<IPAddress> GetNewUpsFromLocalAdapters()
        {
            System.Collections.Generic.HashSet<System.Net.IPAddress> returnValue = new System.Collections.Generic.HashSet<System.Net.IPAddress>();
            try
            {
                // Get 'IPHostEntry' object containing information like host name, IP addresses, aliases for a host.
                //IPHostEntry hostInfo = Dns.GetHostByName(System.Environment.MachineName);
                IPAddress local = IPAddress.Parse("127.0.0.1");
                IPHostEntry hostInfo = Dns.GetHostEntry(local);
                Log.WriteLine("Host name : " + hostInfo.HostName);
                Log.WriteLine("IP address List : ");
                for (int index = 0; index < hostInfo.AddressList.Length; index++)
                {
                    IPAddress a = hostInfo.AddressList[index];
                    returnValue.Add(a);
                    Log.WriteLine(a.ToString());
                }
            }
            catch (System.Net.Sockets.SocketException e)
            {
                Log.WriteLine("SocketException caught!!!");
                Log.WriteLine("Source : " + e.Source);
                Log.WriteLine("Message : " + e.Message);
            }
            catch (System.ArgumentNullException e)
            {
                Log.WriteLine("ArgumentNullException caught!!!");
                Log.WriteLine("Source : " + e.Source);
                Log.WriteLine("Message : " + e.Message);
            }
            catch (System.Exception e)
            {
                Log.WriteLine("Exception caught!!!");
                Log.WriteLine("Source : " + e.Source);
                Log.WriteLine("Message : " + e.Message);
            }
            return returnValue;
        }

        // I don't know how to remove this method and pass CheckAndSendEmail() directly.
        private static void OnTimedIpUpdaterEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            CheckAndSendEmail();
        }

        protected class ImpatientWebClient : System.Net.WebClient
        {
            // We actually use this, but static analyzer thinks we don't.
            protected override WebRequest GetWebRequest(Uri uri)
            {
                System.Net.WebRequest w = base.GetWebRequest(uri);
                w.Timeout = 1000 * 10; // 5 seconds
                return w;
            }
        }
    }
}

