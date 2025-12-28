using System;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using TMPro;
using UITools;
using SFS.Parts;
using SFS.World;
using SFS.Builds;
using SFS.UI.ModGUI;
using SFS.Parsers.Json;
using SFS.Parts.Modules;
using Type = SFS.UI.ModGUI.Type;
using Object = UnityEngine.Object;

namespace PartText
{
    public static class UI
    {
        static readonly int windowID = Builder.GetRandomID();
        public static GameObject windowHolder;
        public static ClosableWindow window;
        public static TextInput input;
        public static Button savePartButton;
        public static Label noSelectedPartText;
        public static Part currentPart;
        static Color defaultWindowColor;
        static bool changingPart = false;
        public static bool editingText = false;

        public static void OnBuildLoaded()
        {
            SettingsManager.Load();
            BuildManager.main.selector.onSelectedChange += UpdateCurrentPart;

            Vector2Int windowSize = Vector2Int.RoundToInt(SettingsManager.settings.windowSize);
            windowHolder = Builder.CreateHolder(Builder.SceneToAttach.CurrentScene, "Part Text GUI Holder");
            window = UIToolsBuilder.CreateClosableWindow
            (
                windowHolder.transform, windowID,
                windowSize.x,
                windowSize.y,
                100,
                100,
                true,
                true,
                SettingsManager.settings.windowOpacity,
                "Part Text",
                false
            );
            window.RegisterPermanentSaving(Main.main.ModNameID);
            window.RegisterOnDropListener(SettingsManager.Save);
            window.RegisterOnDropListener(() => KeepWindowInView(window));
            defaultWindowColor = window.WindowColor;
            window.CreateLayoutGroup(Type.Vertical, spacing: 5, padding: new RectOffset(5, 5, 5, 5));

            noSelectedPartText = Builder.CreateLabel(window, windowSize.x - 20, windowSize.y / 12, text: "Select a part...");

            input = Builder.CreateTextInput(window, windowSize.x - 10, windowSize.y - 100, text: "", onChange: (string _) => { RemoveInvalidCharacters(); UpdateColor(); });
            input.field.onFocusSelectAll = false;
            input.field.onSelect.AddListener((string _) => { UpdateCurrentPart(); });
            input.field.onSelect.AddListener((_) => input.field.caretPosition = TMP_TextUtilities.GetCursorIndexFromPosition(input.field.textComponent, (Vector2)Input.mousePosition, null));
            input.field.onSelect.AddListener((string _) => editingText = true);
            input.field.onDeselect.AddListener((string _) => editingText = false);
            
            input.field.caretWidth = 2;
            input.field.pointSize = SettingsManager.settings.textSize;
            input.field.textComponent.alignment = TextAlignmentOptions.TopLeft;
            input.field.lineType = TMP_InputField.LineType.MultiLineNewline;
            input.Active = false;

            savePartButton = Builder.CreateButton(window, windowSize.x - 20, 30, onClick: ApplyChanges, text: "Save Part");
            savePartButton.Active = false;

            window.Minimized = SettingsManager.settings.windowMinimized;
        }

        public static void OnBuildUnloaded()
        {
            SettingsManager.Save();
            Object.Destroy(windowHolder);
            editingText = false;
        }

        public static void ApplyChanges()
        {
            if (CheckValidJson(input.Text))
            {
                changingPart = true;
                Undo.main.CreateNewStep("Edit Part");
                bool clippingCheat = SandboxSettings.main.settings.partClipping;
                SandboxSettings.main.settings.partClipping = false;

                currentPart.aboutToDestroy?.Invoke(currentPart);
                BuildManager.main.buildGrid.RemoveParts(enableNonIntersecting: true, applyUndo: true, currentPart);
                BuildManager.main.selector.Deselect(currentPart);
                currentPart.DestroyPart(createExplosion: false, updateJoints: false, DestructionReason.Intentional);

                PartSave newPartSave = JsonWrapper.FromJson<PartSave>(input.Text);
                Part newPart = PartsLoader.CreatePart(newPartSave, BuildManager.main.buildGrid.activeGrid.transform, BuildManager.main.buildGrid.activeGrid.selectedLayer, OnPartNotOwned.Delete, out OwnershipState _);
                BuildManager.main.buildGrid.AddParts(true, false, true, newPart);
                SandboxSettings.main.settings.partClipping = clippingCheat;
                changingPart = false;
                BuildManager.main.selector.Select(newPart);
            }
            else
            {
                SFS.UI.MsgDrawer.main.Log("Part JSON is invalid");
            }
        }

        static bool CheckValidJson(string json)
        {
            bool successfulJsonConversion = false;
            try
            {
                successfulJsonConversion = JsonConvert.DeserializeObject<PartSave>(json) != null;
            } catch {}
            return successfulJsonConversion;
        }

        static void UpdateColor()
        {
            window.WindowColor = currentPart != null && !CheckValidJson(input.Text) ? Color.red : defaultWindowColor;
        }

        public static void UpdateCurrentPart()
        {
            try
            {
                if (changingPart)
                    return;
                
                currentPart = BuildManager.main.selector.selected.Count == 1 ? BuildManager.main.selector.selected.First() : null;
                
                window.WindowColor = defaultWindowColor;
                noSelectedPartText.Active = currentPart == null;
                input.Active = currentPart != null;
                savePartButton.Active = currentPart != null;

                if (currentPart != null)
                {
                    input.Text = JsonWrapper.ToJson(new PartSave(currentPart), true);
                }
                else
                {
                    input.Text = "";
                }
            } catch (NullReferenceException) { }

        }

        static void KeepWindowInView(Window window)
        {
            try
            {
                RectTransform rect = window.rectTransform;
                Vector2 center = rect.rect.center / 2;
                rect.position = Vector2.Max((Vector2)rect.position + center, Vector2.zero) - center;
                rect.position = Vector2.Min((Vector2)rect.position + center, new Vector2(Screen.width, Screen.height)) - center;
            } catch {}
        }

        static string RemoveInvalidCharacters()
        {
            return input.Text = input.Text.Replace("", "");
        }
    }
}