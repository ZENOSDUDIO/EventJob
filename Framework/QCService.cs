using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using QCMonitor.Framework.Framework.DBService.SqlClient;
using QCMonitor.Framework.Framework.Message.Device.Exceptions;

namespace QCMonitor.Framework
{
    public delegate void HandleSMSMessage(string phonenumb,string content,string trackingid);

    public class QCServiceTerminalException : Exception
    {
    }


    public enum QCServiceWorkState
    {
        Idle, // it worked on something
        Working,
    };

    [InitTable]
    public class QCServiceOptions : QCOptions
    {

        public QCServiceOptions(string Name)
            : base(Name)
        {
            // TODO: Complete member initialization
        }
        public virtual int IdleInteval
        {
            get
            {
                return GetInt("IdleInteval", 1000);
            }
        }
    }


    public static class ServiceCommand
    {
        public const string STOP = "stop";
        public const string RESUME = "resume";
        public const string SUSPEND = "suspend";
    }

    enum ServiceStatus
    {
        RUNNING,
        PENDING,
        ZOMBIE
    }

    /// <summary>
    /// This is the time to start QC 1.0.
    /// </summary>
    public abstract class QCService
    {
        public abstract string Name
        {
            get;
        }
        private HandleSMSMessage smsHandle;

        public HandleSMSMessage SMSHandle
        {
            get
            {
                return this.smsHandle;
            }
            private set
            {
                this.smsHandle = value;
            }


        }
        private ServiceStatus Status;

        protected QCLogger Logger;

        public QCServiceOptions Options;

        protected abstract void Init();
        protected abstract QCServiceWorkState Work();
        protected abstract void Finish();

        protected Queue CommandQueue;

        public void InsertCommand(string command)
        {
            if (CommandQueue == null)
                Logger.Debug("Command Queue have not initialized!");
            else
                CommandQueue.Enqueue(command);
        }

        public void MainLoop(string[] args)
        {
            // init stage
            Logger = new QCLogger(Name);
            Options = InitOptions();
            CommandQueue = Queue.Synchronized(new Queue());

            Logger.Info("QCService Init Start");

            try
            {
                Init();

            }
            catch (Exception ie)
            {
                Logger.LogException(ie);
                Logger.Error("QCService Init Failed. Service is shutting down error：" + ie.Message);
                throw (ie);
            }

            Status = ServiceStatus.RUNNING;

            // init vars
            int idleInterval = Options.IdleInteval; //Options.GetInt("IdleInterval");
            int lastIdleTime = DateTime.Now.Millisecond;
            int checkInterval = 1000;

            Logger.Info("QCService Init End ");

            Logger.Info("QCService Start Main Loop");

            Int64 loopTimes = 0;
            // wokring loop
            try
            {
                while (true)
                {
                    CheckCommandQueue();

                    if (Status == ServiceStatus.PENDING)
                    {
                        if (loopTimes++ % checkInterval == 0)
                        {
                            Logger.Info("Service is Paused. Everything is fine :-)");
                        }
                        Thread.Sleep(idleInterval);
                    }
                    else if (Status == ServiceStatus.ZOMBIE)
                    {
                        Logger.Info("Service is stopping. Good bye. :-)");
                        break;
                    }
                    else if (Status == ServiceStatus.RUNNING)
                    {
                        QCServiceWorkState result = QCServiceWorkState.Idle;

                        try
                        {
                            result = Work();
                        }


                        catch (Exception e)
                        {
                            Logger.LogException(e);
                        }

                        if (result == QCServiceWorkState.Idle)
                        {
                            int now = DateTime.Now.Millisecond;

                            if (now - lastIdleTime < idleInterval)
                            {
                                Thread.Sleep(idleInterval);
                            }

                            lastIdleTime = now;

                            if (loopTimes++ % checkInterval == 0)
                            {
                                Logger.Info("Service is Idle. Everything is fine :-)");
                            }
                        }
                    }
                    else Debug.Assert(false);
                }
            }
            catch (QCServiceTerminalException e)
            {
                Logger.Debug(e.Message);
            }

            // end stage
            Logger.Info("QCService Finishing");
            Finish();
            Logger.Info("QCService Finished");
        }

        public void MainLoop(HandleSMSMessage hanleSMSException)
        {

            SMSHandle = hanleSMSException;

            // init stage
            Logger = new QCLogger(Name);
            Options = InitOptions();
            CommandQueue = Queue.Synchronized(new Queue());

            Logger.Info("QCService Init Start");

            try
            {
                Init();

            }
            catch (Exception ie)
            {
                Logger.LogException(ie);
                Logger.Error("QCService Init Failed. Service is shutting down error：" + ie.Message);
                throw (ie);
            }

            Status = ServiceStatus.RUNNING;

            // init vars
            int idleInterval = Options.IdleInteval; //Options.GetInt("IdleInterval");
            int lastIdleTime = DateTime.Now.Millisecond;
            int checkInterval = 10000;

            Logger.Info("QCService Init End ");

            Logger.Info("QCService Start Main Loop");

            Int64 loopTimes = 0;
            // wokring loop
            try
            {
                int i = 0;

                while (true)
                {
                    CheckCommandQueue();

                    if (Status == ServiceStatus.PENDING)
                    {
                        if (loopTimes++ % checkInterval == 0)
                        {
                            Logger.Info("Service is Paused. Everything is fine :-)");
                        }
                        Thread.Sleep(idleInterval);
                    }
                    else if (Status == ServiceStatus.ZOMBIE)
                    {
                        Logger.Info("Service is stopping. Good bye. :-)");
                        break;
                    }
                    else if (Status == ServiceStatus.RUNNING)
                    {
                        QCServiceWorkState result = QCServiceWorkState.Idle;

                        try
                        {
                            result = Work();
                        }
                        catch (Exception e)
                        {
                            Logger.LogException(e);
                        }

                        if (result == QCServiceWorkState.Idle)
                        {
                            int now = DateTime.Now.Millisecond;

                            if (now - lastIdleTime < idleInterval)
                            {
                                Thread.Sleep(idleInterval);
                            }

                            lastIdleTime = now;

                            if (loopTimes++ % checkInterval == 0)
                            {
                                Logger.Info("Service is Idle. Everything is fine :-)");
                            }
                        }
                    }
                    else Debug.Assert(false);
                }
            }
            catch (QCServiceTerminalException e)
            {
                Logger.Debug(e.Message);
            }

            // end stage
            Logger.Info("QCService Finishing");
            Finish();
            Logger.Info("QCService Finished");
        }

        protected virtual QCServiceOptions InitOptions()
        {
            return new QCServiceOptions(Name);
        }

        private void CheckCommandQueue()
        {
            if (CommandQueue.Count != 0)
            {

                string command = CommandQueue.Dequeue() as string;

                if (command == ServiceCommand.STOP)
                {
                    Status = ServiceStatus.ZOMBIE;

                    Logger.Info("Service is going to be stopped");
                }
                else if (command == ServiceCommand.SUSPEND)
                {
                    Status = ServiceStatus.PENDING;

                    Logger.Info("Service is going to be paused");
                }
                else if (command == ServiceCommand.RESUME)
                {
                    Status = ServiceStatus.RUNNING;

                    Logger.Info("Service is going to be resumed");
                }
                else
                {
                    Logger.Error("Error Service Command Type");
                    //Debug.Assert(false);
                }
            }
        }

    }

}
