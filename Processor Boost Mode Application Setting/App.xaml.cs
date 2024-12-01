using ProcessBoostManager;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace Processor_Boost_Mode_Application_Setting
{
    public partial class App : System.Windows.Application
    {
        public static NotifyIcon trayIcon = new();
        public static MainWindow? MainWindowInstance { get; set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow mainWindow = new MainWindow();
            if (mainWindow.AutostartCheckBox.IsChecked == true)
                mainWindow.Hide();
            else
                mainWindow.Show();

            trayIcon.Text = "Processor Boost Mode";
            trayIcon.Icon = new Icon("Applications Boost Mode.ico");
            trayIcon.Visible = true;
            trayIcon.MouseClick += TrayIcon_MouseClick;
        }

        private void TrayIcon_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                MainWindow.Show();
                MainWindow.WindowState = WindowState.Normal;
                MainWindow.Activate();
            }
            if (e.Button == MouseButtons.Right)
            {
                trayIcon.ContextMenuStrip?.Show();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            trayIcon.Dispose();
            if (MainWindowInstance != null)
            {
                List<ProgramModel> displayedList = MainWindowInstance.ProgramsInUI.ToList();
                JsonService.SavePrograms(displayedList);
            }
            base.OnExit(e);
        }
    }

}