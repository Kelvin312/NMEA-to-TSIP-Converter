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
   

   
    public class InputData
    {
        private List<FileDataPoint> dataPoints = new List<FileDataPoint>();
        public DateTime StartFileTime;
        public string FileName;

        public IEnumerable<DataPoint> GetPoints()
        {
            return dataPoints.Select(point => point.GetPixel());
        }

        public void Read(string filePath)
        {
            using (var objReader = new StreamReader(filePath, Encoding.Default))
            {
                FileName = "";
                dataPoints.Clear();
                int pointIndex = 0;
                foreach (var s in objReader.ReadToEnd().Split(new char[] {'\0'}, 
                    StringSplitOptions.RemoveEmptyEntries))
                {
                    if (++pointIndex > 2)
                    {
                        try
                        {
                            var temp = new FileDataPoint(s);
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
            DateTime deltaTime = time;
            deltaTime.Subtract(StartFileTime);

        }

        public void Save(string filePath)
        {
            
        }
    }
}
