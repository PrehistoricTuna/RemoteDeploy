using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using AutoBurnInterface;


namespace RemoteDeploy.NetworkService
{
    /// <summary>
    /// 自动烧录推送
    /// </summary>
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false, IncludeExceptionDetailInFaults = true)]

    ///自动烧录推送类
    public class AutoBurnPush:Ipush
    {
        private string ip = "";
        public IRequest IRequest { get; private set; }

        private Action<int, int, string, string> UpdateStatusAct;


        public void UpdateStatus(int curRate, int totalRate, string info)
        {
            UpdateStatusAct(curRate, totalRate, info,ip);
        }

        public AutoBurnPush()
        {


        }
        /// <summary>
        /// 自动烧录推送方法
        /// </summary>
        /// <param name="remoteIp">远程部署的ip</param>
        /// <param name="setFuc">封装处理的设置方法</param>
        public AutoBurnPush(string remoteIp, Action<int, int, string, string> setFuc)
        {
            UpdateStatusAct = setFuc;
            ip = remoteIp;
            NetTcpBinding ntb = new NetTcpBinding(SecurityMode.None, true);
            ntb.MaxReceivedMessageSize = int.MaxValue;
            ntb.ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas() { MaxArrayLength = int.MaxValue };
            ntb.ReliableSession.InactivityTimeout = new TimeSpan(0, 0, 30);
            

            DuplexChannelFactory<IRequest> dcf = new DuplexChannelFactory<IRequest>(new InstanceContext(this), ntb, new EndpointAddress(string.Format("net.tcp://{0}//AutoBurnService", remoteIp)));
            IRequest = dcf.CreateChannel();
            //(IRequest as IContextChannel).OperationTimeout = new TimeSpan(0, 4, 0);
        }
    }
}
