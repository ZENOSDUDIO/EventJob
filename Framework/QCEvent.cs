using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using System.IO;
using QCMonitor.Framework.Util;
using QCMonitor.Framework.Framework.DBService.SqlClient;

namespace QCMonitor.Framework {
    public class QCEventIOException : Exception {
        public QCEventIOException(string errorMsg) : base(errorMsg) {
        }
    }

    public interface IQCObject {
        String StatusString();
    }

    public interface IQCEventSource : IQCObject {
        bool HasNextEvent();
        QCEvent NextEvent();
    }

    public abstract class QCEventSource<T> : IQCEventSource where T : QCEvent {
        public abstract bool HasNextEvent();

        public QCEvent NextEvent() {
            var newEvent = NextEventImpl();

            newEvent = DecorateEvent(newEvent);

            return newEvent;
        }

        public abstract T NextEventImpl();

        protected virtual T DecorateEvent(T newEvent) {
            return newEvent;
        }

        public virtual string StatusString() {
            return "EventSource: " + this.GetType();
        }
    }

    [Serializable]
    public abstract class QCEvent : IQCObject {

        public virtual String StatusString() {
            return "QCEvent Debug String: Type = " + this.GetType();
        }

        public String ToSoapString() {
            string strObject = SoapFormatterHelper.ObjectToSoapString(this);
            return strObject;
        }

        public static QCEvent FromSoapString(string soapString) {
            Object objectFromSoap = SoapFormatterHelper.SoapStringToObject(soapString);
            return (QCEvent)objectFromSoap;
        }
    }

}
