using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
//poorly written by pr0skynesis (discord username)

namespace SailInfo
{   /// <summary>
    /// CHANGELOG: v1.2.0
    /// • Giant code refactor by NAND
    /// • Changed configuration file for better readability
    /// •
    /// 
    /// TODO: v1.3.0
    /// • Complete code rework and improvements in efficiency
    /// • Add Center of Effort indicator in the shipyard
    /// • Improve the winch color system and sail naming system
    /// • Add sail force viewer (maybe as a debug tool only?) 
    /// • More informations might be worth adding to the shipyard infobox:
    ///     - Total sail area
    ///     - Some kind of representation of expected upwind and downwind performances
    ///     - Total weight of the current setup
    /// • Telltales on sail to communicate sail efficiency, they get straighter the more efficient the sail is.
    /// • Wind particles, water sprays, etc. to show wind direction and speed
    /// • Spatialized wind sounds to give wind direction and speed based on where you look at
    /// </summary>
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class SailInfoMain : BaseUnityPlugin
    {
        // Necessary plugin info
        public const string pluginGuid = "pr0skynesis.sailinfo";
        public const string pluginName = "SailInfo";
        public const string pluginVersion = "1.2.0"; //big wip rework should be 1.3.0 //1.2.0 was the coordinate NAND update

        //config file info
        //MAIN SWITCHES
        public static ConfigEntry<bool> winchesInfoConfig;      //Disables the winches things entirely.
        public static ConfigEntry<bool> rudderHUDConfig;        //Disables the rudder HUD entirely.
                                                                //winches stuff
        public static ConfigEntry<ColorCoding> winchesAnchorAngleConfig;
        public static ConfigEntry<WinchOutType> winchesOutConfig;
        public static ConfigEntry<WinchOutTypeLinear> winchesOutConfig2;
        public static ConfigEntry<bool> winchesBarConfig;
        //Efficiency
        public static ConfigEntry<bool> sailEfficiencyConfig;
        public static ConfigEntry<bool> sailForwardForceConfig;
        public static ConfigEntry<bool> sailSidewaysForceConfig;
        //Various
        public static ConfigEntry<bool> flipWinchesOutConfig;
        public static ConfigEntry<SailNameType> sailNameConfig;
        public static ConfigEntry<bool> coloredWinchesConfig;   //Colors the all the winches attached to a currentSail in one color.
        //rudder
        public static ConfigEntry<bool> rudderBarConfig;
        //Wind Absolute
        public static ConfigEntry<WindSpeedType> windSpeedConfig;    //enables the wind speed display
        public static ConfigEntry<HeadingType> windDirectionConfig;    //enables absolute wind direction display
        //Point of Sail
        public static ConfigEntry<ColorCoding> windRelativeConfig; //enable the display of the wind direction relative to the boat (0° to 180° and 0° to -180°)
        //Boat Heading
        public static ConfigEntry<HeadingType> boatHeadingConfig; //enables boat heading display
        //Boat Speed
        public static ConfigEntry<SpeedUnits> boatSpeedConfig;   //enables the boat speed display
        public static ConfigEntry<bool> boatVMGConfig;      //enables VMG display
        //boat heeling
        public static ConfigEntry<bool> boatHeelingConfig;  //displays boat heeling in ° (Category: Rudder HUD: other)
        //coordinates
        public static ConfigEntry<CoordinateType> showCoordinatesConfig; //displays coordinates on the rudder
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
            winchesOutConfig = Config.Bind("B) Winches Display: 1. Sail Trimming and Efficiency", "winchesOutText", WinchOutType.Degrees, "Shows 'X out' when looking at winches.\nPercent: 0 means the winch is fully tightened in, 100 means it's fully released.\nReplaces the 'X out' text with the degrees the sail is out, e.g. '45° out'.");
            winchesBarConfig = Config.Bind("B) Winches Display: 1. Sail Trimming and Efficiency", "winchesBar", true, "Shows a loading bar type of thing when looking at winches. Empty bar means the winch is fully tightened in, 100 means it's fully released. Set to false to disable.");
            sailEfficiencyConfig = Config.Bind("B) Winches Display: 1. Sail Trimming and Efficiency", "sailEfficiency", true, "Shows the efficiency of the sail attached to the winch you are looking at. Does not apply to halyard winches. Negative means sail is pushing backwards. Maximize to find the best trim. Set to false to disable.");
            sailForwardForceConfig = Config.Bind("B) Winches Display: 1. Sail Trimming and Efficiency", "sailForwardForce", false, "Shows how close the FORWARD force generated by the sail is to the maximum. 0% means no force forward, 100% means maximum force forward. Maximize for best result. This was called Sail Efficiency up to v1.1.8. Set to true to enable.");
            sailSidewaysForceConfig = Config.Bind("B) Winches Display: 1. Sail Trimming and Efficiency", "sailSidewayForce", false, "Shows how close the SIDEWAYS force generated by the sail is to the maximum. 0% means no force sideway, 100% means maximum force sideway. Minimize for best result. Set to true to enable.");
            flipWinchesOutConfig = Config.Bind("B) Winches Display: 1. Sail Trimming and Efficiency", "flipWinchesOut", true, "Flips the 'X out' when looking at halyard winches of sail that folds by releasing the rope, like junk sails and some gaffs. When true the sail will be at 100 out when fully opened and at 0 when close, making it more consistent with other type of sail.");
            //Sail Names
            sailNameConfig = Config.Bind("B) Winches Display: 2. Sail Names", "sailName", SailNameType.None, "Shows the name of the sail attached to the winch currently looked at.\nStandard: Exact sail name (e.g. \"brig square 9yd\").\nHistorical: Traditional names based on mast and position (e.g. \"Mizzen T'Gallant\").\nSimple Positional: Uses simple prefixes for sail names (Bottom, Middle, Top, Highest) followed by sail type (Square, Gaff, Lateen, etc.).");
            coloredWinchesConfig = Config.Bind("B) Winches Display: 2. Sail Names", "coloredWinches", false, "Colors all the winches attached to a single sail of the same color, making it easier to identify. Change to true to enable.");
            // anchor information
            winchesAnchorAngleConfig = Config.Bind("B) Winches Display: 3. Anchor Information", "anchorAngle", ColorCoding.ColorCoded, "Shows angle between anchor and pulley when looking at winch. Set to false to disable.");
            winchesOutConfig2 = Config.Bind("B) Winches Display: 3. Anchor Information", "anchorOutText", WinchOutTypeLinear.Absolute, "Shows 'X out' when looking at winches.\nPercent: 0 means the winch is fully tightened in, 100 means it's fully released.\nReplaces the 'X out' text with the amount of rope out, e.g. '45 yds out'.");

            //HUD (all information displayed on the rudder when looking at it)
            //rudder
            rudderBarConfig = Config.Bind("C) Rudder HUD: 5. Other", "rudderBar", true, "Shows a loading bar type of thing indicating how much the rudder is turned either way.  Set to false to disable.");
            //Apparent Wind
            windSpeedConfig = Config.Bind("C) Rudder HUD: 1. Apparent Wind", "windSpeed", WindSpeedType.Beaufort, "Shows the APPARENT wind speed.\nKnots: Wind speed in knots.\nBeaufort: Descriptor words (e.g. Calm, Light Breeze, Gale, etc.");
            windDirectionConfig = Config.Bind("C) Rudder HUD: 1. Apparent Wind", "windDirection", HeadingType.Cardinal, "Shows APPARENT wind direction in degree, relative to the world (e.g. 45° means the APPARENT wind comes from the North-West).\nDegrees: show precise wind direction in degrees.\nCardinal: Approximate cardinal direction (e.g. wind coming from 45° will be shown as NW instead of it's exact numerical value.");
            //Point of Sail
            windRelativeConfig = Config.Bind("C) Rudder HUD: 2. Point of Sail", "windAngleToBoat", ColorCoding.ColorCoded, "Shows the angle (0°-180°) between the APPARENT wind and the boat forward direction. Negative angles mean the wind comes from the left of the boat.\nPlain: White text\nColor Coded: Green if wind comes from the right, red if wind comes from the left.");
            //Heading
            boatHeadingConfig = Config.Bind("C) Rudder HUD: 3. Heading", "boatHeading", HeadingType.Cardinal, "Shows the boat heading.\nDegrees: Heading in degrees.\nCardinal: Approximate boat heading in cardinal directions (e.g. N, NW, SSE, etc.).");
            //Boat Speed
            boatSpeedConfig = Config.Bind("C) Rudder HUD: 4. Boat Speed", "boatSpeed", SpeedUnits.None, "Shows current boat speed.\nKnots: speed in chiplog knots.\nNautical Miles Per Hour: speed in nautical miles per hour instead of kts. In the game nmi/h = kts / 1.555.");
            boatVMGConfig = Config.Bind("C) Rudder HUD: 4. Boat Speed", "boatVMG", false, "Shows the current boat VMG in kts. Set to false to disable.");
            //Boat heeling
            boatHeelingConfig = Config.Bind("C) Rudder HUD: 5. Other", "boatHeeling", false, "Shows current boat heeling in degrees.");
            //Coordinates
            showCoordinatesConfig = Config.Bind("C) Rudder HUD: 5. Other", "showCoordinates", CoordinateType.None, "Adds latitude and longitude to the rudder HUD. Set to true to enable.");
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
            MethodInfo patch2 = AccessTools.Method(typeof(SailInfoPatches), "WheelUpdate_Patch");

            MethodInfo original3 = AccessTools.Method(typeof(SailConnections), "Awake");
            MethodInfo patch3 = AccessTools.Method(typeof(SailInfoPatches), "Awake_Patch");
            MethodInfo original4 = AccessTools.Method(typeof(Mast), "UpdateControllerAttachments");
            MethodInfo patch4 = AccessTools.Method(typeof(SailInfoPatches), "UpdateControllerAttachments_Patch");
            //clock
            MethodInfo original5 = AccessTools.Method(typeof(ShipItemClock), "ExtraLateUpdate");
            MethodInfo patch5 = AccessTools.Method(typeof(SailInfoPatches), "ClockPatch");
            MethodInfo original15 = AccessTools.Method(typeof(RopeControllerAnchor), "Start");
            MethodInfo patch15 = AccessTools.Method(typeof(SailInfoPatches), "Capstan_Patch");

            //PATCH APPLICATION
            if (winchesInfoConfig.Value)
            {
                harmony.Patch(original15, new HarmonyMethod(patch15));
                harmony.Patch(original, new HarmonyMethod(patch));   //winches info
                harmony.Patch(original3, new HarmonyMethod(patch3)); //winches info (for the currentSail efficiency stuff)
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
            #endregion
        }
    }
    public class SailInfoPatches   //contains the patch
    {
        //VARIABLES
        //public static Dictionary<Sail, List<RopeController>> sailRopeControllerMap = new Dictionary<Sail, List<RopeController>>();
        public static int winchColorIndex = 0;
        public static Color[] colorArray = { Color.black, Color.blue, Color.cyan, Color.gray, Color.green, Color.magenta, Color.red, Color.white, Color.yellow };

        //WINCHES METHODS
        public static void Update_Patch(GPButtonRopeWinch __instance, GoPointer ___stickyClickedBy, bool ___isLookedAt, bool ___isClicked)
        {
            if (___isLookedAt || ___stickyClickedBy || ___isClicked)
            {
                __instance.description = __instance.rope.GetComponent<WinchInfo>()?.WinchHUD();
            }
        }
        public static void Capstan_Patch(RopeControllerAnchor __instance)
        {
            __instance.gameObject.AddComponent<WinchInfo>();
        }
        public static void WheelUpdate_Patch(GPButtonSteeringWheel __instance, ref string ___description, GoPointer ___stickyClickedBy, bool ___isLookedAt, bool ___isClicked)
        {
            if (___isLookedAt || ___stickyClickedBy || ___isClicked)
            {
                var component = __instance.GetComponent<BoatInfo>() ?? __instance.gameObject.AddComponent<BoatInfo>();
                ___description = component.RudderHUD();
            }
        }
        public static string SailName(Sail currentSail, SailNameType nameType)
        {
            //FieldInfo currentInstallHeightInfo = AccessTools.Field(typeof(Sail), "currentInstallHeight");   //get ready to access currentInstallHeight
            string[] historicalPrefixes = { "Course", "Topsail", "T'gallant", "Royal", "Skysail", "Moonraker" };
            string[] simplePrefixes = { "Bottom Square", "Lower Square", "Middle Square", "Upper Square", "Top Square", "Highest Square" };

            Mast currentMast = currentSail.transform.parent.GetComponent<Mast>();   //get the mast the currentSail is attached to
            //Sail[] sails = currentMast.GetComponentsInChildren<Sail>(); //get all the sails attached to said mast

            //sails = sails.OrderBy(currentSail => (float)currentInstallHeightInfo.GetValue(currentSail)).ToArray();    //sort the sails array based on currentInstallHeight

            if (currentSail.squareSail)
            {
                if (currentMast.onlySquareSails && nameType == SailNameType.Historical)
                {
                    if (currentSail.GetComponent<SquareTopsailAngleMirror>())
                    {
                        return "Sprit Topsail";
                    }
                    return "Spritsail";
                }
                var sailPrefixes = nameType == SailNameType.Historical ? historicalPrefixes : simplePrefixes;
                int j = 0; //index to access the sailPrefixes
                var mir = currentSail.GetComponent<SquareTopsailAngleMirror>();
                for (j = 0; j < sailPrefixes.Length;)
                {
                    if (mir == null) break;
                    currentMast = mir.sailBelow.transform.parent.GetComponent<Mast>();
                    j++;
                    mir = mir.sailBelow.GetComponent<SquareTopsailAngleMirror>() ?? null;
                }
                return SailInfoPatches.MastName(currentMast) + sailPrefixes[j];
            }
            if (currentSail.category == SailCategory.gaff)
            {
                return SailInfoPatches.MastName(currentMast) + "Gaff";
            }
            if (currentSail.category == SailCategory.lateen)
            {
                return SailInfoPatches.MastName(currentMast) + "Lateen";
            }
            if (currentSail.category == SailCategory.junk)
            {
                return SailInfoPatches.MastName(currentMast) + "Junk";
            }
            if (currentSail.category == SailCategory.other)
            {
                if (currentSail.name.Contains("junklateen"))
                {
                    return SailInfoPatches.MastName(currentMast) + "Fin";
                }
                if (currentSail.name.Contains("lug"))
                {
                    return SailInfoPatches.MastName(currentMast) + "Lug";
                }
            }
            if (currentSail.category == SailCategory.staysail)
            {
                if (MastName(currentMast) == "Forestay ")
                {
                    if (currentSail.name.Contains("genoa"))
                    {
                        return "Genoa";
                    }
                    return "Jib";
                }
                return SailInfoPatches.MastName(currentMast) + "Staysail";
            }
            return "";
        }
        public static string MastName(Mast currentMast)
        {   //returns a human-readable mast name from the Mast gameObject name
            string optionName = currentMast.GetComponent<BoatPartOption>()?.optionName ?? "";

            if (currentMast.onlyStaysails)
            {
                if (optionName.ToLower().Contains("forestay"))
                {   //Checks if this mast is a forestay
                    return "Forestay ";
                }
                foreach (var req in currentMast.GetComponent<BoatPartOption>().requires)
                {
                    if (req.optionName.ToLower().Contains("mizzen"))
                    {
                        return "Mizzen ";
                    }
                }
            }
            Mast[] mastList = GameState.currentBoat.GetComponentsInChildren<Mast>();
            int mastCount = mastList.Length;
            for (int i = 0; i < mastCount; i++)
            {   //Checks if one of the mast is a bowsprit or a stay and removes them from the mast count. We only care about foremast, mainmast and mizzen
                if (mastList[i].onlyStaysails || mastList[i].onlySquareSails)
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
                else if (optionName.ToLower().Contains("foremast"))
                {   //Checks if this mast is the foremast
                    return "Foremast ";
                }
                else if (optionName.ToLower().Contains("main") || currentMast.name == "mast" || currentMast.name == "mast_1" || currentMast.name == "mast_0_extension" || currentMast.name == "mast_1_extension" || currentMast.name == "mast_center" || currentMast.name == "mast_mid_0" || currentMast.name == "mast_mid_1" || currentMast.name == "mast_front" || currentMast.name == "mast_Back_1" || currentMast.name == "mast_Back_2")
                {   //Checks if this mast is the mainmast
                    if (optionName.ToLower().Contains("topmast"))
                    {
                        return "Main Topmast ";
                    }
                    return "Mainmast ";
                }
                else if (optionName.ToLower().Contains("mizzen"))
                {   //Checks if this mast is the mizzen mast
                    if (optionName.ToLower().Contains("topmast"))
                    {
                        return "Mizzen Topmast ";
                    }
                    return "Mizzen ";
                }
            }
            return "";
        }
        private static void WinchColor(GPButtonRopeWinch winch, int winchColorIndex)
        {   //Colors each winch associated to a specific currentSail in a unique way.
            winchColorIndex %= colorArray.Length;
            winch.transform.GetComponent<MeshRenderer>().material.color = colorArray[winchColorIndex];
        }

        // CLOCK
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

        //SAIL → WINCHES MAP
        public static void Awake_Patch(Sail ___sail, RopeController ___reefController, RopeController ___angleControllerMid, RopeController ___angleControllerLeft, RopeController ___angleControllerRight)
        {
            // Check if any rope controller is not null
            if (___angleControllerMid != null || (___angleControllerLeft != null && ___angleControllerRight != null))
            {
                // Add existing rope controllers to the list
                if (___angleControllerMid != null)
                {
                    var midController = ___angleControllerMid.gameObject.GetComponent<WinchInfoSail>() ?? ___angleControllerMid.gameObject.AddComponent<WinchInfoSail>();
                    midController.sailComponent = ___sail;
                }
                if (___angleControllerLeft != null && ___angleControllerRight != null)
                {
                    var leftController = ___angleControllerLeft.gameObject.GetComponent<WinchInfoSail>() ?? ___angleControllerLeft.gameObject.AddComponent<WinchInfoSail>();
                    leftController.sailComponent = ___sail;
                    var rightController = ___angleControllerRight.gameObject.GetComponent<WinchInfoSail>() ?? ___angleControllerRight.gameObject.AddComponent<WinchInfoSail>();
                    rightController.sailComponent = ___sail;
                }
                var reefController = ___reefController.gameObject.GetComponent<WinchInfoSail>() ?? ___reefController.gameObject.AddComponent<WinchInfoSail>();
                reefController.sailComponent = ___sail;
            }
            else
            {
                //Debug.LogError("WinchInfoSail: all rope controllers are null!");
            }
        }
        //WINCH RECOLORING
        public static void UpdateControllerAttachments_Patch(Mast __instance)
        {
            if (GameState.currentlyLoading)
            {
                //this goes through all the sails attached to a mast and assigns
                //unique color to each group of winches associated with a single currentSail.
                GPButtonRopeWinch[] left = __instance.leftAngleWinch;
                GPButtonRopeWinch[] right = __instance.rightAngleWinch;
                GPButtonRopeWinch[] middle = __instance.midAngleWinch;
                GPButtonRopeWinch[] reef = __instance.reefWinch;

                foreach (GameObject sail in __instance.sails)
                {
                    Sail component = sail.GetComponent<Sail>();
                    SailConnections component2 = sail.GetComponent<SailConnections>();
                    bool topsail = false;
                    if (component.squareSail)   //this checks if the currentSail in question is a square above other squares (topsail)
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
                            if (!topsail)
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
