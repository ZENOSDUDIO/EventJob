using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QCMonitor.OnlineReport.ExcelReport.Util {
    public class QCTimeSpan {
        TimeSpan TimeSpan;
        private int Month;
        
        protected QCTimeSpan(int month, int day, int hour, int minitue, int second) {
            this.Month = month;
            this.TimeSpan = new TimeSpan(day, hour, minitue, second);
        }

        public QCTimeSpan(int month) 
            : this(month, 0, 0, 0, 0) {
        }

        public QCTimeSpan(int day, int hour, int min, int second)
            : this(0, day, hour, min, second) {
        }

        public QCTimeSpan(int hour, int min, int second)
            : this(0, hour, min, second) {
        }


        public DateTime Add(DateTime oldTime) {
            DateTime newTime = oldTime.Add(TimeSpan);
            newTime = newTime.AddMonths(Month);
            return newTime;
        }
    }
}
