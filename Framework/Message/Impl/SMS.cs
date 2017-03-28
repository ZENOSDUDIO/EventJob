using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EMPPLib;
using System.Threading;

namespace QCMonitor.Framework.Framework.Message.Impl
{
    public delegate void StatusChangedHandler(int sqid, string msg);
    public delegate void NotifyHandler(string msg);
    public class SMS
    {
        #region private member
        private static SMS _unique = new SMS();
        private List<string> listMsg = new List<string>();
        private bool _isConnecting;
        private EMPPLib.emptcl _ctl;
        private EMPPOptions _config;
        #endregion

        #region public property
        public static SMS Unique
        {
            get
            {
                return _unique;
            }
        }

        public void Refresh()
        {
            _config = new EMPPOptions();
        }
        public int Delay
        {
            get
            {
                return _config.Delay;
            }
        }
        public int ResendTimeOut
        {
            get
            {
                return _config.ResendTimeOut;
            }
        }

        private static StatusChangedHandler _statusChanged;
        public static StatusChangedHandler StatusChanged
        {
            set
            {
                _statusChanged = value;
            }
        }

        private static NotifyHandler _notifyHandler;
        public static NotifyHandler NotifyHandler
        {
            set
            {
                _notifyHandler = value;
            }
        }
        #endregion
    
        #region private method
        private SMS()
        {
            Refresh();
            BuildCtl();
        }
        private void BuildCtl()
        {
            _ctl = new emptcl();
            _ctl.EMPPClosed += new _IemptclEvents_EMPPClosedEventHandler(_ctl_EMPPClosed);
            _ctl.SocketClosed += new _IemptclEvents_SocketClosedEventHandler(_ctl_SocketClosed);
            _ctl.StatusReceivedInterface += new _IemptclEvents_StatusReceivedInterfaceEventHandler(_ctl_StatusReceivedInterface);
            _ctl.SubmitRespInterface += new _IemptclEvents_SubmitRespInterfaceEventHandler(_ctl_SubmitRespInterface);
        }

        private static void _ctl_SubmitRespInterface(SubmitResp sm)
        {
            try
            {
                if (_statusChanged != null)
                {
                    if (sm.Result != SubmitResultEnum.SUBMIT_OK)
                    {
                        _statusChanged(sm.SequenceID, "0|Submit failed:" + sm.Result);
                    }
                    else
                    {
                        _statusChanged(sm.SequenceID, "3|Submited:" + sm.Result);
                    }
                }
            }
            catch
            {
            }
        }

        private static void _ctl_StatusReceivedInterface(StatusReport sm)
        {
            try
            {
                if (_statusChanged != null)
                {
                    _statusChanged(sm.SeqID, parseStatus(sm.Status));
                }
            }
            catch { }
        }

        private void SleepDelay(int delayMs)
        {
            DateTime start = DateTime.Now;
            while (true)
            {
                System.Threading.Thread.Sleep(1);
                TimeSpan ts = DateTime.Now - start;
                if (ts.TotalMilliseconds > delayMs)
                {
                    break;
                }
            }
        }

        private void CheckConnection()
        {
            if (_isConnecting)
            {
                return;//避免循环递归  
            }
            _isConnecting = true;
            if (!_ctl.connected)
            {
                ConnectResultEnum ret = ConnectResultEnum.CONNECT_AUTH_ERROR;
                int tryTimes = 0;
                while (ret != ConnectResultEnum.CONNECT_OK && ret != ConnectResultEnum.CONNECT_KICKLAST && (tryTimes++ < 2))
                {
                    Thread.Sleep(3000);
                    ret = _ctl.connect(_config.HostIP, _config.Port, _config.Account, _config.PassWord);
                }
                if (ret != ConnectResultEnum.CONNECT_OK && ret != ConnectResultEnum.CONNECT_KICKLAST)
                {
                    _isConnecting = false;
                    throw new Exception("连接登录失败，请检查网络和账号配置。");
                }
            }
            _isConnecting = false;
        }

        private void AnalysisMsgTask(String content)
        {
            listMsg.Clear();
            if (content.Length <= 60)
            {
                this.listMsg.Add(content);
                return;
            }

            string msgHeader = "({0}/{1})";
            int len = content.Length / 55;
            int lslen = content.Length % 55;
            int totalcount = len + 1;

            string strTemp = string.Empty;
            for (int i = 0; i < content.Length / 55; i++)
            {
                strTemp = string.Format(msgHeader, i + 1, totalcount) + content.Substring(i * 55, 55);
                this.listMsg.Add(strTemp);
            }

            //添加最后一条
            strTemp = string.Format(msgHeader, totalcount, totalcount) + content.Substring(len * 55, lslen);
            this.listMsg.Add(strTemp);
        }

        private static string parseStatus(string status)
        {
            if (status.ToUpper() == "DELIVRD")
            {
                return "1|" + status;//成功  
            }
            else if (status == "")
            {
                return "2|No response yet,sending may be delayed";
            }
            else
            {
                return "0|status=" + status;//失败  
            }
        }

        private void _ctl_EMPPClosed(int errorCode)
        {
            try
            {
                CheckConnection();
            }
            catch { }
        }

        private void _ctl_SocketClosed(int errorCode)
        {
            try
            {
                CheckConnection();
            }
            catch { }
        }

        #endregion

        #region public methord
        /// <summary>  
        /// 发送消息函数  
        /// </summary>  
        /// <param name="message">消息内容</param>  
        /// <param name="phoneNumber">手机号码</param>  
        public string SendMessage(string phoneNumber, string message, int seqID)
        {
            if (message != null && phoneNumber != null)
            {
                AnalysisMsgTask(message);
                try
                {
                    _ctl.needStatus = true;
                    ShortMessage msg = new ShortMessage();
                    Mobiles mobiles = new Mobiles();
                    mobiles.Add(phoneNumber);
                    msg.DestMobiles = mobiles;

                    msg.needStatus = true;
                    msg.ServiceID = _config.ServiceID;
                    msg.SendNow = true;
                    msg.srcID = _config.Account;
                    msg.SequenceID = seqID;

                    CheckConnection();
                    if (_ctl.connected)
                    {
                        try
                        {
                            foreach (var item in this.listMsg)
                            {
                                msg.content = item;
                                _ctl.submit(msg);
                                if (_notifyHandler != null)
                                {
                                    _notifyHandler(string.Format("sqId:{0} msgcontent:{1} was sent.", seqID, item));
                                }
                            }

                        }
                        catch (System.Runtime.InteropServices.COMException ex)
                        {
                            return "0|EmppError,Exit!";
                        }
                        return "2|Sending...";
                    }
                    else
                    {
                        return "0|Not connected.";
                    }
                }
                catch (Exception ex)
                {
                    return "0|" + ex.Message + "\r\n" + ex.StackTrace;
                }
            }
            else
            {
                return "0|PhoneNumber or message is null";
            }
        }

        public void Dispose()
        {
            if (_ctl != null)
            {
                _ctl.disconnect();
            }
        }
        #endregion
    }
}
