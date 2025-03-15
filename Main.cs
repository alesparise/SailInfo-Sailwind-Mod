using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
//poorly written by pr0skynesis (discord username)

namespace SailInfo
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class SailInfoMain : BaseUnityPlugin
    {
        // Necessary plugin info
        public const string pluginGuid = "pr0skynesis.sailinfo";
        public const string pluginName = "SailInfo";
        public const string pluginVersion = "1.1.10"; //ALREADY CHANGED FOR CLOCK AND STUFF VERSION

        //config file info
        //MAIN SWITCHES
        public static ConfigEntry<bool> winchesInfoConfig;      //Disables the winches things entirely.
        public static ConfigEntry<bool> rudderHUDConfig;        //Disables the rudder HUD entirely.
        //winches stuff
        public static ConfigEntry<bool> winchesOutConfig;
        public static ConfigEntry<bool> winchesOutDegreeConfig;
        public static ConfigEntry<bool> winchesBarConfig;
        //Efficiency
        public static ConfigEntry<bool> sailEfficiencyConfig;
        public static ConfigEntry<bool> sailForwardForceConfig;
        public static ConfigEntry<bool> sailSidewaysForceConfig;
        //Various
        public static ConfigEntry<bool> flipWinchesOutConfig;
        public static ConfigEntry<bool> sailNameConfig;
        public static ConfigEntry<bool> uniqueSailNameConfig;   //Gives sail unique names based on the mast they are on and their height on it.
        public static ConfigEntry<bool> simpleUniqueNamesConfig;      //makes the unique name use simple prefixes 
        public static ConfigEntry<bool> coloredWinchesConfig;   //Colors the all the winches attached to a sail in one color.
        //HUD display (all information displayed on the rudder when looking at it)
        //rudder
        public static ConfigEntry<bool> rudderBarConfig;
        //Wind Absolute
        public static ConfigEntry<bool> windSpeedConfig;    //enables the wind speed display
        public static ConfigEntry<bool> windSpeedBeaufortConfig;    //enables the beaufort scale to represent wind speed.
        public static ConfigEntry<bool> windDirectionConfig;    //enables absolute wind direction display
        public static ConfigEntry<bool> windDirectionNESWConfig; //makes the wind display show approximate cardinal directions (N, NW, SSW, etc.)
        //Point of Sail
        public static ConfigEntry<bool> windRelativeConfig; //enable the display of the wind direction relative to the boat (0° to 180° and 0° to -180°)
        public static ConfigEntry<bool> windRelativeColorConfig; //makes the relative wind direction display in colore text (red -> Left, green -> Right)
        //Boat Heading
        public static ConfigEntry<bool> boatHeadingConfig; //enables boat heading display
        public static ConfigEntry<bool> approximateBoatHeading; //makes the heading display show approximate cardinal directions (N, NW, SSW, etc.)
        //Boat Speed
        public static ConfigEntry <bool> boatSpeedConfig;   //enables the boat speed display
        public static ConfigEntry<bool> boatVMGConfig;      //enables VMG display
        public static ConfigEntry<bool> nauticalMilePerHourConfig; //displays speed in nautical mile per hour (they are kts / 1.555)
        //boat heeling
        public static ConfigEntry<bool> boatHeelingConfig;  //displays boat heeling in ° (Category: Rudder HUD: other)
        //coordinates
        public static ConfigEntry<bool> showCoordinatesConfig; //displays coordinates on the rudder
        //OTHER ITEMS
        //clock
        public static ConfigEntry<bool> showGlobalTimeConfig;    //displays the global time hh:mm when looking at the clock
        public static ConfigEntry<bool> showLocalTimeConfig;    //displays the local time hh:mm when looking at the clock
        public void Awake()
        {
            #region Configuration Setup
            //MAIN SWITCHES
            winchesInfoConfig = Config.Bind("A) Main Switches", "winchesInfo", true, "Enables or disables the winches information entirely. Requires restarting the game.");
            rudderHUDConfig = Config.Bind("A) Main Switches", "rudderHUD", true, "Enables or disables the rudder HUD entirely. Requires restarting the game.");
            //WINCHES
            //Sail Trimming (Efficiency)
            winchesOutConfig = Config.Bind("B) Winches Display: 1. Sail Trimming and Efficiency", "winchesOutText", true, "Shows 'X out' when looking at winches. 0 means the winch is fully tightened in, 100 means it's fully released. Set to false to disable.");
            winchesOutDegreeConfig = Config.Bind("B) Winches Display: 1. Sail Trimming and Efficiency", "winchesOutDegree", true, "Replaces the 'X out' text with the degrees the sail is out, e.g. '45° out'. THIS REQUIRES THE (winchesOutText) OPTION TO BE SET TO TRUE! Set to false to disable.");
            winchesBarConfig = Config.Bind("B) Winches Display: 1. Sail Trimming and Efficiency", "winchesBar", true, "Shows a loading bar type of thing when looking at winches. Empty bar means the winch is fully tightened in, 100 means it's fully released. Set to false to disable.");
            sailEfficiencyConfig = Config.Bind("B) Winches Display: 1. Sail Trimming and Efficiency", "sailEfficiency", true, "Shows the efficiency of the sail attached to the winch you are looking at. Does not apply to halyard winches. Negative means sail is pushing backwards. Maximize to find the best trim. Set to false to disable.");
            sailForwardForceConfig = Config.Bind("B) Winches Display: 1. Sail Trimming and Efficiency", "sailForwardForce", false, "Shows how close the FORWARD force generated by the sail is to the maximum. 0% means no force forward, 100% means maximum force forward. Maximize for best result. This was called Sail Efficiency up to v1.1.8. Set to true to enable.");
            sailSidewaysForceConfig = Config.Bind("B) Winches Display: 1. Sail Trimming and Efficiency", "sailSidewayForce", false, "Shows how close the SIDEWAY force generated by the sail is to the maximum. 0% means no force sideway, 100% means maximum force sideway. Minimize for best result. Set to true to enable.");
            flipWinchesOutConfig = Config.Bind("B) Winches Display: 1. Sail Trimming and Efficiency", "flipWinchesOut", true, "Flips the 'X out' when looking at halyard winches of sail that folds by releasing the rope, like junk sails and some gaffs. When true the sail will be at 100 out when fully opened and at 0 when close, making it more consistent with other type of sail.");
            //Sail Names
            sailNameConfig = Config.Bind("B) Winches Display: 2. Sail Names", "sailName", false, "Shows the name of the sail attached to the winch currently looked at. Set to true to enable.");
            uniqueSailNameConfig = Config.Bind("B) Winches Display: 2. Sail Names", "uniqueSailName", true, "Adds a colored dot next to the sail name. The color of the dot is different for sail with the same name. This helps differentiating winches that control the same type of sail (e.g. if you have two squares of the same size on one mast and don't know wich halyard controls what). THIS REQUIRES THE (sailName) OPTION TO BE SET TO TRUE! Set to true to enable.");
            simpleUniqueNamesConfig = Config.Bind("B) Winches Display: 2. Sail Names", "simpleUniqueSailName", false, "Uses simple prefixes for sail names (Bottom, Middle, Top, Highest) instead of historical ones. THIS REQUIRES THE (sailName) OPTION TO BE SET TO TRUE! Set to true to enable.");
            coloredWinchesConfig = Config.Bind("B) Winches Display: 2. Sail Names", "coloredWinches", false, "Colors all the winches attached to a single sail of the same color, making it easier to identify. Change to true to enable.");

            //HUD (all information displayed on the rudder when looking at it)
            //rudder
            rudderBarConfig = Config.Bind("C) Rudder HUD: 5. Other", "rudderBar", true, "Shows a loading bar type of thing indicating how much the rudder is turned either way.  Set to false to disable.");
            //Apparent Wind
            windSpeedConfig = Config.Bind("C) Rudder HUD: 1. Apparent Wind", "windSpeed", true, "Shows the APPARENT wind speed.  Set to false to disable.");
            windSpeedBeaufortConfig = Config.Bind("C) Rudder HUD: 1. Apparent Wind", "windSpeedBeaufort", true, "Replaces wind speed with Beaufort scale descriptor words (e.g. Calm, Light Breeze, Gale, etc.). THIS REQUIRES THE (windSpeed) OPTION TO BE SET TO TRUE! Set to false to disable.");
            windDirectionConfig = Config.Bind("C) Rudder HUD: 1. Apparent Wind", "windDirection", true, "Shows APPARENT wind direction in degree, relative to the world (e.g. 45° means the APPARENT wind comes from the North-West).  Set to false to disable.");
            windDirectionNESWConfig = Config.Bind("C) Rudder HUD: 1. Apparent Wind", "windDirectionNESW", true, "Shows the approximate cardinal direction of the APPARENT wind (e.g. wind coming from 45° will be shown as NW instead of it's exact numerical value. Set to false to disable. THIS REQUIRES THE (windDirection) TO BE SET TO TRUE!");
            //Point of Sail
            windRelativeConfig = Config.Bind("C) Rudder HUD: 2. Point of Sail", "windAngleToBoat", true, "Shows the angle (0°-180°) between the APPARENT wind and the boat forward direction. Negative angles mean the wind comes from the left of the boat. Set to false to disable.");
            windRelativeColorConfig = Config.Bind("C) Rudder HUD: 2. Point of Sail", "windAngleToBoatColor", true, "Wind direction relative to boat displayed in green it wind comes from the right, red if wind comes from the left. THIS REQUIRES THE (windAngleToBoat) TO BE SET TO TRUE! Set to false to disable.");
            //Heading
            boatHeadingConfig = Config.Bind("C) Rudder HUD: 3. Heading", "boatHeading", false, "Shows the boat heading in degrees. Set to false to disable.");
            approximateBoatHeading = Config.Bind("C) Rudder HUD: 3. Heading", "approximateBoatHeading", true, "Shows the approximate boat heading in cardinal directions (e.g. N, NW, SSE, etc.). THIS REQUIRES THE (boatHeading) TO BE SET TO TRUE! Set to false to disable.");
            //Boat Speed
            boatSpeedConfig = Config.Bind("C) Rudder HUD: 4. Boat Speed", "boatSpeed", false, "Shows current boat speed in kts. Set to false to disable.");
            nauticalMilePerHourConfig = Config.Bind("C) Rudder HUD: 4. Boat Speed", "nauticalMilePerHour", false, "Displays boat speed in nautical miles per hour instead of kts. In the game nmi/h = kts / 1.555. THIS REQUIRES THE (boatSpeed) TO BE SET TO TRUE! Change to true to enable.");
            boatVMGConfig = Config.Bind("C) Rudder HUD: 4. Boat Speed", "boatVMG", false, "Shows the current boat VMG in kts. Set to false to disable.");
            //Boat heeling
            boatHeelingConfig = Config.Bind("C) Rudder HUD: 5. Other", "boatHeeling", false, "Shows current boat heeling in degrees.");
            //Coordinates
            showCoordinatesConfig = Config.Bind("C) Rudder HUD: 5. Other", "showCoordinates", false, "Adds latitude and longitude to the rudder HUD. Set to true to enable.");
            //OTHER ITEMS
            //Clock
            showGlobalTimeConfig = Config.Bind("D) Other Items: 1. Clock", "showGlobalTime", false, "Shows exact global time in the hh:mm format when looking at a clock. Set to true to enable.");
            showLocalTimeConfig = Config.Bind("D) Other Items: 1. Clock", "showLocalTime", false, "Shows exact local time in the hh:mm format when looking at a clock. Set to true to enable.");
            #endregion

            #region Patching Information
            //PATCHES INFO
            Harmony harmony = new Harmony(pluginGuid);
            MethodInfo original = AccessTools.Method(typeof(GPButtonRopeWinch), "Update");
            MethodInfo patch = AccessTools.Method(typeof(SailInfoPatches), "Update_Patch");
            MethodInfo original2 = AccessTools.Method(typeof(GPButtonSteeringWheel), "ExtraLateUpdate");
            MethodInfo patch2 = AccessTools.Method(typeof(SailInfoPatches), "ExtraLateUpdate_Patch");
            MethodInfo original3 = AccessTools.Method(typeof(SailConnections), "Awake");
            MethodInfo patch3 = AccessTools.Method(typeof(SailInfoPatches), "Awake_Patch");
            MethodInfo original4 = AccessTools.Method(typeof(Mast), "UpdateControllerAttachments");
            MethodInfo patch4 = AccessTools.Method(typeof(SailInfoPatches), "UpdateControllerAttachments_Patch");
            //clock
            MethodInfo original5 = AccessTools.Method(typeof(ShipItemClock), "ExtraLateUpdate");
            MethodInfo patch5 = AccessTools.Method(typeof(SailInfoPatches), "ClockPatch");
            //quadrant
            //MethodInfo original6 = AccessTools.Method(typeof(ShipItemQuadrant), "ExtraLateUpdate");
            //MethodInfo patch6 = AccessTools.Method(typeof(SailInfoPatches), "QuadrantPatch");
            //MethodInfo original7 = AccessTools.Method(typeof(ShipItemQuadrant), "OnPickup");
            //MethodInfo patch7 = AccessTools.Method(typeof(SailInfoPatches), "QuadrantOnPickupPatch");
            //MethodInfo original8 = AccessTools.Method(typeof(ShipItemQuadrant), "OnDrop");
            //MethodInfo patch8 = AccessTools.Method(typeof(SailInfoPatches), "QuadrantOnDropPatch");
            //MethodInfo original9 = AccessTools.Method(typeof(ShipItemQuadrant), "OnLeaveInventory");
            //MethodInfo patch9 = AccessTools.Method(typeof(SailInfoPatches), "QuadrantLeaveInventoryPatch");

            //PATCH APPLICATION
            if (winchesInfoConfig.Value)
            {
                harmony.Patch(original, new HarmonyMethod(patch));   //winches info
                harmony.Patch(original3, new HarmonyMethod(patch3)); //winches info (for the sail efficiency stuff)
                if (coloredWinchesConfig.Value)
                {
                    harmony.Patch(original4, new HarmonyMethod(patch4)); //patch to color the winches
                }
            }
            if (rudderHUDConfig.Value)
            {
                harmony.Patch(original2, new HarmonyMethod(patch2)); //rudder HUD
            }
            //CLOCK PATCH
            harmony.Patch(original5, new HarmonyMethod(patch5));
            //QUADRANT PATCHES
            //NOTE: All quadrant patches are in the code, just left unpatched. Might become useful in the future
            //harmony.Patch(original6, new HarmonyMethod(patch6));
            //harmony.Patch(original7, new HarmonyMethod(patch7));
            //harmony.Patch(original8, new HarmonyMethod(patch8));
            //harmony.Patch(original9, new HarmonyMethod(patch9));
            #endregion
        }
    }
    public class SailInfoPatches   //contains the patch
    {
        //VARIABLES
        public static Dictionary<Sail, List<RopeController>> sailRopeControllerMap = new Dictionary<Sail, List<RopeController>>();
        public static int winchColorIndex = 0;
        public static Color[] colorArray = { Color.black, Color.blue, Color.cyan, Color.gray, Color.green, Color.magenta, Color.red, Color.white, Color.yellow };
        //quadrant
        private static bool quadrantHeld;

        //WINCHES METHODS
        [HarmonyPrefix] //patch happens before original
        public static void Update_Patch(GPButtonRopeWinch __instance, GoPointer ___stickyClickedBy, bool ___isLookedAt)
        {
            if (___isLookedAt || ___stickyClickedBy)
            {
                __instance.description = RopeBar(__instance.rope);
            }
        }
        public static string RopeBar(RopeController rope)
        {   //possibly useful ASCII characters: ████░░░░░░ ████▒▒▒▒ ■□□□□ ▄▄▄... ▀▀    ═══───
            //this adds the bar, but it's also what calls most of the other stuff
            float amount = rope.currentLength;
            if (SailInfoMain.flipWinchesOutConfig.Value && SailInfoMain.winchesOutConfig.Value && rope.name.Contains("reef"))
            {   //flip the X out text for reverseReefing sail
                RopeControllerSailReef castedRope = (RopeControllerSailReef)rope;
                if (castedRope.reverseReefing)
                {   //flips the out thingy
                    amount = 1f - amount;
                }
            }
            int barLength = 300; // Total length of the loading bar
            int filledLength = (int)(amount * barLength); // Calculate the number of filled characters
            string description = "";

            StringBuilder bar = new StringBuilder();
            for (int i = 0; i < barLength; i++)
            {
                if (i < filledLength)
                {
                    bar.Append("█"); // Filled character
                }
                else
                {
                    bar.Append("░"); // Empty character
                }
            }
            float sailOut = Mathf.Round(amount * 100);
            //SAIL EFFICIENCY / NAMING THINGS
            if (rope.name.Contains("reef"))
            {   //do this if it's an halyard winch
                if (SailInfoMain.flipWinchesOutConfig.Value || SailInfoMain.sailNameConfig.Value)
                {   //The two cases where we actually need to know what the sail attached to the controller is
                    foreach (var sail in sailRopeControllerMap)
                    {
                        foreach (var ropeController in sail.Value)
                        {
                            if (rope == ropeController)
                            {
                                Sail currentSail = sail.Key;
                                if (SailInfoMain.sailNameConfig.Value)
                                {
                                    if (SailInfoMain.uniqueSailNameConfig.Value)
                                    {
                                        description += $"<size=70%>{SailName(currentSail)} Halyard\n</size>";
                                    }
                                    else
                                    {
                                        description += $"<size=70%>{currentSail.sailName} Halyard\n</size>";
                                    }
                                }
                                break;  //this should exit the foreach once the ropeController is found, making it more efficient.
                            }
                        }
                    }
                }
                if (SailInfoMain.winchesOutConfig.Value)
                {
                    description += $"<size=70%>{sailOut} out</size>";
                }
                if (SailInfoMain.winchesBarConfig.Value)
                {
                    description += $"\n<size=3%>{bar}</size>";
                }
            }
            else
            {   //do this if it's a sheet winch
                if (SailInfoMain.sailEfficiencyConfig.Value || SailInfoMain.sailForwardForceConfig.Value || SailInfoMain.sailSidewaysForceConfig.Value || SailInfoMain.sailNameConfig.Value)
                {   //Identifies what sail the controller being looked at is attached to!
                    foreach (var sail in sailRopeControllerMap)
                    {
                        foreach (var ropeController in sail.Value)
                        {
                            if (rope == ropeController)
                            {   //This is the sail the looked at controller controls
                                Sail currentSail = sail.Key;
                                if (SailInfoMain.sailNameConfig.Value)
                                {
                                    if (SailInfoMain.uniqueSailNameConfig.Value)
                                    {
                                        description += $"<size=70%>{SailName(currentSail)}\n</size>";
                                    }
                                    else
                                    {
                                        description += $"<size=70%>{currentSail.sailName}\n</size>";
                                    }
                                }
                                if (SailInfoMain.sailEfficiencyConfig.Value)
                                {
                                    description += $"<size=70%>Eff: {CombinedEfficiency(currentSail)}% </size>";
                                }
                                if (SailInfoMain.sailForwardForceConfig.Value)
                                {
                                    description += $"<size=70%>F: {SailEfficiency(currentSail)}% </size>";
                                }
                                if (SailInfoMain.sailSidewaysForceConfig.Value)
                                {
                                    description += $"<size=70%>S: {SailInefficiency(currentSail)}% </size>";
                                }
                                if (SailInfoMain.winchesOutConfig.Value)
                                {   //X out
                                    if (SailInfoMain.winchesOutDegreeConfig.Value)
                                    {   //X° out (0° - maxAngle°)
                                        description += $"<size=70%>\n{SailDegree(currentSail)}° out</size>";
                                    }
                                    else
                                    {   //X out (0-100)
                                        description += $"<size=70%>\n{sailOut} out</size>";
                                    }
                                }
                            }
                        }
                    }
                }
                if (SailInfoMain.winchesBarConfig.Value)
                {
                    description += $"\n<size=3%>{bar}</size>";
                }
            }
            return description;
        }
        private static float SailEfficiency(Sail currentSail)
        {   // Calculates the efficiency of a sail trim (max is best)

            //This is the force created by the sail
            FieldInfo unamplifiedForceInfo = AccessTools.Field(typeof(Sail), "unamplifiedForwardForce");
            float unamplifiedForce = (float)unamplifiedForceInfo.GetValue(currentSail);
            //This is the total force the wind applies to the sail. This is also the maximum force forward the sail can generate on the boat.
            FieldInfo totalWindForceInfo = AccessTools.Field(typeof(Sail), "totalWindForce");
            float totalWindForce = (float)totalWindForceInfo.GetValue(currentSail);

            float efficiency = Mathf.Round(unamplifiedForce / totalWindForce * 100f);

            return efficiency;
        }
        private static float SailInefficiency(Sail currentSail)
        {   // Calculates the percentage of sideway force on a sail (min is best)
            FieldInfo unamplifiedForceInfo = AccessTools.Field(typeof(Sail), "unamplifiedSidewayForce");
            float unamplifiedForce = (float)unamplifiedForceInfo.GetValue(currentSail);

            //This is the total force the wind applies to the sail. This is also the maximum force forward the sail can generate on the boat.
            FieldInfo totalWindForceInfo = AccessTools.Field(typeof(Sail), "totalWindForce");
            float totalWindForce = (float)totalWindForceInfo.GetValue(currentSail);

            float inefficiency = Mathf.Abs(Mathf.Round(unamplifiedForce / totalWindForce * 100f));

            return inefficiency;
        }
        private static float CombinedEfficiency(Sail currentSail)
        {   //combines Efficiency and Inefficiency into one single value (max is best)
            //this is the real efficiency!
            float eff = SailEfficiency(currentSail);
            if (eff <= 0f)
            {
                return eff;
            }
            float ineff = 100 - SailInefficiency(currentSail);
            float comb = Mathf.Round((eff + ineff) / 2f);

            return comb;
        }
        private static string SailName(Sail currentSail)
        {
            FieldInfo currentInstallHeightInfo = AccessTools.Field(typeof(Sail), "currentInstallHeight");   //get ready to access currentInstallHeight
            string[] sailPrefixes = { "Main", "Topsail", "T'gallant", "Royal" };
            if (SailInfoMain.simpleUniqueNamesConfig.Value)
            {
                sailPrefixes[0] = "Bottom Square";
                sailPrefixes[1] = "Middle Square";
                sailPrefixes[2] = "Top Square";
                sailPrefixes[3] = "Highest Square";
            }

            Mast currentMast = currentSail.transform.parent.GetComponent<Mast>();   //get the mast the sail is attached to
            Sail[] sails = currentMast.GetComponentsInChildren<Sail>(); //get all the sails attached to said mast

            sails = sails.OrderBy(sail => (float)currentInstallHeightInfo.GetValue(sail)).ToArray();    //sort the sails array based on currentInstallHeight

            int j = 0; //index to access the sailPrefixes
            string uniqueName = "";
            for (int i = 0; i < sails.Length; i++)
            {
                float currentInstallHeight = (float)currentInstallHeightInfo.GetValue(sails[i]);
                if (sails[i].name.Contains("square"))
                {
                    uniqueName = MastName(currentMast) + sailPrefixes[j];
                    j++;
                }
                else if (sails[i].name.Contains("gaff"))
                {
                    uniqueName = MastName(currentMast) + "Gaff";
                }
                else if (sails[i].name.Contains("lateen"))
                {
                    uniqueName = MastName(currentMast) + "Lateen";
                }
                else if (sails[i].name.Contains("junk"))
                {
                    uniqueName = MastName(currentMast) + "Junk";
                }
                else if (sails[i].name.Contains("junklateen"))
                {
                    uniqueName = MastName(currentMast) + "Fin";
                }
                else if (sails[i].name.Contains("jib"))
                {
                    uniqueName = MastName(currentMast) + "Jib";
                }
                else if (sails[i].name.Contains("genoa"))
                {
                    uniqueName = MastName(currentMast) + "Genoa";
                }
                if (currentSail.name == sails[i].name)
                {
                    return uniqueName;
                }
                else
                {
                    //Debug.LogWarning($"SailInfo: Sail[{i}]: {uniqueName}, height: {currentInstallHeight}");
                }
            }
            return "";
        }
        private static string MastName(Mast currentMast)
        {   //returns a human-readable mast name from the Mast gameObject name

            Mast[] mastList = GameState.currentBoat.GetComponentsInChildren<Mast>();
            int mastCount = mastList.Length;
            for (int i = 0; i < mastCount; i++)
            {   //Checks if one of the mast is a bowsprit or a forestay and removes them from the mast count. We only care about foremast, mainmast and mizzen
                if (mastList[i].name.Contains("bowsprit") || mastList[i].name.Contains("stay"))
                {
                    mastCount--;
                }
            }
            if (mastCount > 1)
            {   //Checks if there is more than 1 mast, if there isn't return ""
                if (currentMast.name.Contains("bowsprit"))
                {   //Checks if this masts is a bowsprit
                    return "Bowsprit ";
                }
                else if (currentMast.name == "mast_front_" || currentMast.name == "mast_Front_0" || currentMast.name == "mast_Front_1")
                {   //Checks if this mast is the foremast
                    return "Foremast ";
                }
                else if (currentMast.name == "mast" || currentMast.name == "mast_1" || currentMast.name == "mast_0_extension" || currentMast.name == "mast_1_extension" || currentMast.name == "mast_center" || currentMast.name == "mast_mid_0" || currentMast.name == "mast_mid_1" || currentMast.name == "mast_front" || currentMast.name == "mast_Back_1" || currentMast.name == "mast_Back_2")
                {   //Checks if this mast is the mainmast
                    return "Mainmast ";
                }
                else if (currentMast.name == "mast_002" || currentMast.name == "mast_001" || currentMast.name == "mast_mizzen_0" || currentMast.name == "mast_mizzen_1" || currentMast.name == "mast_mizzen")
                {   //Checks if this mast is the mizzen mast
                    return "Mizzen ";
                }
            }
            return "";
        }
        private static void WinchColor(GPButtonRopeWinch winch, int winchColorIndex)
        {   //Colors each winch associated to a specific sail in a unique way.
            winchColorIndex %= colorArray.Length;
            winch.transform.GetComponent<MeshRenderer>().material.color = colorArray[winchColorIndex];
        }
        private static int SailDegree(Sail sail)
        {   //gets the sail in and returns the angle wiht the boat forward direction out out                      
            Vector3 boatVector = -GameState.currentBoat.transform.right;    //boat direction
            Vector3 sailVector = sail.squareSail ? sail.transform.up : -sail.transform.right; //sail "direction" since squares are made differently we use the up direction for them, otherwise the -right direction (also known as left)

            int angle = Mathf.RoundToInt(Vector3.SignedAngle(boatVector, sailVector, -Vector3.up)); //calculate the angle

            angle = angle > 90 ? 180 - angle : angle; //keep it in a 0° to 90° angle
            angle = angle < 0 ? -angle : angle; //keep it positive
            return angle;
        }

        //RUDDER HUD METHODS
        [HarmonyPrefix]
        public static void ExtraLateUpdate_Patch(ref string ___description, GoPointer ___stickyClickedBy, bool ___isLookedAt, Rudder ___rudder, HingeJoint ___attachedRudder)
        {
            if (___isLookedAt || ___stickyClickedBy)
            {
                ___description = "";
                if (SailInfoMain.windSpeedConfig.Value || SailInfoMain.windDirectionConfig.Value || SailInfoMain.windRelativeConfig.Value)
                {
                    ___description = "Wind: ";
                }
                if (SailInfoMain.windSpeedConfig.Value)
                {
                    if (SailInfoMain.windSpeedBeaufortConfig.Value)
                    {
                        ___description += $"{Beaufort()} ";
                    }
                    else
                    {
                        ___description += $"{Mathf.Round(WindForce())} kts ";
                    }
                }
                if (SailInfoMain.windDirectionConfig.Value)
                {
                    if (SailInfoMain.windDirectionNESWConfig.Value)
                    {
                        ___description += $"{DirectionNESW(Mathf.Round(WindDirection()))} ";
                    }
                    else
                    {
                        ___description += $"{Mathf.Round(WindDirection())}° ";
                    }
                }
                if (SailInfoMain.windRelativeConfig.Value)
                {
                    if (SailInfoMain.windRelativeColorConfig.Value)
                    {
                        float angleToBoat = AngleToBoat();
                        if (angleToBoat > 0f)
                        {   //positive angle means right, so green color.
                            ___description += $"<color=#113905>{Mathf.Round(angleToBoat)}°</color>";
                        }
                        else
                        {
                            ___description += $"<color=#7C0000>{Mathf.Round(angleToBoat)}°</color>";
                        }

                    }
                    else
                    {
                        ___description += $"{Mathf.Round(AngleToBoat())}°";
                    }
                }
                if (SailInfoMain.boatHeadingConfig.Value || SailInfoMain.boatSpeedConfig.Value || SailInfoMain.boatVMGConfig.Value)
                {
                    ___description += "\n";
                }
                if (SailInfoMain.boatSpeedConfig.Value)
                {
                    if (SailInfoMain.nauticalMilePerHourConfig.Value)
                    {
                        ___description += $"SPD: {Mathf.Round(BoatSpeed() / 1.555f)} nmi/h ";
                    }
                    else
                    {
                        ___description += $"SPD: {Mathf.Round(BoatSpeed())} kts ";
                    }
                }
                if (SailInfoMain.boatHeadingConfig.Value)
                {
                    if (SailInfoMain.approximateBoatHeading.Value)
                    {
                        ___description += $"HDG: {DirectionNESW(Mathf.Round(BoatHeading()))} ";
                    }
                    else
                    {
                        ___description += $"HDG: {Mathf.Round(BoatHeading())}° ";
                    }
                }
                if (SailInfoMain.boatVMGConfig.Value)
                {
                    ___description += $"VMG: {Mathf.Round(VMG())} kts";
                }
                if (SailInfoMain.boatHeelingConfig.Value)
                {
                    ___description += $"\nHeeling {Mathf.Round(Heeling())}°";
                }
                if (SailInfoMain.showCoordinatesConfig.Value)
                {
                    ___description += $"\n{Latitude()}, {Longitude()}";
                }
                if (SailInfoMain.rudderBarConfig.Value)
                {
                    ___description += $"\n{RudderBar(___rudder.currentAngle, ___attachedRudder.limits.max)}";
                }
            }
        }
        public static string RudderBar(float currentAngle, float angleLimit)
        {
            int barLength = 150;
            bool turnedRight = currentAngle <= 0;
            string description;
            string emptySide = "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░";
            int filledLength = (int)(Math.Abs(currentAngle) * barLength / angleLimit);
            int emptyLength = barLength - filledLength;

            StringBuilder barRight = new StringBuilder();
            for (int i = 0; i < barLength; i++)
            {
                if (i < filledLength)
                {
                    barRight.Append("█"); // Filled character
                }
                else
                {
                    barRight.Append("░"); // Empty character
                }
            }
            StringBuilder barLeft = new StringBuilder();
            for (int i = 0; i < barLength; i++)
            {
                if (i < emptyLength)
                {
                    barLeft.Append("░"); // Filled character
                }
                else
                {
                    barLeft.Append("█"); // Empty character
                }
            }
            if (turnedRight)
            {
                description = $"<size=3%>{emptySide}█{barRight}</size>";
            }
            else
            {
                description = $"<size=3%>{barLeft}█{emptySide}</size>";
            }
            return description;
        }
        private static float WindForce()
        {
            if (GameState.currentBoat.GetComponentInChildren<Sail>() != null)
            {
                return GameState.currentBoat.GetComponentInChildren<Sail>().apparentWind.magnitude;
            }
            else
            {
                return 0f;
            }
        }
        private static float WindDirection()
        {   //calculate apparent wind direction in an absolute frame of reference
            Transform currentShip = GameState.currentBoat;
            Sail anySail;
            if (currentShip.GetComponentInChildren<Sail>() != null)
            {
                anySail = currentShip.GetComponentInChildren<Sail>();
            }
            else
            {
                return 0f;
            }
            float windDirection = Vector3.SignedAngle(anySail.apparentWind, -Vector3.forward, -Vector3.up);
            windDirection = windDirection < 0 ? windDirection + 360f : windDirection;

            return windDirection;
        }
        private static float BoatHeading()
        {   //calculates boat heading in an absolute frame of reference
            Transform currentShip = GameState.currentBoat;
            Vector3 boatDirection = -currentShip.transform.right;

            float boatHeading = Vector3.SignedAngle(boatDirection, -Vector3.forward, -Vector3.up);
            boatHeading = boatHeading < 0 ? boatHeading + 360f : boatHeading;

            return boatHeading;
        }
        private static float AngleToBoat()
        {   //calculates angle between boat and apparent wind, 0 to 180°. Positive on the right, negative on the left
            Transform currentShip = GameState.currentBoat;
            Sail anySail;
            if (currentShip.GetComponentInChildren<Sail>() != null)
            {
                anySail = currentShip.GetComponentInChildren<Sail>();
            }
            else
            {
                return 0f;
            }
            Vector3 boatDirection = -currentShip.transform.right;

            return Vector3.SignedAngle(boatDirection, anySail.apparentWind, Vector3.up);
        }
        private static float BoatSpeed()
        {   //calculates boat speed in kts
            Transform currentShip = GameState.currentBoat;
            float boatSpeed = currentShip.parent.GetComponent<Rigidbody>().velocity.magnitude;

            return boatSpeed * 1.94384f;
        }
        private static string DirectionNESW(float direction)
        {   //takes in an angle 0-360 and returns a string representing it's cardinal direction (eg. N, NNW, NW, etc.)

            string[] sectors = { "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW", "N" };

            string cardinal = sectors[(int)Math.Floor(((direction + 11.25) % 360) / 22.5)];
            return cardinal;
        }
        private static float VMG()
        {   //calculates the VMG of the boat

            Vector3 velocity = GameState.currentBoat.parent.GetComponent<Rigidbody>().velocity;
            Vector3 trueWind = Wind.currentWind;
            float angle = Vector3.Angle(velocity, trueWind) * Mathf.Deg2Rad;
            float boatSpeed = velocity.magnitude * 1.94384f;

            return -boatSpeed * Mathf.Cos(angle);
        }
        //private static string PointOfSail()
        //{   //returns the point of sail in 32-esimes (eg, 1/32 is headwind, I think)
        //      using things like broad reach, close hauled and so on might be an option. Not sure.
        //    return "";
        //}
        private static string Beaufort()
        {   //returns a descriptor text indicating how strong the wind is.

            float windSpeed;
            if (GameState.currentBoat.GetComponentInChildren<Sail>() != null)
            {
                windSpeed = GameState.currentBoat.GetComponentInChildren<Sail>().apparentWind.magnitude;
            }
            else
            {
                return "Add at least one sail. ";
            }

            string descriptor =
                windSpeed < 1f ? "Calm(0) " :
                windSpeed >= 1f && windSpeed < 3f ? "Light Air(1) " :
                windSpeed >= 3f && windSpeed < 6f ? "Light Breeze(2) " :
                windSpeed >= 6f && windSpeed < 10f ? "Gentle Breeze(3) " :
                windSpeed >= 10f && windSpeed < 16f ? "Moderate Breeze(4) " :
                windSpeed >= 16f && windSpeed < 21f ? "Fresh Breeze(5) " :
                windSpeed >= 21f && windSpeed < 27f ? "Strong Breeze(6) " :
                windSpeed >= 27f && windSpeed < 33f ? "High Wind(7) " :
                windSpeed >= 33f && windSpeed < 40f ? "Gale(8) " :
                windSpeed >= 40f && windSpeed < 47f ? "Strong Gale(9) " :
                windSpeed >= 47f && windSpeed < 55f ? "Storm(10) " :
                windSpeed >= 55f && windSpeed < 63f ? "Violent Storm(11) " :
                windSpeed >= 63f ? "Hurricane-force(12) " :
                "Unknown";
            descriptor = $"<size=60%>{descriptor}</size>";
            return descriptor;

        }
        private static float Heeling()
        {   //returns boat heeling in degrees
            Transform currentShip = GameState.currentBoat;
            Vector3 boatUp = currentShip.transform.up;

            float boatHeading = Vector3.SignedAngle(boatUp, Vector3.up, -Vector3.forward);

            return boatHeading;
        }

        // CLOCK, QUADRANT, SUNCOMPASS, CHRONOCOMPASS
        [HarmonyPrefix]
        public static void ClockPatch(ShipItemClock __instance, bool ___isLookedAt)
        {   //shows hours and minutes when looking at the clock 
            if (___isLookedAt)
            {
                __instance.description = "";
                if (!SailInfoMain.showGlobalTimeConfig.Value && !SailInfoMain.showLocalTimeConfig.Value)
                {   //do nothing if config says so
                    return;
                }
                if (SailInfoMain.showGlobalTimeConfig.Value)
                {   //global time
                    
                    float time = Sun.sun.globalTime;
                    __instance.description += $"\n\n{GetTime(time)}";
                }
                if (SailInfoMain.showLocalTimeConfig.Value)
                {   //local time
                    float time = Sun.sun.localTime;
                    __instance.description += $"\n{GetTime(time)}";
                }
            }
        }
        private static string GetTime(float time)
        {   //gets a float in and give hh:mm out
            int hours = (int)time;
            int min = (int)((time - hours) * 60);
            if (min < 10)
            {
                return $"{hours}:0{min}";
            }
            else
            {
                return $"{hours}:{min}";
            }
        }
        [HarmonyPrefix]
        public static void QuadrantPatch()
        {   //shows either the latitude or the reading when inspecting the quadrant
            //DEBUG: ISSUE: Using recoveryText like this breaks all other instances of recovery text...
            //SOLUTION: Maybe I don't need this to be in the Update patch! If I only show the lat...
            //          If I wanna show the reading though, the UpdatePatch would be required I guess...
            //          Maybe I can create a copy of recoveryText?
            //RELEASE: all the Quadrant Patches are left there but not implemented. Might become useful in the future.
            if (quadrantHeld)
            {
                Sleep.instance.recoveryText.text = $"<size=20%>\n\n\nLat: {Latitude()}</size>";
            }
            else
            {
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

        private static string Latitude()
        {   //get latitude
            Transform pos = Refs.charController.transform;
            float lat = FloatingOriginManager.instance.GetGlobeCoords(pos).z;
            int deg = (int)lat;
            int min = (int)((lat - deg) * 60);
            if( deg >= 0)
            {
                return $"{deg}° {min}'N";
            }
            else
            {
                return $"{deg}° {min}'S";
            }
        }
        private static string Longitude()
        {   //get longitude
            Transform pos = Refs.charController.transform;
            float lon = FloatingOriginManager.instance.GetGlobeCoords(pos).x;
            int deg = (int)lon;
            int min = (int)((lon - deg) * 60);
            if (deg >= 0)
            {
                return $"{deg}° {min}'E";
            }
            else
            {
                return $"{deg}° {min}'W";
            }
        }

        //SAIL → WINCHES MAP
        [HarmonyPostfix]
        public static void Awake_Patch(Sail ___sail, RopeController ___reefController, RopeController ___angleControllerMid, RopeController ___angleControllerLeft, RopeController ___angleControllerRight)
        {
            //This creates a map that associates each rope controller to a sail,
            //and saves it into a dictionary. Sail name is the key.

            // Check if any rope controller is not null
            if (___angleControllerMid != null || (___angleControllerLeft != null && ___angleControllerRight != null))
            {
                List<RopeController> ropeControllers = new List<RopeController>();

                // Add existing rope controllers to the list
                if (___angleControllerMid != null)
                {
                    ropeControllers.Add(___angleControllerMid);
                }
                if (___angleControllerLeft != null && ___angleControllerRight != null)
                {
                    ropeControllers.Add(___angleControllerLeft);
                    ropeControllers.Add(___angleControllerRight);
                }
                ropeControllers.Add(___reefController);

                // Add the list of rope controllers to the dictionary
                sailRopeControllerMap[___sail] = ropeControllers;
            }
            else
            {
                //Debug.LogError("SailInfo: all rope controllers are null!");
            }
        }
        //WINCH RECOLORING
        [HarmonyPostfix]
        public static void UpdateControllerAttachments_Patch(Mast __instance)
        {
            if (GameState.currentlyLoading)
            {
                //this goes through all the sails attached to a mast and assigns
                //unique color to each group of winches associated with a single sail.
                GPButtonRopeWinch[] left = __instance.leftAngleWinch;
                GPButtonRopeWinch[] right = __instance.rightAngleWinch;
                GPButtonRopeWinch[] middle = __instance.midAngleWinch;
                GPButtonRopeWinch[] reef = __instance.reefWinch;

                foreach (GameObject sail in __instance.sails)
                {
                    Sail component = sail.GetComponent<Sail>();
                    SailConnections component2 = sail.GetComponent<SailConnections>();
                    bool topsail = false;
                    if (component.squareSail)   //this checks if the sail in question is a square above other squares (topsail)
                    {
                        for (int num = component.mastOrder - 1; num >= 0; num--)
                        {
                            if (__instance.sails[num] != null && __instance.sails[num].GetComponent<Sail>().squareSail)
                            {
                                topsail = true;
                            }
                        }
                    }
                    if ((bool)component2.reefController)
                    {
                        WinchColor(reef[component.mastOrder], winchColorIndex);
                    }
                    if ((bool)component2.angleControllerMid)
                    {
                        WinchColor(middle[component.mastOrder], winchColorIndex);
                    }
                    if ((bool)component2.angleControllerLeft || (bool)component2.angleControllerRight)
                    {
                        if (component.squareSail)
                        {
                            if(!topsail)
                            {   //it's not a topsail square, we only have the lower winches so skip everything else.
                                WinchColor(left[0], winchColorIndex);
                                WinchColor(right[0], winchColorIndex);
                            }
                            else
                            {
                                //just a topsail, don't color the winches...
                            }
                        }
                        else
                        {   //color the winches with mastOrder, this is for when you have multiple staysay on a single mast, I believe
                            WinchColor(left[component.mastOrder], winchColorIndex);
                            WinchColor(right[component.mastOrder], winchColorIndex);
                        }
                    }
                    winchColorIndex++;
                }
            }
        }
    }
}
