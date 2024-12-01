using System.IO;
using System.Text.Json;


namespace ProcessBoostManager
{
    public class JsonService
    {
        public static string FilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Database.json");
        public static void AddToFile(ProgramModel program)
        {
            List<ProgramModel> programs;

            try
            {
                var jsonContent = File.ReadAllText(FilePath);

                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    programs = new List<ProgramModel>();
                }
                else
                {
                    programs = JsonSerializer.Deserialize<List<ProgramModel>>(jsonContent) ?? new List<ProgramModel>();
                }
            }
            catch (JsonException)
            {
                programs = new List<ProgramModel>();
            }

            if (programs.Contains(program))
                return;

            programs.RemoveAll(p => p.Name == program.Name);
            programs.Add(program);
            SavePrograms(programs);
            //MainWindow.Instance.UpdateList();
        }
        public static void ManualAdd()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new()
            {
                Title = "Select an Application",
                Filter = "Executable files (*.exe)|*.exe",
                CheckFileExists = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                string fileLocation = openFileDialog.FileName;
                var program = new ProgramModel
                {
                    Name = fileName,
                    Location = fileLocation,
                    BoostMode = "Disabled"
                };
                JsonService.AddToFile(program);
            }
        }
        public static void RemoveProcess(ProgramModel program)
        {
            List<ProgramModel> programs;
            var jsonContent = File.ReadAllText(FilePath);
            programs = JsonSerializer.Deserialize<List<ProgramModel>>(jsonContent) ?? new List<ProgramModel>();
            programs.RemoveAll(p => p.Name == program.Name);
            SavePrograms(programs);
        }
        public static void SavePrograms(List<ProgramModel>? programs = null)
        {
            if (programs == null)
            {
                var jsonContent = File.ReadAllText(FilePath);
                programs = JsonSerializer.Deserialize<List<ProgramModel>>(File.ReadAllText(FilePath)) ?? new List<ProgramModel>();
            }
            File.WriteAllText(FilePath, JsonSerializer.Serialize(programs, new JsonSerializerOptions
            {
                WriteIndented = true
            }));
        }
    }
}
