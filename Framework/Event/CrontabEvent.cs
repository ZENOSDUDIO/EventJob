using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QCMonitor.OnlineReport.ExcelReport.Util;

namespace QCMonitor.Framework.Event {
    public class CrontabEvent : QCEvent {
        public DateTime ScheduledStartTime;
        public String Key;
    }

    public class CrontabTask<T> where T : CrontabEvent, new() {
        public DateTime NextTime;
        public QCTimeSpan DeltaSpan;
        public String Key;

        static Dictionary<String, CrontabTask<T>> CrontabKeyToTasks = new Dictionary<String, CrontabTask<T>>();

        public CrontabTask(DateTime startTime,
            QCTimeSpan deltaSpan,
            String key) {
            Key = key;
            DeltaSpan = deltaSpan;
            NextTime = startTime;

            if (CrontabKeyToTasks.ContainsKey(key))
                throw new NotImplementedException("Crontab Task Key Duplicate Detected : Key = " + key);

            CrontabKeyToTasks.Add(key, this);

            InitTick();
        }

        protected virtual void InitTick(){

            var lastRanTime = LastRanTime();
            
            while (NextTime < lastRanTime) {
                NextTime = DeltaSpan.Add(NextTime);
            }
        }

        protected virtual DateTime LastRanTime() {
            return DateTime.Now;
        }

        public virtual T Tick() {

            var shoudBeStartAt = NextTime;

            while (NextTime < DateTime.Now) {
                NextTime = DeltaSpan.Add(NextTime);
            }

            var crontabEvent = new T();
            crontabEvent.ScheduledStartTime = shoudBeStartAt;
            crontabEvent.Key = Key;

            return crontabEvent;
        }

    }

    /// <summary>
    /// </summary>
    public class CrontabEventSource<T> : QCEventSource<T> where T : CrontabEvent, new () {
        protected IList<CrontabTask<T>> CrontabTasks = new List<CrontabTask<T>>();
        protected QCLogger Logger;

        public CrontabEventSource(QCLogger logger) {
            Logger = logger;
        }

        public void InsertEvent(CrontabTask<T> task) {
            CrontabTasks.Add(task);
        }

        public override bool HasNextEvent() {
            return GetTodoTasks().Count() > 0;
        }

        protected virtual IEnumerable<CrontabTask<T>> GetTodoTasks() {
            return CrontabTasks.Where(task => task.NextTime < DateTime.Now);
        }

        public override T NextEventImpl() {
            var nextTask = GetTodoTasks().First();
            return nextTask.Tick();
        }
    }
}
