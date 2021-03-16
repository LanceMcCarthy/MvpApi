using System.Collections.Generic;
using System.Globalization;
using Windows.UI;

namespace MvpCompanion.UI.Common.Helpers
{
    public static class PaletteHelper
    {
        // http://www.colorbox.io/#steps=21#hue_start=0#hue_end=359#hue_curve=easeInSine#sat_start=59#sat_end=60#sat_curve=easeOutQuad#sat_rate=48#lum_start=87#lum_end=59#lum_curve=easeOutQuad#lock_hex=
        // Contrast ratio too low
        public static List<Color> ContributionTypeBackgroundColors = new List<Color>
        {
            ConvertToColor("#E6B3B3"),
            ConvertToColor("#DDA09F"),
            ConvertToColor("#DDA29E"),
            ConvertToColor("#DCA79D"),
            ConvertToColor("#DBAD9C"),
            ConvertToColor("#D9B69B"),
            ConvertToColor("#D8C09A"),
            ConvertToColor("#D6CC99"),
            ConvertToColor("#CDD397"),
            ConvertToColor("#B8D195"),
            ConvertToColor("#A2CE93"),
            ConvertToColor("#90CA96"),
            ConvertToColor("#8DC6AA"),
            ConvertToColor("#8AC1BE"),
            ConvertToColor("#86A8BC"),
            ConvertToColor("#828BB7"),
            ConvertToColor("#C1B9D4"),
            ConvertToColor("#C4A8C7"),
            ConvertToColor("#B48DB1"),
            ConvertToColor("#B5929F"),
            ConvertToColor("#B59797")
        };

        public static List<Color> ContributionTypeForegroundColors = new List<Color>
        {
            ConvertToColor("#561A1A"),
            ConvertToColor("#3E1414"),
            ConvertToColor("#361511"),
            ConvertToColor("#331B14"),
            ConvertToColor("#3E2119"),
            ConvertToColor("#3A2617"),
            ConvertToColor("#392D18"),
            ConvertToColor("#332D14"),
            ConvertToColor("#383918"),
            ConvertToColor("#2A3418"),
            ConvertToColor("#1D3116"),
            ConvertToColor("#152E17"),
            ConvertToColor("#192E26"),
            ConvertToColor("#192E2C"),
            ConvertToColor("#19252E"),
            ConvertToColor("#0A0E14"),
            ConvertToColor("#2E2E2E"),
            ConvertToColor("#242424"),
            ConvertToColor("#000000"),
            ConvertToColor("#000000"),
            ConvertToColor("#000000")
        };

        public static Color ConvertToColor(string colorHexString)
        {
            if (colorHexString.Contains("#"))
            {
                colorHexString = colorHexString.TrimStart('#');
            }

            Color color;

            if (colorHexString.Length == 6)
            {
                // No alpha available
                color = Color.FromArgb(255,
                    (byte)int.Parse(colorHexString.Substring(0, 2), NumberStyles.HexNumber),
                    (byte)int.Parse(colorHexString.Substring(2, 2), NumberStyles.HexNumber),
                    (byte)int.Parse(colorHexString.Substring(4, 2), NumberStyles.HexNumber));
            }
            else
            {
                // Alpha included
                color = Color.FromArgb(
                    (byte)int.Parse(colorHexString.Substring(0, 2), NumberStyles.HexNumber),
                    (byte)int.Parse(colorHexString.Substring(2, 2), NumberStyles.HexNumber),
                    (byte)int.Parse(colorHexString.Substring(4, 2), NumberStyles.HexNumber),
                    (byte)int.Parse(colorHexString.Substring(6, 2), NumberStyles.HexNumber));
            }

            return color;
        }
    }
}
