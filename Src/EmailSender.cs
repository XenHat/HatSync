// This is an independent project of an individual developer. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Net.Mail;

namespace HatSync
{
    internal static class EmailSender
    {
        // TODO: Replace with a message to a mail server to queue e-mail for provided user ID
        internal static void SendEmail(bool forced = false)
        {
            
            var SettingsHost = Properties.Settings.Default.EmailProvider;
            var SettingsPort = Properties.Settings.Default.EmailPort;
            if (SettingsHost.Equals(null) ||
                SettingsPort.CompareTo(1) < 1 ||
                Creds.FromPassword.Equals("") ||
                Creds.FromAddress.Equals(null) ||
                Creds.ToAddress.Equals(null))
            {
                return;
            }
            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient
            {
                Host = Properties.Settings.Default.EmailProvider,
                Port = Properties.Settings.Default.EmailPort,
                EnableSsl = true,
                DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                Credentials = new System.Net.NetworkCredential(Creds.FromAddress.Address, Creds.FromPassword),
                Timeout = 20000
            };
            // construct body out of function to do some string logic
            var newBody = "";
            // TODO: Give a slightly friendler version when it's the first time
            // the client is run (or settings are gone?)
            // This needs a database, probably. Maybe. Optionally.
            // Remember, the idea is to rely on as little external communications as possible because
            // we're trying to be as independant as possible.
            // Ideally we would run on a LAN without internet access at all.
            // This means that client discovery is done entirely by the client
            // itself and no external storage is needed.
            // GOOD LUCK FUTURE ME.
            if (null != IPUpdater.CachedValues.GetCachedExternalIpAddressv6() && null != IPUpdater.CachedValues.GetPreviousExternalIpAddressv6())
            {// IP existed
                if (IPUpdater.CachedValues.GetPreviousExternalIpAddressv6().Equals(IPUpdater.CachedValues.GetCachedExternalIpAddressv6()))
                {// Value changed
                    newBody = "New IPv6 Address: " + IPUpdater.CachedValues.GetCachedExternalIpAddressv6() + " (was " + IPUpdater.CachedValues.GetCachedExternalIpAddressv6() + ")";
                }
                else
                {// Value is the same
                    newBody = "IPv6 Address (no change): " + IPUpdater.CachedValues.GetCachedExternalIpAddressv6();
                }
            }
            else if (null == IPUpdater.CachedValues.GetPreviousExternalIpAddressv6() && null != IPUpdater.CachedValues.GetCachedExternalIpAddressv6())
            {
                // IP didn't exist and was added
                newBody = "IPv6 Address (added): " + IPUpdater.CachedValues.GetCachedExternalIpAddressv6();
            }
            else if (null == IPUpdater.CachedValues.GetCachedExternalIpAddressv6() && null == IPUpdater.CachedValues.GetPreviousExternalIpAddressv6())
            {
                // IP disappeared!
                newBody = "IPv6 Address has been unassigned";
            }
            if (null != IPUpdater.CachedValues.GetCachedExternalIpAddressv4() && null != IPUpdater.CachedValues.GetPreviousExternalIpAddressv4())
            {// IP existed
                if (IPUpdater.CachedValues.GetPreviousExternalIpAddressv4().Equals(IPUpdater.CachedValues.GetCachedExternalIpAddressv4()))
                {// Value changed
                    newBody = "New IPv4 Address: " + IPUpdater.CachedValues.GetCachedExternalIpAddressv4() + " (was " + IPUpdater.CachedValues.GetCachedExternalIpAddressv4() + ")";
                }
                else
                {// Value is the same
                    newBody = "IPv4 Address (no change): " + IPUpdater.CachedValues.GetCachedExternalIpAddressv4();
                }
            }
            else if (null == IPUpdater.CachedValues.GetPreviousExternalIpAddressv4() && null != IPUpdater.CachedValues.GetCachedExternalIpAddressv4())
            {
                // IP didn't exist and was added
                newBody = "IPv4 Address (added): " + IPUpdater.CachedValues.GetCachedExternalIpAddressv4();
            }
            else if (null == IPUpdater.CachedValues.GetCachedExternalIpAddressv4() && null == IPUpdater.CachedValues.GetPreviousExternalIpAddressv4())
            {
                // IP disappeared!
                newBody = "IPv4 Address has been unassigned";
            }
            if (string.IsNullOrEmpty(newBody))
            {
                newBody = "NO IP FOUND (How did I even send this E-Mail?!)";
            }
            newBody = "Your sync client running on " + System.Environment.MachineName +
                " has " + (forced ? "requested an IP update" : "obtained new IP addresses") + ":" + System.Environment.NewLine + newBody + System.Environment.NewLine + System.Environment.NewLine + "Report generated by " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + " version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version + ".";
            using (System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage(Creds.FromAddress.ToString(), Creds.ToAddress.ToString())
            {
                Subject = System.Environment.MachineName + " IP Update",
                Body = newBody
            })
            {
                Log.WriteLine("Sending email" + (forced ? " (forced)" : ""));
                smtp.Send(message);
            }
        }

        private static class Creds
        {
            // TODO: Configurable
            public static string FromPassword => Properties.Settings.Default.EmailPass;

            public static MailAddress FromAddress => new System.Net.Mail.MailAddress(Properties.Settings.Default.EmailFrom, "HatSync notification service");

            public static MailAddress ToAddress => new System.Net.Mail.MailAddress(Properties.Settings.Default.EmailTo, Properties.Settings.Default.EmailToName);
        }
    }
}
