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
        public static double XMinZoom = 1.0 / 24 / 60 / 60;
        public static double YMinZoom = 1.0;
        public static ChartRect GlobalRect = new ChartRect(double.NaN, double.NaN, double.NaN, double.NaN);
        public static string ViewTimeFormat = "yy.MM.dd-HH:mm:ss";
    }
}
