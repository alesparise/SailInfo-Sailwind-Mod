using System;
using UnityEngine;

namespace SailInfo
{
    public class SailInfoPatches
    {
        //VARIABLES
        public static int winchColorIndex = 0;
        public static Color[] colorArray = { Color.black, Color.blue, Color.cyan, Color.gray, Color.green, Color.magenta, Color.red, Color.white, Color.yellow };

        //WINCHES METHODS
        public static void UpdatePatch(GPButtonRopeWinch __instance, GoPointer ___stickyClickedBy, bool ___isLookedAt, bool ___isClicked)
        {
            if (___isLookedAt || ___stickyClickedBy || ___isClicked)
            {
                __instance.description = __instance.rope.GetComponent<WinchInfo>()?.WinchHUD();
            }
        }
        public static void CapstanPatch(RopeControllerAnchor __instance)
        {
            __instance.gameObject.AddComponent<WinchInfo>();
        }
        //RUDDER HUD PATCHES
        
        public static void RudderPatch(Rudder __instance)
        {   //patching the rudder to find the tiller on modded boats (could patch anything but this seems fitting)

            Transform boatModel = __instance.GetComponentInParent<BoatHorizon>()?.transform;
            if (boatModel == null) return;
            Transform tiller1;
            Transform tiller2;

            if (boatModel.name == "cutterModel")
            {
                tiller1 = __instance.transform.Find("rudder_tiller_cutter");
                tiller2 = __instance.transform.Find("rudder_tiller_cutter_center");
            }
            else if (boatModel.name == "paraw")
            {
                tiller1 = boatModel.Find("hull_regular").Find("rudder_right_reg").Find("tiller_reg");
                tiller2 = boatModel.Find("hull_extension").Find("rudder_right_ext").Find("tiller_ext");
            }
            else
            {
                GPButtonSteeringWheel[] wheels = boatModel.GetComponentsInChildren<GPButtonSteeringWheel>();
                foreach (GPButtonSteeringWheel wheel in wheels)
                {
                    wheel.gameObject.AddComponent<RudderHUD>();
                }

                return;
            }

            tiller1.gameObject.AddComponent<RudderHUD>();
            tiller2.gameObject.AddComponent<RudderHUD>();
        }
        public static string SailName(Sail currentSail, SailNameType nameType)
        {
            string[] historicalPrefixes = { "Course", "Topsail", "T'gallant", "Royal", "Skysail", "Moonraker" };
            string[] simplePrefixes = { "Bottom Square", "Lower Square", "Middle Square", "Upper Square", "Top Square", "Highest Square" };

            Mast currentMast = currentSail.transform.parent.GetComponent<Mast>();   //get the mast the currentSail is attached to

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
                return MastName(currentMast) + sailPrefixes[j];
            }
            if (currentSail.category == SailCategory.gaff)
            {
                return MastName(currentMast) + "Gaff";
            }
            if (currentSail.category == SailCategory.lateen)
            {
                return MastName(currentMast) + "Lateen";
            }
            if (currentSail.category == SailCategory.junk)
            {
                return MastName(currentMast) + "Junk";
            }
            if (currentSail.category == SailCategory.other)
            {
                if (currentSail.name.Contains("junklateen"))
                {
                    return MastName(currentMast) + "Fin";
                }
                if (currentSail.name.Contains("lug"))
                {
                    return MastName(currentMast) + "Lug";
                }
                if (currentSail.name.Contains("tanja"))
                {
                    return MastName(currentMast) + "Tanja";
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
                return MastName(currentMast) + "Staysail";
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
        public static void AwakePatch(Sail ___sail, RopeController ___reefController, RopeController ___angleControllerMid, RopeController ___angleControllerLeft, RopeController ___angleControllerRight)
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
        public static void UpdateControllerAttachmentsPatch(Mast __instance)
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

        //TILLER COMPATIBILITY
        private static Component GetComponentByString(GameObject obj, string component)
        {
            
            return obj.GetComponent(component);
        }
    }
}
