using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QCMonitor.Framework.Framework.DBService.SqlClient;

namespace QCMonitor.Framework.Event {
    /// <summary>
    /// The first column is row_id, the second is create_time
    /// </summary>
    public abstract class AutoExpireStreamingEventSource<T> : StreamingEventSource<T> where T: AutoExpireStreamingEvent {

        public virtual TimeSpan ExpireSpan {
            get {
                // default expire span: 1 hour 0 minitue 0 second
//#if (DEBUG)
//                return new TimeSpan(30, 0, 0, 0);
//#else
//                return new TimeSpan(0, 1, 0, 0);
//#endif
                return new TimeSpan(0, 1, 0, 0);
            }
        }

        public override ColumnDefinition ValueDefs {
            get {
                return base.ValueDefs.AddDefinition(new ColumnDefinition("create_time", "datetime"));
            }
        }

        protected override bool IsValidRow(object[] rs) {
            if (base.IsValidRow(rs)) {
                DateTime createTime = (DateTime)rs[1];
                if (DateTime.Now.Add(ExpireSpan.Negate()) < createTime) {
                    return true;
                }
            } 
            return false;
        }

        protected sealed override T DecorateStreamingEvent(T streamingEvent) {
            return DecorateAutoStreamingEvent(streamingEvent);
        }

        protected virtual T DecorateAutoStreamingEvent(T autoStreamingEvent) {
            return autoStreamingEvent;
        }
    }

    [Serializable]
    public class AutoExpireStreamingEvent : StreamingEvent {
    }
}
