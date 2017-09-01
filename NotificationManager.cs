namespace MaxwellGPUIdle
{
    public class NotificationManager
    {
        public static void PushNotificationToOS(string content, string title = "")
        {
            if (!Properties.Settings.Default.ShowNotifications)
            {
                return;
            }
            if (title == "")
            {
                title = Program.ProductName;
            }
            ProcessIcon.ni.BalloonTipTitle = title;
            ProcessIcon.ni.BalloonTipText = content;
            ProcessIcon.ni.ShowBalloonTip(1);
        }
    }
}
