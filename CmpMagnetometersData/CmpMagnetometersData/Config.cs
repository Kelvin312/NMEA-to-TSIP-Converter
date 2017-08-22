using System.Drawing;


namespace CmpMagnetometersData
{
    public static class Config
    {
        public static bool IsMagneticField = true;
        public static Color NormalColor = Color.DarkGreen;
        public static Color WarningColor = Color.DarkOrange;
        public static Color ErrorColor = Color.Red;
        public static double ZoomSpeed = 0.25;

        public static ChartRect GlobalRect = new ChartRect(0, 1000, 0, 1000);
    }
}
