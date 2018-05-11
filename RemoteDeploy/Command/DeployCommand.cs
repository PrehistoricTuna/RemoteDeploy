using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using RemoteDeploy.NetworkService;

namespace RemoteDeploy.Command
{
    public class DeployCommand: ICommand
    {
        public string ChezuName{ get; private set; }
        public string MechineName{ get; private set; }
        /// <summary>
        /// 部署命令
        /// </summary>
        /// <param name="chezuName">车组名</param>
        /// <param name="mechineName">设备名</param>
        public DeployCommand(string chezuName, string mechineName)
        {
            this.ChezuName = chezuName;
            this.MechineName = mechineName;
        }
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <returns>返回布尔值</returns>
        public override bool Exec()
        {
            Udp.Send("发布" + this.ChezuName + "-" + this.MechineName);
            return true;
        }
    }
}
