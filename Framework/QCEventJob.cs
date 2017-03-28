using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using QCMonitor.Framework.Service;

namespace QCMonitor.Framework {
    public interface IQCEventJobFactory : IQCObject {
        bool AcceptEvent(QCEvent e);
        IQCEventJob NewJob(QCEvent e);
    }

    public interface IQCEventJob {
        void Work(bool inNewThread);
    }

    public abstract class QCEventJobFactory<T> : IQCEventJobFactory where T : QCEvent {
        protected QCLogger Logger;

        public QCEventJobFactory(QCLogger logger) {
            this.Logger = logger;
        }
        public bool AcceptEvent(QCEvent e) {
            if (e is T) {
                return AcceptEventImpl(e as T);
            }
            return false;
        }

        public abstract bool AcceptEventImpl(T e);

        public IQCEventJob NewJob(QCEvent e) {
            return NewJobImpl(e as T);
        }

        public abstract QCEventJob<T> NewJobImpl(T e);

        public virtual String StatusString() {
            return "JobFactory: " + this.GetType();
        }
    }

    public abstract class QCEventJob<T> : IQCEventJob where T : QCEvent {
        protected T Event { get; private set; }

        public QCEventJob(T e) {
            this.Event = e;
        }

        public abstract void Work(bool inNewThread);
    }

    public abstract class QCParallelEventJobFactory<T> : QCEventJobFactory<T> where T : QCEvent {
        
        private MultiThreadService Service;

        protected QCParallelEventJobFactory(MultiThreadService service)
            : base(service.Logger) {
            Service = service;
        }
        
        public override QCEventJob<T> NewJobImpl(T e) {
            var parallelJob = NewParallelJobImpl(e);
            // init the semaphore
            parallelJob.Semaphore = Service.Semaphore;
            return parallelJob;
        }

        public abstract QCParallelEventJob<T> NewParallelJobImpl(T e);
    }


    public abstract class QCParallelEventJob<T> : QCEventJob<T> where T : QCEvent {
        public Semaphore Semaphore;

        public QCParallelEventJob(T e)
            : base(e) {
        }

        public sealed override void Work(bool inNewThread) {
            WorkStub();
            if (inNewThread) {
                Semaphore.Release();
            }
        }

        protected abstract void WorkStub();

        protected void Debug(Object o) {
            System.Diagnostics.Debug.Print(DateTime.Now.ToShortTimeString() + @" [" + Thread.CurrentThread + "]" + ": " + o.ToString());
        }
    }

    /// <summary>
    /// Helper class for empty job.
    /// </summary>
    public sealed class QCEmptyEventJob<T> : QCParallelEventJob<T> where T: QCEvent {
        public QCEmptyEventJob(T e)
            : base(e) {
        }

        protected override void WorkStub()
        {
        }
    }
}
