using System.Collections.Generic;
using HarmonyLib;
using SFS.Builds;
using SFS.Input;
using SFS.Parts;
using SFS.Parts.Modules;
using UnityEngine;

namespace PartText
{
    public static class Patches
    {
        [HarmonyPatch(typeof(BuildMenus), "TryDoubleClick")]
        class DisableDoubleClickSelect
        {
            static bool Prefix()
            {
                return !SettingsManager.settings.disableDoubleClickSelect;
            }
        }

        [HarmonyPatch(typeof(Screen_Game), nameof(Screen_Game.ProcessInput))]
        class WindowFocus
        {
            static bool Prefix()
            {
                return !UI.editingText;
            }
        }

        [HarmonyPatch(typeof(Part_Utility), nameof(Part_Utility.ApplyOrientationChange))]
        class FixFlippingAndMirroring
        {
            static bool Prefix(Orientation change, Vector2 pivot, IEnumerable<Part> parts)
            {
                if (!SettingsManager.settings.fixMirroringRotatedParts)
                    return true;
                Debug.Log($"{change.x}, {change.y}, {change.z}");
                

                foreach (Part part in parts)
                {
                    Orientation changeNew = change;
                    if (!(Mathf.Abs(part.orientation.orientation.Value.z % 90) < 0.0001))
                    {
                        if (changeNew == new Orientation(-1,1,0))
                            changeNew = new Orientation(1,-1,0);
                        
                        if (changeNew.x != 1 || changeNew.y != 1)
                            part.orientation.orientation.Value.z = 180 - part.orientation.orientation.Value.z;
                    }

                    part.orientation.orientation.Value += (part.orientation.orientation.Value.InversedAxis() ? new Orientation(changeNew.y, changeNew.x, changeNew.z) : changeNew);
                    part.transform.localPosition = ((Vector2)part.transform.localPosition - pivot) * changeNew + pivot;
                    part.RegenerateMesh();
                }
                return false;
            }
        }
    }
}