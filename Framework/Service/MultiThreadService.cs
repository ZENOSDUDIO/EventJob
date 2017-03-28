using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using QCMonitor.Framework.Framework.Message.Device.Exceptions;
using QCMonitor.Framework.Framework.Message.Device;

namespace QCMonitor.Framework.Service
{
  

    public class MultiThreadServiceOptions : QCServiceOptions
    {
        public MultiThreadServiceOptions(String Name)
            : base(Name)
        {
        }

        public virtual int MaxWorkerThreads
        {
            get
            {
                return GetInt("MaxWorkerThreads", 10);
            }
            set
            {
                SaveOrUpdateString("MaxWorkerThreads", value + "");
            }
        }

        public virtual int WaitInteval
        {
            get
            {
                return GetInt("WaitInteval", 100);
            }
            set
            {
                SaveOrUpdateString("WaitInteval", value + "");
            }
        }
    }

    public abstract class MultiThreadService : QCService
    {
        protected IList<IQCEventSource> EventSources;
        protected IList<IQCEventJobFactory> EventJobFactories;

        public new QCLogger Logger
        {
            get
            {
                return base.Logger;
            }
        }

        public new MultiThreadServiceOptions Options;
        protected int WaitInteval;
        public Semaphore Semaphore;

        protected bool IsSTAThread;

        protected sealed override QCServiceOptions InitOptions()
        {
            return InitMultiThreadServiceOptions();
        }

        protected abstract MultiThreadServiceOptions InitMultiThreadServiceOptions();

        protected abstract IList<IQCEventSource> InitEventSources();

        protected abstract IList<IQCEventJobFactory> InitEventJobFactories();

        protected override void Init()
        {
            Options = (MultiThreadServiceOptions)base.Options;

            Logger.Info("Starting Main service");

            WaitInteval = Options.WaitInteval;
            Semaphore = new Semaphore(Options.MaxWorkerThreads, Options.MaxWorkerThreads);

            Logger.Info("Main Service Wait Interval: " + WaitInteval);

            try
            {
                EventJobFactories = InitEventJobFactories();
            }
            catch (Exception e)
            {
                Logger.Info("init job factory has many error:" + e.Message);
            }
            EventSources = InitEventSources();

            IsSTAThread = false;

            Logger.Info("Main service inited");
        }

        protected override void Finish()
        {
        }

        protected override QCServiceWorkState Work()
        {
            bool idle = true;
            Logger.Info("Start Main service Work()!");
            foreach (var source in EventSources)
            {
                try
                {
                    if (source.HasNextEvent())
                    {
                        Logger.Info("source.HasNextEvent()");
                        QCEvent e = source.NextEvent();

                        Debug.Assert(e != null, "Event can't be null");

                        bool eventHasNotBeenAccept = true;

                        foreach (IQCEventJobFactory handlerFactory in EventJobFactories)
                        {
                            if (handlerFactory.AcceptEvent(e))
                            {
                                eventHasNotBeenAccept = false;
                                IQCEventJob handler = handlerFactory.NewJob(e);
                                Logger.Info(" Handler: " + handlerFactory.GetType() + " Event: " + source.GetType());

                                if (IsSTAThread)
                                {
                                    Logger.Info("STAThread, do without new thread");
                                    HandlerWork(e, handler, false);


                                }
                                else
                                {
                                    Semaphore.WaitOne(); // need to be the last line

                                    Task.Factory.StartNew(() =>
                                    {
                                        HandlerWork(e, handler, true);
                                    });
                                }
                            }
                        }

                        if (eventHasNotBeenAccept)
                        {
                            Logger.Debug("Event " + e + " has not been accepted by any factory. Skipped");
                        }

                        idle = false;
                    }
                }
                catch (QCEventIOException e)
                {
                    Logger.Debug("Event Fetching Error: " + e);
                }
                catch (SendMessageException smse) {
                   
                    throw smse;
                }
            }

            if (idle) return QCServiceWorkState.Idle;
            else return QCServiceWorkState.Working;
        }

        private void HandlerWork(QCEvent e, IQCEventJob handler, bool inNewThread)
        {
            try
            {
                Logger.Info("Start work,Event is :" + e.ToString() + ",job is :" + handler.ToString() + "");

                handler.Work(inNewThread);
            }

            catch (SendMessageException smse) {
                throw smse;
            }

            catch (Exception ne)
            {
                Logger.Info("The work have some error ,Event is :" + e.ToString() + ",job is :" + handler.ToString() + "");
                Logger.LogException(ne);
                Logger.Error(e.StatusString());

                Logger.Error("System Status Dumping ");
                foreach (var dSource in EventSources)
                {
                    Logger.Error(dSource.StatusString());
                }

                foreach (var dFactory in EventJobFactories)
                {
                    Logger.Error(dFactory.StatusString());
                }
                Logger.Error("System Status Dump End");

                if (inNewThread)
                {
                    // remeber to release the semaphore
                    Semaphore.Release();
                }
                throw ne;
            }
        }
    }
}
