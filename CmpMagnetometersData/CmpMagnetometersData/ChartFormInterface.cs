using System;
using System.Collections.Generic;

namespace CmpMagnetometersData
{
    public interface IChartForm
    {
        void ChartControl_MouseWheel(int delta);
         ChartRect Border { get;}
         bool IsMinimize { get; }
         bool IsEnable { get; }
         bool IsValid { get; }
         string FileName { get; }

        int ChartType { get; }

        event EventHandler<ChartRect> ScaleViewChanged;
         event EventHandler<bool> OtherEvent;
         event EventHandler<EventArgs> CreateChart;

         SortedSet<KeyValueHolder<double, int>> GetXList();
        int GetValue(int index);
        int GetCount();
         void RemovePoints(SortedSet<KeyValueHolder<double, int>> rlist);
        void SetTimeView();
        void RefreshData(DateTime? newTime = null);

        void UpdateAxis(ChartRect newView = null, bool isUpdateY = false, bool isResetZoom = false,
            bool isUpdateBorder = false);

    }
}