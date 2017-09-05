using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Test.Properties;

namespace Test
{
    public class FileForm:ChartForm
    {
        public FileForm(string filePath):base(Path.GetFileNameWithoutExtension(filePath))
        {
            _filePath = filePath;
            _ptrSeries.XValueType = ChartValueType.DateTime;
            _ptrChartArea.CursorX.IntervalType = DateTimeIntervalType.Seconds;
            _ptrSeries.YValueType = ChartValueType.Int32;
            _ptrAxisY.LabelStyle.Format = "#";
            XMinZoom = Config.XMinZoom;
            YMinZoom = Config.YMinZoom;
            ReadFile(_filePath);
        }

        public override void UpdateControls()
        {
            _ptrAxisX.LabelStyle.Format = Settings.Default.ViewTimeChart;
            base.UpdateControls();
        }

        protected override void OnDtpValueChanged(DateTime newTime)
        {
            RefreshData(newTime);
            OnOtherEvent(OtherEventType.DataChanged);
        }

        private readonly string _filePath;
        private List<FilePoint> _pointsList = new List<FilePoint>();
        private SortedSet<KeyValueHolder<double, int>> _xList = new SortedSet<KeyValueHolder<double, int>>();

        public SortedSet<KeyValueHolder<double, int>> GetXList() => _xList;

        public int GetValue(int index)
        {
            return Config.IsMagneticField ? _pointsList[index].MagneticField : _pointsList[index].RmsDeviation;
        }
        public int GetCount() => _pointsList.Count;

        public bool IsValid = false;

        protected override void cbEnable_CheckedChanged(object sender, EventArgs e)
        {
            if (!IsValid) cbEnable.Checked = false;
            base.cbEnable_CheckedChanged(sender,e);
        }

        public void RemovePoints(SortedSet<KeyValueHolder<double, int>> rlist)
        {
            for (int i = _pointsList.Count - 1; i > -1; i--)
            {
                var test = new KeyValueHolder<double, int>(_pointsList[i].GetRoundTime());
                if (rlist.Contains(test)) _pointsList.RemoveAt(i);
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
                IsValid = false;
                MessageBox.Show(ChartName + "\r\n" + ex.Message,
                    "Ошибка открытия файла",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                throw;
            }
            RefreshData();
        }

        public void RefreshData(DateTime? newTime = null)
        {
            IsValid = _pointsList.Count > 1;
            _ptrSeries.Points.Clear();
            _xList.Clear();
            Border = new ChartRect();
            TimeSpan deltaTime = new TimeSpan();
            if (!IsValid)
            {
                IsEnable = false;
                base.UpdateControls();
                return;
            }
            if (newTime != null)
            {
                deltaTime = newTime.Value.Subtract(_pointsList[0].Time);
            }
            for (int i = 0; i < _pointsList.Count; i++)
            {
                var point = _pointsList[i];
                if (newTime != null)
                {
                    point.Time = point.Time.Add(deltaTime);
                }
                var pix = point.GetPixel();
                _ptrSeries.Points.Add(pix);
                _xList.Add(new KeyValueHolder<double, int>(point.GetRoundTime(), i));
                Border.Union(pix.XValue, pix.YValues.First());
            }

            dtpDate.Value = _pointsList[0].Time;
            dtpTime.Value = _pointsList[0].Time;
        }

        public override void OnOtherEvent(object sender, OtherEventType e)
        {
            if (e == OtherEventType.ResetZoom && !this.Equals(sender))
            {
                UpdateAxis(null, false, true);
            }
            if (e == OtherEventType.DataChanged)
            {
                UpdateAxis(null,false,false,true);
            }
        }

        public override void OnScaleViewChanged(object sender, ChartRect e)
        {
           if(!this.Equals(sender)) UpdateAxis(e);
        }

        protected override void ViewChanged(bool isResetZoom = false)
        {
            if(isResetZoom) OnOtherEvent(OtherEventType.ResetZoom);
            else OnScaleViewChanged();
        }
    }
}
