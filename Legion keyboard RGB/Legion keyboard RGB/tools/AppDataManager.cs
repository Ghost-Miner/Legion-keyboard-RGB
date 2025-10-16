using System.Text.Json;
using System.IO;
using System.Windows;

namespace Legion_keyboard_RGB.tools
{
    public static class AppDataManager
    {
        private static readonly string appDataFolderPath = Environment.GetEnvironmentVariable("appdata") + "/GhostMiner/Ambient KB/";
        private static readonly string appDataFileName = "settings.json";  


        public static void SaveSettingsData (SettingsDataClass settingsData)
        {
            string json = JsonSerializer.Serialize(settingsData);

            File.WriteAllText(appDataFolderPath + appDataFileName, json);
            //MessageBox.Show($"Saved {json} to {appDataFolderPath + appDataFileName}");
        }

        public static SettingsDataClass GetSettingsData()
        {
            string json = File.ReadAllText(appDataFolderPath + appDataFileName);

            SettingsDataClass? settingsData = JsonSerializer.Deserialize<SettingsDataClass>(json);
            //MessageBox.Show($"Loaded {json} from {appDataFolderPath + appDataFileName}");
            return settingsData;
        }

        public static bool SettingsDataExists()
        {
            return File.Exists(appDataFolderPath + appDataFileName);
        }

        public class SettingsDataClass
        {
            public float _saturationValue { get; set; }
            public float _lightnessValue { get; set; }
            public float _desiredFrameRate { get; set; }
            public int   _topMargin { get; set; }
            public int   _bottomMargin { get; set; }
        }
    }
}
