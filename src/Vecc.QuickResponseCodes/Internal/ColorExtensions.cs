using System.Drawing;

namespace Vecc.QuickResponseCodes.Internal
{
    public static class ColorExtensions
    {
        public static Color ToColor(this Abstractions.Color color)
        {
            return Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
        }
    }
}
