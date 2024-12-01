namespace ProcessBoostManager
{
    public static class TextBoxHandling
    {
        public static MainWindow? MainWindowInstance { get; set; }
        private static string permanentMessage = "";
        private static readonly System.Windows.Threading.DispatcherTimer timerLower = new System.Windows.Threading.DispatcherTimer();
        private static readonly System.Windows.Threading.DispatcherTimer timerUpper = new System.Windows.Threading.DispatcherTimer();

        public static void Upper(string message, bool priority = false)
        {
            if (MainWindowInstance != null)
            {
                if (!priority)
                {
                    MainWindowInstance.UpperMainInfo.Text = message;

                    timerUpper.Interval = TimeSpan.FromSeconds(5);
                    timerUpper.Tick += (s, e) =>
                    {
                        if (MainWindowInstance.UpperMainInfo.Text == message)
                            MainWindowInstance.UpperMainInfo.Text = permanentMessage;
                        timerUpper.Stop();
                    };
                    timerUpper.Start();
                }
                else 
                {
                    timerUpper.Stop();
                    permanentMessage = message;
                    MainWindowInstance.UpperMainInfo.Text = permanentMessage;
                }
            }
        }

        public static void Lower(string message, bool permanent = false)
        {
            if (MainWindowInstance != null)
            {
                if (!permanent)
                {
                    MainWindowInstance.LowerMainInfo.Text = message;

                    timerLower.Interval = TimeSpan.FromSeconds(5);
                    timerLower.Tick += (s, e) =>
                    {
                        MainWindowInstance.LowerMainInfo.Text = permanentMessage;
                        timerLower.Stop();
                    };
                    timerLower.Start();
                }
                else
                {
                    permanentMessage = message;
                    MainWindowInstance.LowerMainInfo.Text = permanentMessage;
                }
            }
        }
    }
}