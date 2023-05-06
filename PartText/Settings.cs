using UnityEngine;
using SFS.Parsers.Json;
using SFS.Input;
using ModLoader;
using ModLoader.Helpers;

namespace PartText
{
    public static class SettingsManager
    {
        public static Settings settings;
        public static void Save()
        {
            if (UI.window != null)
                settings.windowMinimized = UI.window.Minimized;
            JsonWrapper.SaveAsJson(Main.SettingsFilePath, settings, true);
        }
        public static void Load()
        {
            if (!JsonWrapper.TryLoadJson(Main.SettingsFilePath, out settings))
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

    // public class KeybindsManager : ModKeybindings
    // {
    //     public KeybindingsPC.Key Key_SavePart = KeybindingsPC.Key.Ctrl_(KeyCode.S);

    //     public static KeybindsManager keybindsManager;
	// 	public static void Setup()
    //     {
    //         keybindsManager = SetupKeybindings<KeybindsManager>(Main.main);
    //     }
    //     public override void CreateUI()
    //     {
    //         KeybindsManager defaults = new KeybindsManager();
	// 		CreateUI_Text("Part Text");
	// 		CreateUI_Keybinding(Key_SavePart, defaults.Key_SavePart, "Save Part");
    //     }
    // }
}