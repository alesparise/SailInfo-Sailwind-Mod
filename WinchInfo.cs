using System.Text;
using UnityEngine;

namespace SailInfo
{
    public class WinchInfo : MonoBehaviour
    {
        protected RopeController rope;
        public virtual void Awake()
        {
            rope = GetComponent<RopeController>();
        }
        protected StringBuilder Bar(float amount)
        {
            StringBuilder bar = new StringBuilder();
            int barLength = 300; // Total length of the loading bar
            int filledLength = (int)(amount * barLength); // Calculate the number of filled characters

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
            return bar;
        }
        private float AnchorAngle()
        {   //calculates the angle of the anchor rope
            float angle = 0f;
            if (rope is RopeControllerAnchor anchorRope)
            {
                var joint = anchorRope.joint;
                Vector3 bottomAttach = joint.transform.position;
                Vector3 topAttach = joint.connectedBody.transform.TransformPoint(joint.connectedAnchor);
                angle = Vector3.Angle(topAttach - bottomAttach, Vector3.up);
            }
            return angle;
        }
        public virtual string WinchHUD()
        {   //possibly useful ASCII characters: ████░░░░░░ ████▒▒▒▒ ■□□□□ ▄▄▄... ▀▀    ═══───
            //this adds the bar, but it's also what calls most of the other stuff
            //SAIL EFFICIENCY / NAMING THINGS
            float amount = rope.currentLength;
            string description = "";
            if (rope is RopeControllerAnchor anchorRope)
            {
                float yardsOut = amount * anchorRope.maxLength;
                if (yardsOut > 0.05f)
                {
                    if (SailInfoMain.winchesAnchorAngleConfig.Value == ColorCoding.ColorCoded)
                    {
                        float angleToBoat = AnchorAngle();
                        if (angleToBoat > 50f)
                        {   //positive angle means right, so green color.
                            description += $"<color=#113905>{Mathf.Round(angleToBoat)}°</color>";
                        }
                        else
                        {
                            description += $"<color=#7C0000>{Mathf.Round(angleToBoat)}°</color>";
                        }
                    }
                    else if (SailInfoMain.winchesAnchorAngleConfig.Value == ColorCoding.Plain)
                    {
                        description += $"{Mathf.Round(AnchorAngle())}°";
                    }
                }
                if (SailInfoMain.winchesOutConfig2.Value == WinchOutTypeLinear.Absolute)
                {   //X° out (0° - maxAngle°)
                    description += $"<size=70%>\n{Mathf.Round(yardsOut)} yds out</size>";
                }
                else if (SailInfoMain.winchesOutConfig2.Value == WinchOutTypeLinear.Percent)
                {   //X out (0-100)
                    description += $"<size=70%>\n{Mathf.Round(amount * 100)}% out</size>";
                }
            }
            if (SailInfoMain.winchesBarConfig.Value)
            {
                description += $"\n<size=3%>{Bar(amount)}</size>";
            }
            return description;
        }
    }
}

