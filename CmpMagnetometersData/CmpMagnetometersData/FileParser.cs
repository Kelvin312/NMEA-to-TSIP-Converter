using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using CmpMagnetometersData.Properties;

namespace CmpMagnetometersData
{
    public class FileParser
    {
        public DataLineControl DataControl = null;
        public DataLineControl DeviationControl = null;
        private List<FilePoint> _pointsList = new List<FilePoint>();
        public FileParser(string filePath, int loadIndex = -1)
        {
            ReadFile(filePath);

            if (_pointsList.Count < 2) return;
             
            if (loadIndex == -1 || loadIndex == 0)
            {
                DataControl = new DataLineControl
                {
                    ReloadPath = filePath,
                    ReloadIndex = 0,
                    LineName = Path.GetFileName(filePath)
                };
                DataControl.DataPixels.Clear();
                foreach (var p in _pointsList)
                {
                    DataControl.DataPixels.Add(new DataPixel() {Color = p.StateColor, Time = p.Time, Val = p.MagneticField});
                }
            }
            if (loadIndex == -1 || loadIndex == 1)
            {
                DeviationControl = new DataLineControl
                {
                    ReloadPath = filePath,
                    ReloadIndex = 1,
                    LineName = Path.GetFileName(filePath) + " СКО"
                };
                DeviationControl.DataPixels.Clear();
                foreach (var p in _pointsList)
                {
                    DeviationControl.DataPixels.Add(new DataPixel() { Color = p.StateColor, Time = p.Time, Val = p.RmsDeviation });
                }
            }
        }

        private void ReadFile(string filePath)
        {
            _pointsList.Clear();
            
            var pointIndex = -3;
            try
            {
                using (var sr = new StreamReader(filePath))
                {
                    foreach (var str in sr.ReadToEnd()
                        .Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (++pointIndex < 0) continue;
                        try
                        {
                            var p = new FilePoint(str);
                            _pointsList.Add(p);
                        }
                        catch (Exception)
                        {
                            // throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(filePath + "\r\n" + ex.Message,
                    "Ошибка открытия файла",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                throw;
            }
        }
    }


    public class FilePoint
    {
        public DateTime Time;
        public int MagneticField;
        public int RmsDeviation;
        public Color StateColor;

        public FilePoint(string text)
        {
            TextParse(text);
        }
       
        public void TextParse(string txt)
        {
            //54641714 +- 00076 [82] 01-20-97 23:01:36.00
            var args = txt.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            MagneticField = int.Parse(args[0]);
            RmsDeviation = ushort.Parse(args[2]);
            var code = Convert.ToByte(args[3].Trim('[', ']'), 16);
            StateColor = Settings.Default.ErrorColor;
            if ((code & 0x80) != 0) StateColor = Settings.Default.WarningColor;
            if (code == 0x80) StateColor = Settings.Default.NormalColor;
            var provider = CultureInfo.InvariantCulture;
            Time = DateTime.ParseExact(args[4] + ' ' + args[5], "MM-dd-yy HH:mm:ss.ff", provider);
        }
    }
}
