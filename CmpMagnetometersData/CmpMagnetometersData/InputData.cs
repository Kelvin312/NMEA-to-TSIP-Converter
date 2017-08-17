using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace CmpMagnetometersData
{
    public static class Config
    {
        public static bool IsMagneticField = true;
        public static Color NormalColor = Color.DarkGreen;
        public static Color WarningColor = Color.DarkOrange;
        public static Color ErrorColor = Color.Red;
    }

    public class InDataPoint
    {
        public DateTime Time;
        public int MagneticField;
        public ushort RmsDeviation;
        public byte StateCode;

        public DataPoint GetPixel()
        {
            DataPoint result = new DataPoint();
            result.SetValueXY(Time, Config.IsMagneticField ? MagneticField : RmsDeviation);
            result.Color = Config.ErrorColor;
            if ((StateCode & 0x80) != 0) result.Color = Config.WarningColor;
            if (StateCode == 0x80) result.Color = Config.NormalColor;
            return result;
        }

        public InDataPoint(string text)
        {
            TextParse(text);
        }
        public void TextParse(string txt)
        {
                //54641714 +- 00076 [82] 01-20-97 23:01:36.00
                var args = txt.Split(new [] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                MagneticField = int.Parse(args[0]);
                RmsDeviation = ushort.Parse(args[2]);
                StateCode = (byte)uint.Parse(args[3].Trim('[', ']'));
                CultureInfo provider = CultureInfo.InvariantCulture;
                Time = DateTime.ParseExact(args[4] + ' ' + args[5], "MM-dd-yy HH:mm:ss.ff", provider);
        }
    }

    public class InputData
    {
        private List<InDataPoint> dataPoints = new List<InDataPoint>();
        public DateTime StartFileTime;
        public string FileName;

        public void Read(string filePath)
        {
            FileName = "";
            dataPoints.Clear();
            int pointIndex = 0;
            using (var objReader = new StreamReader(filePath, Encoding.Default))
            {
                foreach (var s in objReader.ReadToEnd().Split(new char[] {'\0'}, 
                    StringSplitOptions.RemoveEmptyEntries))
                {
                    if (++pointIndex > 2)
                    {
                        try
                        {
                            var temp = new InDataPoint(s);
                            dataPoints.Add(temp);
                        }
                        catch (Exception)
                        {
                            // throw;
                        }

                    }
                }
            }
            if (dataPoints.Count > 0)
            {
                StartFileTime = dataPoints[0].Time;
                FileName = Path.GetFileNameWithoutExtension(filePath);
            }
        }

        public void UpdateTime(DateTime time)
        {
            
        }

        public void Save(string filePath)
        {
            
        }
    }
}
