using System;
using System.Text;
using UnityEngine;


namespace SailInfo
{
    internal class BoatInfo : MonoBehaviour
    {
        private Transform boat;

        private Rigidbody shipRigidbody;

        private HingeJoint rudderJoint;

        private Rudder rudder;

        public void Awake()
        {
            boat = GetComponentInParent<PurchasableBoat>().transform;
            shipRigidbody = boat.GetComponent<Rigidbody>();
            rudder = boat.GetComponentInChildren<Rudder>();
            rudderJoint = rudder.GetComponent<HingeJoint>();
        }

        public string RudderHUD()
        {
            string text = "";
            //Wind Speed
            if (SailInfoMain.windSpeedConfig.Value > 0 || SailInfoMain.windDirectionConfig.Value > 0 || SailInfoMain.windRelativeConfig.Value > 0)
            {
                text = "Wind: ";
            }
            if (SailInfoMain.windSpeedConfig.Value == WindSpeedType.Beaufort)
            {
                text += $"{Beaufort()} ";
            }
            else if (SailInfoMain.windSpeedConfig.Value == WindSpeedType.Knots)
            {
                text += $"{Mathf.Round(WindForce())} kts ";
            }

            //Wind Direction Absolute
            if (SailInfoMain.windDirectionConfig.Value == HeadingType.Cardinal)
            {
                text += $"{DirectionNESW(Mathf.Round(WindDirection()))} ";
            }
            else if (SailInfoMain.windDirectionConfig.Value == HeadingType.Degrees)
            {
                text += $"{Mathf.Round(WindDirection())}° ";
            }

            //Wind Direction Relative
            if (SailInfoMain.windRelativeConfig.Value == ColorCoding.ColorCoded)
            {
                float angleToBoat = AngleToBoat();
                if (angleToBoat > 0f)
                {   //positive angle means right, so green color.
                    text += $"<color=#113905>{Mathf.Round(angleToBoat)}°</color>";
                }
                else
                {
                    text += $"<color=#7C0000>{Mathf.Round(angleToBoat)}°</color>";
                }
            }
            else if (SailInfoMain.windRelativeConfig.Value == ColorCoding.Plain)
            {
                text += $"{Mathf.Round(AngleToBoat())}°";
            }

            //Boat Stuff
            if (SailInfoMain.boatHeadingConfig.Value != HeadingType.None || SailInfoMain.boatSpeedConfig.Value != SpeedUnits.None || SailInfoMain.boatVMGConfig.Value)
            {
                text += "\n";
            }

            //Boat Speed
            if (SailInfoMain.boatSpeedConfig.Value == SpeedUnits.NauticalMilesPerHour)
            { // ↓↓ magic number to convert chiplog knots into timescale-adjusted nm/h
                float timeMultiplier = 80.36f * Sun.sun.initialTimescale;
                text += $"SPD: {Mathf.Round(BoatSpeed() * timeMultiplier)} nmi/h ";
            }
            else if (SailInfoMain.boatSpeedConfig.Value == SpeedUnits.Standard)
            {
                text += $"SPD: {Mathf.Round(BoatSpeed())} kts ";
            }

            //Boat Heading
            if (SailInfoMain.boatHeadingConfig.Value == HeadingType.Cardinal)
            {
                text += $"HDG: {DirectionNESW(Mathf.Round(BoatHeading()))} ";
            }
            else if (SailInfoMain.boatHeadingConfig.Value == HeadingType.Degrees)
            {
                text += $"HDG: {Mathf.Round(BoatHeading())}° ";
            }

            //VMG
            if (SailInfoMain.boatVMGConfig.Value)
            {
                text += $"VMG: {Mathf.Round(VMG())} kts";
            }

            //Heeling
            if (SailInfoMain.boatHeelingConfig.Value)
            {
                text += $"\nHeeling {Mathf.Round(Heeling())}°";
            }
            if (SailInfoMain.showCoordinatesConfig.Value != CoordinateType.None)
            {
                text += $"\n{Latitude()}, {Longitude()}";
            }
            if (SailInfoMain.rudderBarConfig.Value)
            {
                text += $"\n{RudderBar(rudder.currentAngle, rudderJoint.limits.max)}";
            }
            return text;
        }
        private string RudderBar(float currentAngle, float angleLimit)
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
        private float WindForce()
        {
            return GetApparentWind().magnitude;
        }
        private Vector3 GetApparentWind()
        {
            Vector3 result = Wind.currentWind - shipRigidbody.velocity;
            return result;
        }
        private float WindDirection()
        {   //calculate apparent wind direction in an absolute frame of reference
            float windDirection = Vector3.SignedAngle(GetApparentWind(), -Vector3.forward, -Vector3.up);

            return windDirection < 0 ? windDirection + 360f : windDirection;
        }
        private float BoatHeading()
        {   //calculates boat heading in an absolute frame of reference
            float boatHeading = Vector3.SignedAngle(boat.forward, Vector3.forward, -Vector3.up);
            boatHeading = boatHeading < 0 ? boatHeading + 360f : boatHeading;

            return boatHeading;
        }
        private float AngleToBoat()
        {   //calculates angle between boat and apparent wind, 0 to 180°. Positive on the right, negative on the left
            return Vector3.SignedAngle(-boat.forward, GetApparentWind(), Vector3.up);
        }
        private float BoatSpeed()
        {   //calculates boat speed in kts
            return shipRigidbody.velocity.magnitude * 1.94384f;
        }
        private static string DirectionNESW(float direction)
        {   //takes in an angle 0-360 and returns a string representing it's cardinal direction (eg. N, NNW, NW, etc.)

            string[] sectors = { "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW", "N" };

            string cardinal = sectors[(int)Math.Floor(((direction + 11.25) % 360) / 22.5)];
            return cardinal;
        }
        private float VMG()
        {   //calculates the VMG of the boat

            Vector3 velocity = shipRigidbody.velocity;
            Vector3 trueWind = Wind.currentWind;
            float angle = Vector3.Angle(velocity, trueWind) * Mathf.Deg2Rad;

            return -BoatSpeed() * Mathf.Cos(angle);
        }
        //private static string PointOfSail()
        //{ //returns the point of sailComponent in 32-esimes (eg, 1/32 is headwind, I think)
        //  using things like broad reach, close hauled and so on might be an option. Not sure.
        //  return "";
        //}
        private static string Beaufort()
        {   //returns a descriptor text indicating how strong the wind is.
            float windSpeed = (Wind.currentWind - GameState.currentBoat.parent.GetComponent<Rigidbody>().velocity).magnitude;

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
        private float Heeling()
        {   //returns boat heeling in degrees
            float boatHeading = Vector3.SignedAngle(boat.up, Vector3.up, -Vector3.forward);

            return boatHeading;
        }
        internal string Latitude()
        {   //get latitude
            float lat = FloatingOriginManager.instance.GetGlobeCoords(boat).z;
            if (SailInfoMain.showCoordinatesConfig.Value == CoordinateType.Decimal)
            {
                return $"Lat: {Math.Round(lat, 2)}°";
            }
            string hemisphere = lat < 0 ? "S" : "N";
            lat = Math.Abs(lat);

            int deg = (int)lat;
            int min = (int)((lat - deg) * 60);

            return $"{deg}° {min}'{hemisphere}";
        }
        internal string Longitude()
        {   //get longitude
            float lon = FloatingOriginManager.instance.GetGlobeCoords(boat).x;
            if (SailInfoMain.showCoordinatesConfig.Value == CoordinateType.Decimal)
            {
                return $"Long: {Math.Round(lon, 2)}°";
            }
            string hemisphere = lon < 0 ? "W" : "E";
            lon = Math.Abs(lon);
            int deg = (int)lon;
            int min = (int)((lon - deg) * 60);

            return $"{deg}° {min}'{hemisphere}";
        }
    }
}
