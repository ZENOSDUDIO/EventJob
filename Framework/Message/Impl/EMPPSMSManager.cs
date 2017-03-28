//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using EMPPLib;
//using System.Diagnostics;
//using System.Xml.Linq;
//using System.IO;
//using System.Text.RegularExpressions;
//using System.Configuration;
//using QCMonitor.Framework.Framework.Message.Device;
//using QCMonitor.Framework.Framework.Message.Device.Exceptions;
//using QCMonitor.Framework.Framework.Message.Device.Listener;
//using QCMonitor.Framework;
//using QCMonitor.Framework.Util;
//using System.Threading;

//namespace QCMonitor.Framework.Framework.Message.Impl
//{
//    public class EMPPSMSLogger
//    {
//        public static readonly QCLogger Logger = new QCLogger("EMPPSMSManager");
//    }

//    public class EMPPSMSManager 
//    {
//        private Semaphore emppSemaphore;
//        private System.Threading.AutoResetEvent arse = new System.Threading.AutoResetEvent(true);

//        const int interval = 10000;
//        public static readonly QCLogger Logger = EMPPSMSLogger.Logger;

//        private List<string> listMsg = new List<string>();
//        private bool currConnectStatus;

//        private string Phone { get; set; }
//        private string[] Phones { get; set; }

//        public EMPPSMSManager(Semaphore smsSemaphore)
//        {
//            this.emppSemaphore = smsSemaphore;
//            this.empp = new EMPPLib.emptcl();
//            AddEventHandle(empp);
//        }

//        private EMPPOptions EMPPOptions = new EMPPOptions();
//        private EMPPLib.emptcl empp;

//        private ISMSListener Listener;

//        public emptcl Empp
//        {
//            get
//            {
//                if (empp == null)
//                {
//                    empp = new emptcl();
//                    AddEventHandle(empp);
//                    empp.needStatus = true;
//                }
//                return empp;
//            }
//        }

//        //public void SendMessage(string phoneNo, string content, string trackingId)
//        //{
//        //    InitMsgTask(content);
//        //    Phone = phoneNo;

//        //    EMPPLib.ShortMessage smsmsg = new EMPPLib.ShortMessage();
//        //    EMPPLib.Mobiles dest = new EMPPLib.Mobiles();

//        //    dest.Add(Phone);

//        //    smsmsg.srcID = EMPPOptions.Account;
//        //    smsmsg.ServiceID = EMPPOptions.ServiceId;
//        //    smsmsg.needStatus = true;
//        //    smsmsg.SendNow = true;
//        //    smsmsg.DestMobiles = dest;

//        //    for (int i = 0; i < this.listMsg.Count; i++)
//        //    {
//        //        smsmsg.content = listMsg[i];
//        //        smsmsg.SequenceID = i;
//        //        Send(smsmsg, i);
//        //    }

//        //    arse.WaitOne(interval);
//        //}

//        public bool SendMessage(string phoneNo, string content, string trackingId)
//        {
//            bool result = true;
//            InitMsgTask(content);
//            Phone = phoneNo;

//            EMPPLib.ShortMessage smsmsg = new EMPPLib.ShortMessage();
//            EMPPLib.Mobiles dest = new EMPPLib.Mobiles();

//            dest.Add(Phone);

//            smsmsg.srcID = EMPPOptions.Account;
//            smsmsg.ServiceID = EMPPOptions.ServiceId;
//            smsmsg.needStatus = true;
//            smsmsg.SendNow = true;
//            smsmsg.DestMobiles = dest;

//            for (int i = 0; i < this.listMsg.Count; i++)
//            {
//                smsmsg.content = listMsg[i];
//                smsmsg.SequenceID = i;
//                if (!Send(smsmsg, i))
//                {
//                    result = false;
//                }
//            }
//            arse.WaitOne(interval);
//            this.emppSemaphore.Release();

//            return result;
//        }

//        public void SendMessage(string[] phoneNums, string content)
//        {
//            InitMsgTask(content);
//            Phones = phoneNums;

//            EMPPLib.ShortMessage smsmsg = new EMPPLib.ShortMessage();
//            EMPPLib.Mobiles dest = new EMPPLib.Mobiles();
//            foreach (var phonenumb in Phones)
//            {
//                dest.Add(phonenumb);
//            }
//            smsmsg.srcID = EMPPOptions.Account;
//            smsmsg.ServiceID = EMPPOptions.ServiceId;
//            smsmsg.needStatus = true;
//            smsmsg.SendNow = true;
//            smsmsg.DestMobiles = dest;

//            for (int i = 0; i < this.listMsg.Count; i++)
//            {
//                smsmsg.content = listMsg[i];
//                smsmsg.SequenceID = i;
//                Send(smsmsg, i);
//            }
//            arse.WaitOne(interval);
//        }

//        public void StopDevice()
//        {
//            Logger.Info("Stopping device");
//            //if (empp != null)
//            //{
//            //    empp.disconnect();
//            //}
//            //this.empp = null;
//            Logger.Info("Device stopped");
//        }

//        private void AddEventHandle(emptcl empp)
//        {
//            empp.EMPPClosed += (new _IemptclEvents_EMPPClosedEventHandler(EMPPClosed));
//            empp.EMPPConnected += (new _IemptclEvents_EMPPConnectedEventHandler(EMPPConnected));
//            empp.MessageReceivedInterface += (new _IemptclEvents_MessageReceivedInterfaceEventHandler(MessageReceivedInterface));
//            empp.SocketClosed += (new _IemptclEvents_SocketClosedEventHandler(SocketClosed));
//            empp.StatusReceivedInterface += (new _IemptclEvents_StatusReceivedInterfaceEventHandler(StatusReceivedInterface));
//            empp.SubmitRespInterface += (new _IemptclEvents_SubmitRespInterfaceEventHandler(SubmitRespInterface));

//        }

//        //接收到服务器发送的terminate后，此事件触发
//        public void EMPPClosed(int errorCode)
//        {
//            Logger.Info(" EMPP service  EMPPClosed ! at " + DateTime.Now);
//        }

//        //网络连接断开时触发此事件，可能是网络连接不正常，如断网，服务器连接断开等
//        //接收到此事件表明网络连接已经断开，需要用户重新登陆
//        public void SocketClosed(int errorCode)
//        {
//            Logger.Info(" EMPP service  SocketClosed ! at " + DateTime.Now);
//        }

//        //在接收到短信且不是状态报告时会调用此事件从中可以调用ISMDeliverdPtr  的相关方法，
//        //取得发送者手机号，发送时间，和短信内容等信息 
//        public void MessageReceivedInterface(SMDeliverd sm)
//        {
//            //Listener.ReceiveMessage(sm.srcID, sm.content, sm.submitDatetime);
//            Logger.Info("Recieve a SMS  from " + sm.srcID);
//        }

//        //发送消息后服务器返回发送状态
//        public void SubmitRespInterface(SubmitResp resp)
//        {
//            Logger.Info("SubmitRespInterface" + resp.Result);
//            arse.Set();
//        }

//        //在接收到状态报告时会调用此事件
//        public void StatusReceivedInterface(StatusReport sm)
//        {
//            Logger.Info("StatusReceivedInterface" + sm.Status);
//        }
//        //组件每175秒检查一次连接状况，连接正常将触发此事件
//        public void EMPPConnected()
//        {
//            Logger.Info("EMPP connected " + DateTime.Now);
//        }

//        private Boolean Connection()
//        {
//            EMPPLib.ConnectResultEnum result = ConnectResultEnum.CONNECT_OTHER_ERROR;
//            try
//            {
//                result = Empp.connect(EMPPOptions.HostIP, EMPPOptions.HostPort, EMPPOptions.Account, EMPPOptions.Password);
//            }
//            catch (Exception ex)
//            {
//                //连接失败  
//                result = ConnectResultEnum.CONNECT_OTHER_ERROR;
//                Logger.Error("Can't connect to server, exception: " + ex.Message);
//                this.emppSemaphore.Release();
//            }

//            int retryCount = 0;
//            while (result != EMPPLib.ConnectResultEnum.CONNECT_OK && result != EMPPLib.ConnectResultEnum.CONNECT_KICKLAST)
//            {
//                //首次连接失败，接下来进行重连 
//                retryCount++;
//                try
//                {
//                    Logger.Info("retry to build connection" + retryCount);
//                    result = Empp.connect(EMPPOptions.HostIP, EMPPOptions.HostPort, EMPPOptions.Account, EMPPOptions.Password);
//                    Logger.Info("retry to build connection result" + result);
//                }
//                catch (Exception ex)
//                {
//                    Logger.Info(ex.Message);
//                    result = ConnectResultEnum.CONNECT_OTHER_ERROR;
//                    this.emppSemaphore.Release();
//                }
//                if (retryCount == 20)
//                {
//                    Logger.Error("We still have not connected to EMPP server.SMS message sending was failed");
//                    currConnectStatus = false;
//                    return false;
//                }
//            }
//            currConnectStatus = true;
//            Logger.Info("Connected successfully");
//            return currConnectStatus;
//        }

//        /************************************************************************/
//        /* 按60个字的长度发送分割消息并发送（实际的限制应该是70个左右的汉字长度*/
//        /* 考虑到消息中要附带一些公司信息，暂定60                              */
//        /************************************************************************/
//        private Boolean Send(EMPPLib.ShortMessage msg, int taskIndex)
//        {
//            bool result;

//            if (Empp.connected == true)
//            {
//                result = arse.WaitOne(interval);
//                if (!result & taskIndex >= 1)
//                {
//                    return false;
//                    //msg.content = this.listMsg[taskIndex - 1];
//                    //Send(msg, taskIndex - 1);
//                }

//                if (currConnectStatus)
//                {
//                    Logger.Info("Begin to send " + msg.content);
//                    Empp.submit2(msg);
//                    Logger.Info(msg.content + " was sent!");
//                }
//            }
//            else
//            {
//                if (Connection())
//                {
//                    result = arse.WaitOne(interval);
//                    if (!result & taskIndex >= 1)
//                    {
//                        return false;
//                        //msg.content = this.listMsg[taskIndex - 1];
//                        //Send(msg, taskIndex - 1);
//                    }

//                    if (currConnectStatus)
//                    {
//                        Logger.Info("Begin to send " + msg.content);
//                        Empp.submit2(msg);
//                        Logger.Info(msg.content + " was sent!");
//                    }
//                }
//                else
//                {
//                    return false;
//                }
//            }
//            return true;
//        }

//        private void InitMsgTask(String content)
//        {
//            if (content.Length <= 60)
//            {
//                this.listMsg.Add(content);
//                return;
//            }

//            string msgHeader = "({0}/{1})";
//            int len = content.Length / 55;
//            int lslen = content.Length % 55;
//            int totalcount = len + 1;

//            string strTemp = string.Empty;
//            for (int i = 0; i < content.Length / 55; i++)
//            {
//                strTemp = string.Format(msgHeader, i + 1, totalcount) + content.Substring(i * 55, 55);
//                this.listMsg.Add(strTemp);
//            }

//            //添加最后一条
//            strTemp = string.Format(msgHeader, totalcount, totalcount) + content.Substring(len * 55, lslen);
//            this.listMsg.Add(strTemp);
//        }

//        public void InitDevice(ISMSListener listener)
//        {
//            this.Listener = listener;
//        }
//    }
//}
