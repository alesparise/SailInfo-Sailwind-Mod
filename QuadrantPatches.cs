/*using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinchInfoSail
{
    internal static class QuadrantPatches
    {
        //quadrant
        //private static bool quadrantHeld;
        static bool wasHeld = false;
        [HarmonyPostfix]
        public static void QuadrantPatch(bool ___held)
        {   //shows either the latitude or the reading when inspecting the quadrant
            //DEBUG: ISSUE: Using recoveryText like this breaks all other instances of recovery text...
            //SOLUTION: Maybe I don't need this to be in the Update patch! If I only show the lat...
            //          If I wanna show the reading though, the UpdatePatch would be required I guess...
            //          Maybe I can create a copy of recoveryText?
            //RELEASE: all the Quadrant Patches are left there but not implemented. Might become useful in the future.
            if (!SailInfoMain.showLatitudeConfig.Value)
            {
                wasHeld = false;
                return;
            }
            if (___held)
            {
                wasHeld = true;
                Sleep.instance.recoveryText.text = $"<size=20%>\n\n\nLat: {SailInfoPatches.Latitude()}</size>";
            }
            else if (wasHeld)
            {
                wasHeld = false;
                Sleep.instance.recoveryText.text = "";
            }
        }
        [HarmonyPrefix]
        public static void QuadrantOnPickupPatch()
        {   //detects the quadrant being picked up
            quadrantHeld = true;
        }
        [HarmonyPrefix]
        public static void QuadrantOnDropPatch()
        {   //detects the quadrant being dropped or put in the inventory
            quadrantHeld = false;
        }
        [HarmonyPrefix]
        public static void QuadrantLeaveInventoryPatch()
        {   //detects quadrant leaving the inventory
            quadrantHeld = true;
        }

    }
}
*/