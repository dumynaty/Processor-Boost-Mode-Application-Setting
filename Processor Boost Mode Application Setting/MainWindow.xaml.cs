using Microsoft.VisualBasic;
using Processor_Boost_Mode_Application_Setting;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Windows;
using System.Windows.Controls;
using MessageBox = System.Windows.MessageBox;

namespace ProcessBoostManager
{
    public partial class MainWindow : Window
    {
        private readonly RegistryStartupManager startupManager = new();
        public ObservableCollection<ProgramModel> ProgramsInUI= new();
        private readonly System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            ProcessSelectionWindow.MainWindowInstance = this;
            App.MainWindowInstance = this;

            // Initialize checkbox
            AutostartCheckBox.IsChecked = startupManager.IsAutostartEnabled();

            // Initialize TextBoxHandling Class     ----->     CHECK CODE AND INSTANTIATION N2L
            TextBoxHandling.MainWindowInstance = this;

            // Prepare the active GUID Value
            GUIDHandling.getGUIDs();

            // Display Database programs on Window ListBox
            ProcessListBox.ItemsSource = ProgramsInUI;
            UpdateUI();

            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                UpdateUI();
            };
            timer.Start();
        }


        // CheckBoxes Methods
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            startupManager.RegisterStartup();
            TextBoxHandling.Upper("Application is registered to starting with Windows!");
        } 
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            startupManager.UnregisterStartup();
            TextBoxHandling.Upper("Application is unregistered from starting with Windows!");
        }

        // Event actions for the buttons
        public void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessSelectionWindow selection = new();
            selection.Show();
        }
        private void AddManuallyButton_Click(object sender, RoutedEventArgs e)
        {
            JsonService.ManualAdd();
            ProgramsInUI.Clear();
            UpdateUI();
        }
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            // Remove from lists but also from file. Call removefromfile function with selectedProgram argument
            if (ProcessListBox.SelectedItem is ProgramModel selectedProgram)
            {
                JsonService.RemoveProcess(selectedProgram);
                TextBoxHandling.Lower("Process " + selectedProgram.Name + " has been removed!");
                ProgramsInUI.Remove(selectedProgram);
            }
            else
            {
                TextBoxHandling.Lower("Please select a program to remove!");
            }
        }
        private void ExtraButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        public void UpdateUI()
        {
            var currentBoostModeHighestValue = Processes.highestBoostModeValue;
            var ProgramsInDatabase = Processes.GetInitialListOfPrograms();
            var newBoostModeHighestValue = Processes.highestBoostModeValue;
            ProgramsInDatabase = ProgramsInDatabase.OrderByDescending(p => p.IsRunning).ThenByDescending(p => p.HighestValue).ThenBy(p => p.Name).ToList();
            bool differentList = false;

            if (ProgramsInUI.Count == 0)
            {
                foreach (var program in ProgramsInDatabase)
                {
                    ProgramsInUI.Add(program);
                }
            }
            else
            {
                for (int i = 0; i < ProgramsInDatabase.Count; i++)
                {
                    if (ProgramsInUI[i].IsRunning != ProgramsInDatabase[i].IsRunning)
                    {
                        differentList = true;
                        break;
                    }
                }
                if (!differentList)
                    return;

                ProgramsInUI.Clear();
                foreach (var program in ProgramsInDatabase)
                {
                    ProgramsInUI.Add(program);
                }
            }

            if (currentBoostModeHighestValue != newBoostModeHighestValue)
            {
                GUIDHandling.setGUID();
                App.trayIcon.ShowBalloonTip(2500, "Status change:", $"Current mode set to: " +
                    $"{Processes.highestBoostModeValue.ToString()}", ToolTipIcon.None);
            }
        }

        private void ProcessListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ProcessListBox.SelectedItem is ProgramModel selectedProcess)
            {
                TextBoxHandling.Lower("Process selected: " + selectedProcess.Name, true);
            }
        }
        private void ProcessListBox_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ProcessListBox.SelectedItem != null)
            {
                var clickedItem = ProcessListBox.ContainerFromElement((DependencyObject)e.OriginalSource) as ListBoxItem;
                if (clickedItem != null && clickedItem.IsSelected)
                {
                    ProcessListBox.SelectedItem = null;
                    TextBoxHandling.Lower("Current mode: "+ Processes.highestBoostModeValue, true);
                    e.Handled = true;
                }
            }
        }


        public bool comboBoxIsSelectedByUser = false; 
        private void ProcessListBox_ComboBoxPreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            comboBoxIsSelectedByUser = true;
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxIsSelectedByUser)
            {
                List<ProgramModel> displayedList = ProgramsInUI.ToList();
                JsonService.SavePrograms(displayedList);
                ProgramsInUI.Clear();
                UpdateUI();
                TextBoxHandling.Lower("Current mode: " + Processes.highestBoostModeValue, true);
                comboBoxIsSelectedByUser = false;
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            var x = this.WindowState;
            if (x == WindowState.Minimized)
            {
                this.Hide();
            }
        }
    }
}