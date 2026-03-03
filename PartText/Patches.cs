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
        public static class DisableDoubleClickSelect
        {
            public static bool Prefix()
            {
                return !Settings.settings.disableDoubleClickSelect;
            }
        }

        [HarmonyPatch(typeof(Screen_Game), nameof(Screen_Game.ProcessInput))]
        public static class WindowFocus
        {
            public static bool Prefix()
            {
                return !UI.editingText;
            }
        }

        [HarmonyPatch(typeof(Part_Utility), nameof(Part_Utility.ApplyOrientationChange))]
        public class FixFlippingAndMirroring
        {
            public static bool Prefix(Orientation change, Vector2 pivot, IEnumerable<Part> parts)
            {
                if (!Settings.settings.fixMirroringRotatedParts)
                    return true;

                foreach (Part part in parts)
                {
                    Orientation changeNew = change;
                    if (!(Mathf.Abs(part.orientation.orientation.Value.z % 90) < 0.0001))
                    {
                        if (changeNew == new Orientation(-1,1,0))
                            changeNew = new Orientation(1,-1,0);
                        
                        if (!(Mathf.Approximately(changeNew.x, 1) && Mathf.Approximately(changeNew.y, 1)))
                            part.orientation.orientation.Value.z = 180 - part.orientation.orientation.Value.z;
                    }

                    part.orientation.orientation.Value += part.orientation.orientation.Value.InversedAxis() ? new Orientation(changeNew.y, changeNew.x, changeNew.z) : changeNew;
                    part.transform.localPosition = ((Vector2)part.transform.localPosition - pivot) * changeNew + pivot;
                    part.RegenerateMesh();
                }
                return false;
            }
        }
    }
}