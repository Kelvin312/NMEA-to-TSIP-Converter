using System;
using System.Globalization;
using System.Windows.Forms.DataVisualization.Charting;

namespace CmpMagnetometersData
{
    public class FilePoint
    {
        public DateTime Time;
        public int MagneticField;
        public ushort RmsDeviation;
        public byte StateCode;

        public FilePoint(string text)
        {
            TextParse(text);
        }
        public DataPoint GetPixel()
        {
            DataPoint result = new DataPoint();
            result.SetValueXY(Time.ToOADate(), Config.IsMagneticField ? MagneticField : RmsDeviation);
            result.Color = Config.ErrorColor;
            if ((StateCode & 0x80) != 0) result.Color = Config.WarningColor;
            if (StateCode == 0x80) result.Color = Config.NormalColor;
            return result;
        }
        public void TextParse(string txt)
        {
            //54641714 +- 00076 [82] 01-20-97 23:01:36.00
            var args = txt.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            MagneticField = int.Parse(args[0]);
            RmsDeviation = ushort.Parse(args[2]);
            StateCode = Convert.ToByte(args[3].Trim('[', ']'), 16);
            CultureInfo provider = CultureInfo.InvariantCulture;
            Time = DateTime.ParseExact(args[4] + ' ' + args[5], "MM-dd-yy HH:mm:ss.ff", provider);
        }
    }
}
