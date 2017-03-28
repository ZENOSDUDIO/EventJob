using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QCMonitor.OnlineReport.ExcelReport.Util;
using QCMonitor.Crontab.Domain;
using QCMonitor.Crontab.Repository;
using System.Diagnostics;

namespace QCMonitor.Framework.Event {
    public class DBCrontabEvent : CrontabEvent {
        public DateTime LastRanTime;
    }

    public class DBCrontabTask : CrontabTask<DBCrontabEvent> {
        DBCrontabEntry CrontabEntry;

        public DBCrontabTask(DateTime startTime,
            QCTimeSpan deltaSpan,
            String key) : base(startTime, deltaSpan, key){
        }

        protected override void InitTick() {
            
            var crontabEntryRepository = new DBCrontabEntryRepository();
            CrontabEntry = crontabEntryRepository.LoadCrontabByKey(Key);

            if (CrontabEntry == null) {
                CrontabEntry = new DBCrontabEntry(Key);
                crontabEntryRepository.SaveOrUpdate(CrontabEntry);
            }

            base.InitTick();
        }

        protected override DateTime LastRanTime() {
            return CrontabEntry.LastRunTime;
        }

        public override DBCrontabEvent Tick() {
            var crontabEvent = base.Tick();
            crontabEvent.LastRanTime = CrontabEntry.LastRunTime;

            CrontabEntry.LastRunTime = DateTime.Now;
            var crontabEntryRepository = new DBCrontabEntryRepository();
            crontabEntryRepository.SaveOrUpdate(CrontabEntry);

            return crontabEvent;
        }
    }

    public class DBCrontabEventSource : QCEventSource<QCEvent> {
        protected IList<DBCrontabTask> CrontabTasks = new List<DBCrontabTask>();
        protected QCLogger Logger;


        public DBCrontabEventSource(QCLogger logger, ICollection<DBCrontabTask> crontabTasks) {
            Logger = logger;
            CrontabTasks = crontabTasks.ToList();
        }
        
        public override bool HasNextEvent() {
            return GetTodoTasks().Count() > 0;
        }

        protected virtual IEnumerable<DBCrontabTask> GetTodoTasks() {
            return CrontabTasks.Where(task => task.NextTime < DateTime.Now);
        }

        public override QCEvent NextEventImpl() {
            var nextTask = GetTodoTasks().First();
            return nextTask.Tick();
        }
    }
}
