using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using RemoteDeploy.NetworkService;

using RemoteDeploy.EquData;
using RemoteDeploy.ControlDispatcher;
using RemoteDeploy.DataPack;
using RemoteDeploy.Models.VOBC;

namespace RemoteDeploy.SendRecv.Recv
{
    public class Recv : SendRecv
    {

        public override void Begin()
        {
  
            #region UDP接收代码 调试使用

            //while (true)
            //{
            //    if (continueExe)
            //    {
            //        byte[] recvServerData = Udp.Recv("127.0.0.1", 60000);

            //        DataAnalysis.VOBCDataAnalysis(recvServerData);
            //    }
            //    else
            //    {
            //        break;
            //    }
            //} 

            #endregion
        }



    }
}
