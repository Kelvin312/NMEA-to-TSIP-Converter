using System.Drawing;


namespace CmpMagnetometersData
{
    public static class Config
    {
        public static bool IsMagneticField = true;
        
        public static double XMinZoom = 1.0 / 24 / 60 / 60;
        public static double YMinZoom = 1.0;

        public static ChartRect GlobalBorder = new ChartRect();
    }
}
