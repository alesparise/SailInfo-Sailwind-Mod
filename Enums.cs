using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SailInfo
{
    public enum SailNameType
    {
        None = 0,
        Standard = 1,
        Historical = 2,
        SimplePositional = 3,
    }
    public enum SpeedUnits
    {
        None = 0,
        Standard = 1,
        NauticalMilesPerHour = 2,
    }
    public enum HeadingType
    {
        None = 0,
        Cardinal = 1,
        Degrees = 2,
    }
    public enum WindSpeedType
    {
        None = 0,
        Beaufort = 1,
        Knots = 2,
    }
    public enum WinchOutType
    {
        None = 0,
        Degrees = 1,
        Percent = 2,
    }
    public enum ColorCoding
    {
        None = 0,
        Plain = 1,
        ColorCoded = 2,
    }
    public enum CoordinateType
    {
        None = 0,
        DegreesAndMinutes = 1,
        Decimal = 2,
    }
    public enum WinchOutTypeLinear
    {
        None = 0,
        Absolute = 1,
        Percent = 2,
    }
}
