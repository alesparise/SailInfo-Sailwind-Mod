using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SailInfo
{
    public class WinchInfoSail : WinchInfo
    {
        public Sail sailComponent;
        private string sailName;
        public SailNameType nameType;
        FieldInfo unamplifiedForceInfo;
        FieldInfo totalWindForceInfo;

        public override void Awake()
        {
            base.Awake();
            //sailComponent = GetComponent<Sail>();
            unamplifiedForceInfo = AccessTools.Field(typeof(Sail), "unamplifiedForwardForce");
            totalWindForceInfo = AccessTools.Field(typeof(Sail), "totalWindForce");
        }

        public string SailName()
        {
            if (SailInfoMain.sailNameConfig.Value == SailNameType.Standard)
            {
                return sailComponent.sailName;
            }
            if (sailName != null && sailName.Length > 0 && nameType == SailInfoMain.sailNameConfig.Value)
            {
                return sailName;
            }
            nameType = SailInfoMain.sailNameConfig.Value;

            sailName = SailInfoPatches.SailName(sailComponent, nameType);
            return sailName;
        }

        private int SailDegree()
        {   //gets the sailComponent in and returns the angle wiht the boat forward direction out out                      
            Vector3 boatVector = sailComponent.shipRigidbody.transform.forward;    //boat direction
            Vector3 sailVector = sailComponent.squareSail ? sailComponent.transform.up : -sailComponent.transform.right; //sailComponent "direction" since squares are made differently we use the up direction for them, otherwise the -right direction (also known as left)

            int angle = Mathf.RoundToInt(Vector3.SignedAngle(boatVector, sailVector, -Vector3.up)); //calculate the angle

            angle = angle > 90 ? 180 - angle : angle; //keep it in a 0° to 90° angle
            angle = angle < 0 ? -angle : angle; //keep it positive
            return angle;
        }
        private float SailEfficiency()
        {   // Calculates the efficiency of a sailComponent trim (max is best)

            //This is the force created by the sailComponent
            float unamplifiedForce = (float)unamplifiedForceInfo.GetValue(sailComponent);
            //This is the total force the wind applies to the sailComponent. This is also the maximum force forward the sailComponent can generate on the boat.
            float totalWindForce = (float)totalWindForceInfo.GetValue(sailComponent);

            float efficiency = Mathf.Round(unamplifiedForce / totalWindForce * 100f);

            return efficiency;
        }
        private float SailInefficiency()
        {   // Calculates the percentage of sideway force on a sailComponent (min is best)
            float unamplifiedForce = (float)unamplifiedForceInfo.GetValue(sailComponent);

            //This is the total force the wind applies to the sailComponent. This is also the maximum force forward the sailComponent can generate on the boat.
            float totalWindForce = (float)totalWindForceInfo.GetValue(sailComponent);

            float inefficiency = Mathf.Abs(Mathf.Round(unamplifiedForce / totalWindForce * 100f));

            return inefficiency;
        }
        private float CombinedEfficiency()
        {   //combines Efficiency and Inefficiency into one single value (max is best)
            //this is the real efficiency!
            float eff = SailEfficiency();
            if (eff <= 0f)
            {
                return eff;
            }
            float ineff = 100 - SailInefficiency();
            float comb = Mathf.Round((eff + ineff) / 2f);

            return comb;
        }
        public override string WinchHUD()
        {
            string description = "";
            float amount = rope.currentLength;
            float sailOut = Mathf.Round(amount * 100);
            if (SailInfoMain.flipWinchesOutConfig.Value && SailInfoMain.winchesOutConfig.Value != WinchOutType.None && rope is RopeControllerSailReef castedRope)
            {   //flip the X out text for reverseReefing sailComponent
                if (castedRope.reverseReefing)
                {   //flips the out thingy
                    amount = 1f - amount;
                }
            }
            
            if (rope is RopeControllerSailReef)
            {   //do this if it's an halyard winch
                if (SailInfoMain.flipWinchesOutConfig.Value || SailInfoMain.sailNameConfig.Value != SailNameType.None)
                {   //The two cases where we actually need to know what the sailComponent attached to the controller is
                    description += $"<size=70%>{SailName()} Halyard\n</size>";
                }
                if (SailInfoMain.winchesOutConfig.Value != WinchOutType.None)
                {
                    description += $"<size=70%>{sailOut} out</size>";
                }
                if (SailInfoMain.winchesBarConfig.Value)
                {
                    description += $"\n<size=3%>{Bar(amount)}</size>";
                }
            }
            else
            {   //do this if it's a sheet winch
                if (SailInfoMain.sailEfficiencyConfig.Value || SailInfoMain.sailForwardForceConfig.Value || SailInfoMain.sailSidewaysForceConfig.Value || SailInfoMain.sailNameConfig.Value != SailNameType.None)
                {   //Identifies what sailComponent the controller being looked at is attached to!
                    if (SailInfoMain.sailNameConfig.Value != SailNameType.None)
                    {
                        description += $"<size=70%>{SailName()}\n</size>";
                    }
                    if (SailInfoMain.sailEfficiencyConfig.Value)
                    {
                        description += $"<size=70%>Eff: {CombinedEfficiency()}% </size>";
                    }
                    if (SailInfoMain.sailForwardForceConfig.Value)
                    {
                        description += $"<size=70%>F: {SailEfficiency()}% </size>";
                    }
                    if (SailInfoMain.sailSidewaysForceConfig.Value)
                    {
                        description += $"<size=70%>S: {SailInefficiency()}% </size>";
                    }
                    if (SailInfoMain.winchesOutConfig.Value == WinchOutType.Degrees)
                    {   //X° out (0° - maxAngle°)
                        description += $"<size=70%>\n{SailDegree()}° out</size>";
                    }
                    else if (SailInfoMain.winchesOutConfig.Value == WinchOutType.Percent)
                    {   //X out (0-100)
                        description += $"<size=70%>\n{sailOut} out</size>";
                    }
                }
                if (SailInfoMain.winchesBarConfig.Value)
                {
                    description += $"\n<size=3%>{Bar(amount)}</size>";
                }
            }
            return description;
        }
    }
}
