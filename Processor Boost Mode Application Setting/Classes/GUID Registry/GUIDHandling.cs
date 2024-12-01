using Microsoft.Win32;
using System.Diagnostics;

namespace ProcessBoostManager
{
    public class GUIDHandling
    {
        public static string PowerPlanLocation = @"SYSTEM\CurrentControlSet\Control\Power\User\PowerSchemes\";
        public static string PROCESSOR_SUBGROUP_GUID = "54533251-82be-4824-96c1-47b60b740d00";
        public static string PROCESSOR_BOOST_MODE_GUID = "be337238-0d82-4146-a960-4f3749d470c7";
        public static string? CurrentGUID;
        static RegistryKey? rk;

        public static void getGUIDs()
        {
            using (rk = Registry.LocalMachine.OpenSubKey(PowerPlanLocation, false))
            {
                if (rk != null)
                {
                    var value = rk.GetValue("ActivePowerScheme");
                    if (value != null)
                    {
                        CurrentGUID = value.ToString();
                    }
                    else
                    {
                        MessageBox.Show("ActivePowerScheme value not found.");
                    }
                }
                else
                {
                    MessageBox.Show("Registry key not found.");
                }
            }
        }

        public static void setGUID()
        {
            int boostMode = getCurrentProcessorValue();
            if (boostMode == -1)
                { return; }

            try
            {
                if (string.IsNullOrEmpty(CurrentGUID))
                {
                    throw new Exception("Failed to retrieve current power scheme GUID");
                }
                
                string commands = $@"
                powercfg /setacvalueindex {CurrentGUID} {PROCESSOR_SUBGROUP_GUID} {PROCESSOR_BOOST_MODE_GUID} {boostMode};
                powercfg /setdcvalueindex {CurrentGUID} {PROCESSOR_SUBGROUP_GUID} {PROCESSOR_BOOST_MODE_GUID} 0;
                powercfg /setactive SCHEME_CURRENT
            ";

                var processInfo = new ProcessStartInfo("powershell.exe")
                {
                    Arguments = $"-NoProfile -NonInteractive -Command \"{commands}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Verb = "runas"
                };

                using var process = Process.Start(processInfo);
                process?.WaitForExit();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error applying boost mode: {ex.Message}");
            }
        }

        public static int getCurrentProcessorValue()
        {
            string valueCheck = PowerPlanLocation + CurrentGUID +"\\" + PROCESSOR_SUBGROUP_GUID + "\\" + PROCESSOR_BOOST_MODE_GUID;

            string highestValue = Processes.highestBoostModeValue;
            int boostModeMethod(string highestValue) =>
                highestValue switch
                {
                    "Disabled" => 0,
                    "Enabled" => 1,
                    "Aggressive" => 2,
                    _ => 0
                };
            int boostMode = boostModeMethod(highestValue);

            using (rk = Registry.LocalMachine.OpenSubKey(valueCheck, false))
            {
                if (rk != null)
                {
                    var value = rk.GetValue("ACSettingIndex");
                    if (value != null)
                    {
                        if ((int)value == boostMode)
                        {
                            return -1;
                        }
                        else
                        {
                            return boostMode;
                        }
                    }
                    MessageBox.Show("ACSettingIndex rk value null!");
                    return -1;
                }
                MessageBox.Show("rk value null! Check permissions!");
                return -1;
            }
        }

        public static string getCurrentProcessorBoostMode()
        {
            string valueCheck = PowerPlanLocation + CurrentGUID + "\\" + PROCESSOR_SUBGROUP_GUID + "\\" + PROCESSOR_BOOST_MODE_GUID;

            string highestValue;
            int highestValueAsInt = -1;

            using (rk = Registry.LocalMachine.OpenSubKey(valueCheck, false))
            {
                if (rk != null)
                {
                    var value = rk.GetValue("ACSettingIndex");
                    if (value != null)
                    {
                        highestValueAsInt = (int)value;
                    }
                    else
                        MessageBox.Show("ACSettingIndex rk value null!");
                }
                else
                    MessageBox.Show("rk value null! Check permissions!");
            }
            switch (highestValueAsInt)
            {
                case 0:
                    highestValue = "Disabled";
                    break;
                case 1:
                    highestValue = "Enabled";
                    break;
                case 2:
                    highestValue = "Aggressive";
                    break;
                default:
                    highestValue = "Disabled";
                    MessageBox.Show("Unexpected error in getCurrentProcessorBoostMode() occured.");
                    break;
            }
            return highestValue;
        }
    }
}
