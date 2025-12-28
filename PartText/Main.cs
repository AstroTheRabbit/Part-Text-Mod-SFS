using System.Collections.Generic;
using HarmonyLib;
using ModLoader;
using ModLoader.Helpers;
using SFS.IO;
using UITools;

namespace PartText
{
    public class Main : Mod, IUpdatable
    {
        public static Main main;

        public override string ModNameID => "parttext";
        public override string DisplayName => "Part Text";
        public override string Author => "Astro The Rabbit";
        public override string MinimumGameVersionNecessary => "1.6.0";
        public override string ModVersion => "v1.2";
        public override string Description => "An alternative to the Part Editor mod that allows you to edit parts as text.";
        public override string IconLink => "https://i.imgur.com/ou34dVs.png";
        
        public override Dictionary<string, string> Dependencies { get; } = new Dictionary<string, string>
        {
            { "UITools", "1.1.5" }
        };
        public Dictionary<string, FilePath> UpdatableFiles => new Dictionary<string, FilePath> { 
        {
            "https://github.com/AstroTheRabbit/Part-Text-Mod-SFS/releases/latest/download/PartText.dll",
            new FolderPath(ModFolder).ExtendToFile("PartText.dll")
        } };

        public override void Early_Load()
        {
            new Harmony(ModNameID).PatchAll();
            main = this;
        }

        public override void Load()
        {
            SettingsManager.Load();
            if (SettingsManager.settings.windowEnabled)
            {
                SceneHelper.OnBuildSceneLoaded += UI.OnBuildLoaded;
                SceneHelper.OnBuildSceneUnloaded += UI.OnBuildUnloaded;
            }
        }
    }
}