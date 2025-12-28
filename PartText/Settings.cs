using UnityEngine;
using SFS.Parsers.Json;

namespace PartText
{
    public static class SettingsManager
    {
        public static Settings settings;
        private static IFile SettingsPath => new DefaultFolder(Main.main.ModFolder).GetFile("Settings.txt");
        public static void Save()
        {
            if (UI.window != null)
                settings.windowMinimized = UI.window.Minimized;
            JsonWrapper.SaveAsJson(SettingsPath, settings, true);
        }
        public static void Load()
        {
            if (!JsonWrapper.TryLoadJson(SettingsPath, out settings))
            {
                settings = new Settings();
                Save();
            }
        }
    }

    public class Settings {
        public bool disableDoubleClickSelect = false;
        public bool fixMirroringRotatedParts = true;
        public bool windowEnabled = true;
        public Vector2 windowSize = new Vector2(400, 600);
        public bool windowMinimized = false;
        public float windowOpacity = 1f;
        public int textSize = 20;
    }
}