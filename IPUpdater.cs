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
        public class CachedValues
        {
            private static IPAddress cachedExternalIpAddressv4;
            private static IPAddress cachedExternalIpAddressv6;
            private static IPAddress previousExternalIpAddressv4;
            private static IPAddress previousExternalIpAddressv6;

            public static IPAddress CachedExternalIpAddressv4 { get => cachedExternalIpAddressv4; set => cachedExternalIpAddressv4 = value; }
            public static IPAddress CachedExternalIpAddressv6 { get => cachedExternalIpAddressv6; set => cachedExternalIpAddressv6 = value; }
            public static IPAddress PreviousExternalIpAddressv4 { get => previousExternalIpAddressv4; set => previousExternalIpAddressv4 = value; }
            public static IPAddress PreviousExternalIpAddressv6 { get => previousExternalIpAddressv6; set => previousExternalIpAddressv6 = value; }

            public static IEnumerable<IPAddress> GetCachedIPs()
            {
                return new System.Collections.Generic.HashSet<System.Net.IPAddress> { CachedExternalIpAddressv4, CachedExternalIpAddressv6 };
            }
        }

        public static void CheckAndSendEmail(bool forceSend = false)
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
            // This returns the adapter(s)' local IPs. Not what we want, but might be useful later.
            //System.Collections.Generic.HashSet<IPAddress> result = GetNewUpsFromLocalAdapters();
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
                    fetchedV6 = new ImpatientWebClient().DownloadString("https://ipv6.icanhazip.com").Trim();
                }
                catch (System.Exception ex)
                {
                    Log.WriteLine(ex.ToString());
                }
                finally
                {
                    //if (v6 != null)
                    {
                        var isValid = System.Net.IPAddress.TryParse(fetchedV6, out v6);
                        if (v6 != null)
                        {
                            if (isValid && v6.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                            {
                                //Log.WriteLine("IPV6: " + v6.ToString());
                            }
                            else if (v6.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                            {
                                // ipv6 returned an ipv4 address.
                                // Save one lookup and use it in-place.
                                fetchedV4 = fetchedV6;
                                v6 = null;
                                Log.WriteLine("Recycling returned IPv4 address...");
                            }
                        }
                        else
                        {
                            Log.WriteLine("Fetching IPv6 failed");
                        }
                    }
                }
                if (string.IsNullOrEmpty(fetchedV4))
                {
                    try
                    {
                        Log.WriteLine("Fetching IPv4 Address...");
                        fetchedV4 = new ImpatientWebClient().DownloadString("https://ipv4.icanhazip.com").Trim();
                    }
                    catch (System.Exception ex)
                    {
                        Log.WriteLine("Fetching IPv4 failed");
                        Log.WriteLine(ex.ToString());
                    }
                    finally
                    {
                        if (System.Net.IPAddress.TryParse(fetchedV4, out v4))
                        {
                            //Log.WriteLine("V4 address passes validity checks");
                            //Log.WriteLine("IPV4: " + v4.ToString());
                        }
                    }
                }

                System.Collections.Generic.HashSet<System.Net.IPAddress> set = new System.Collections.Generic.HashSet<System.Net.IPAddress>();
                if (v4 != null)
                {
                    set.Add(v4);
                    //Log.WriteLine("Stored IPv4");
                }
                if (v6 != null)
                {
                    set.Add(v6);
                    //Log.WriteLine("Stored IPv6");
                }
                returnValue = set;
            }
            catch (System.Exception ex)
            {
                Log.WriteLine(ex.ToString());
            }
            return returnValue;
        }

        public static void Main(string[] args)
        {
            SetUpTimer();
            // Keep running
            while (true) { /*no-op*/}
        }

        public static void SetUpTimer(bool retry = false)
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
                        CachedValues.CachedExternalIpAddressv6 = v6;
                        CachedValues.PreviousExternalIpAddressv6 = v6;
                    }
                    if (settingsIpv4 != null && System.Net.IPAddress.TryParse(settingsIpv4, out System.Net.IPAddress v4))
                    {
                        CachedValues.CachedExternalIpAddressv4 = v4;
                        CachedValues.PreviousExternalIpAddressv4 = v4;
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
            //Log.WriteLine("Cached IPs:" + String.Join(",",cached_ips));
            // TODO: Compares hashed lists instead of creating these
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
                if (CachedValues.PreviousExternalIpAddressv6 != null)
                {
                    Log.WriteLine("Previous IPv6: " + CachedValues.PreviousExternalIpAddressv6);
                }

                if (CachedValues.PreviousExternalIpAddressv4 != null)
                {
                    Log.WriteLine("Previous IPv4: " + CachedValues.PreviousExternalIpAddressv4);
                }

                CachedValues.PreviousExternalIpAddressv6 = CachedValues.CachedExternalIpAddressv6;
                CachedValues.PreviousExternalIpAddressv4 = CachedValues.CachedExternalIpAddressv4;
                CachedValues.CachedExternalIpAddressv6 = null;
                CachedValues.CachedExternalIpAddressv4 = null;
                Log.WriteLine("Fetched IPs:" + string.Join(",", newIps));
                foreach (System.Net.IPAddress address in newIps)
                {
                    //Log.WriteLine("Processing " + address.ToString());
                    switch (address.AddressFamily)
                    {
                        case System.Net.Sockets.AddressFamily.InterNetworkV6:
                            // we have IPv6
                            if (CachedValues.PreviousExternalIpAddressv6 != null)
                            {
                                if (!address.Equals(CachedValues.PreviousExternalIpAddressv6))
                                {
                                    changed = true;
                                    Log.WriteLine("IPv6 Changed (" + CachedValues.PreviousExternalIpAddressv6 + " => " + address + ")");
                                }
                            }
                            else
                            {
                                changed = true;
                            }

                            CachedValues.CachedExternalIpAddressv6 = address;
                            break;

                        case System.Net.Sockets.AddressFamily.InterNetwork:
                            // we have IPv4
                            if (CachedValues.PreviousExternalIpAddressv4 != null)
                            {
                                if (!address.Equals(CachedValues.PreviousExternalIpAddressv4))
                                {
                                    changed = true;
                                    Log.WriteLine("IPv4 Changed (" + CachedValues.PreviousExternalIpAddressv4 + " => " + address + ")");
                                }
                            }
                            else
                            {
                                changed = true;
                            }

                            CachedValues.CachedExternalIpAddressv4 = address;
                            break;

                        default:
                            // Do nothing. Security wants this.
                            break;
                    }
                }
            }
            if (changed)
            {
                Properties.Settings.Default.LastIPv4 = CachedValues.CachedExternalIpAddressv4?.ToString();
                Properties.Settings.Default.LastIPv6 = CachedValues.CachedExternalIpAddressv6?.ToString();
                Properties.Settings.Default.Save();
            }
            //Log.WriteLine("Properties.Settings.Default.LastIPv6 = " + Properties.Settings.Default.LastIPv6);
            //Log.WriteLine("CachedValues.CachedExternalIPAddressv6 = " + CachedValues.CachedExternalIPAddressv6.ToString());
            return changed;
        }

        private static System.Collections.Generic.HashSet<IPAddress> GetNewUpsFromLocalAdapters()
        {
            System.Collections.Generic.HashSet<System.Net.IPAddress> returnValue = new System.Collections.Generic.HashSet<System.Net.IPAddress>();
#if REQUIRE_INTERNET
            try
            {
                // IPV4
                using (var socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Dgram, 0))
                {
                    // Connect socket to Google's Public DNS service
                    socket.Connect("8.8.8.8", 65530);
                    if (!(socket.LocalEndPoint is IPEndPoint endPoint))
                    {
                        throw new System.InvalidOperationException($"Error occurred casting {socket.LocalEndPoint} to IPEndPoint");
                    }
                    returnValue.Add(endPoint.Address);
                }
                // IPV6
                using (var socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetworkV6, System.Net.Sockets.SocketType.Dgram, 0))
                {
                    // Connect socket to Google's Public DNS service
                    socket.Connect("2001:4860:4860::8888", 65530);
                    if (!(socket.LocalEndPoint is IPEndPoint endPoint))
                    {
                        throw new System.InvalidOperationException($"Error occurred casting {socket.LocalEndPoint} to IPEndPoint");
                    }
                    returnValue.Add(endPoint.Address);
                }
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                // Handle exception
                Log.WriteLine(ex.ToString());
            }

#else
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
#endif
            return returnValue;
        }

        private static void OnTimedIpUpdaterEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            CheckAndSendEmail();
        }

        private class ImpatientWebClient : System.Net.WebClient
        {
            protected override System.Net.WebRequest GetWebRequest(System.Uri uri)
            {
                System.Net.WebRequest w = base.GetWebRequest(uri);
                w.Timeout = 1000 * 5; // 5 seconds
                return w;
            }
        }
    }
}

