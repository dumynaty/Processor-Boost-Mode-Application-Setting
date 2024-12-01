using System.Text.Json;
using System.IO;
using MessageBox = System.Windows.MessageBox;
using System.Diagnostics;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Collections.ObjectModel;
using System;

namespace ProcessBoostManager
{
    public class Processes
    {
        public static string highestBoostModeValue = "Disabled";
        public static int runningProcessesAsNumberValue = 0;

        public static List<ProgramModel> GetListOfPrograms()
        {
            List<ProgramModel> programs = new List<ProgramModel>();
            string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database.json");
            if (!File.Exists(FilePath))
            {
                MessageBox.Show("Database not found! Creating one...");
            }
            else
            {
                try
                {
                    var jsonData = File.ReadAllText(FilePath);
                    programs = JsonSerializer.Deserialize<List<ProgramModel>>(jsonData) ?? new List<ProgramModel>();
                }
                catch (JsonException ex)
                {
                    MessageBox.Show($"Error deserializing JSON: {ex.Message}");
                    MessageBox.Show("Database file could be empty!");
                    programs = new List<ProgramModel>();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                    programs = new List<ProgramModel>();
                }
            }
            return programs;
        }

        public static List<ProgramModel> GetHighestBoostModeAndRunningProcesses(List<ProgramModel> programs)
        {
            highestBoostModeValue = "Disabled"; // Needs to be defined otherwise when all programs are changed to Disabled it will not refresh
            runningProcessesAsNumberValue = 0;
            var runningProcesses = Process.GetProcesses().Distinct().Select(p => p.ProcessName).ToList();

            programs ??= GetListOfPrograms();
            foreach (var program in programs)
            {
                if (runningProcesses.Contains(program.Name))
                {
                    program.IsRunning = true;
                    runningProcessesAsNumberValue++;
                    if (program.BoostMode == "Enabled" && highestBoostModeValue == "Disabled")
                    {
                        highestBoostModeValue = "Enabled";
                    }
                    else if (program.BoostMode == "Aggressive" && highestBoostModeValue != "Aggressive")
                    {
                        highestBoostModeValue = "Aggressive";
                    }
                }
            }
            return programs;
        }

        public static List<ProgramModel> SetIconAndHighestValue(List<ProgramModel> programs)
        {
            foreach (var program in programs)
            {
                program.Icon = ExtractIcon(program.Location);
                if (program.IsRunning == true && program.BoostMode == highestBoostModeValue && highestBoostModeValue != "Disabled")
                    program.HighestValue = true;
                else if (program.BoostMode != highestBoostModeValue)
                    program.HighestValue = false;
            }
            return programs;
        }


        // This can be made way more efficient
        public static List<ProgramModel> GetInitialListOfPrograms()
        {
            var ProgramsInDatabase = GetListOfPrograms();
            ProgramsInDatabase = GetHighestBoostModeAndRunningProcesses(ProgramsInDatabase);
            ProgramsInDatabase = SetIconAndHighestValue(ProgramsInDatabase);

            if (ProgramsInDatabase.Count == 0)
            {
                TextBoxHandling.Upper("Add programs to the list!");
                TextBoxHandling.Lower("No programs loaded. Database empty!");
            }
            else
            {
                TextBoxHandling.Upper("Current processes running: " + runningProcessesAsNumberValue, true);
            }
            return ProgramsInDatabase;
        }

        public static BitmapSource? ExtractIcon(string programLocation)
        {
            using (var extractedIcon = Icon.ExtractAssociatedIcon(programLocation))
            {
                if (extractedIcon != null)
                {
                    return Imaging.CreateBitmapSourceFromHIcon(
                           extractedIcon.Handle,
                           Int32Rect.Empty,
                           BitmapSizeOptions.FromEmptyOptions());
                }
            }
            return null;
        }
    }
}
