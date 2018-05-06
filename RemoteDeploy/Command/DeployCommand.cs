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

        public DeployCommand(string chezuName, string mechineName)
        {
            this.ChezuName = chezuName;
            this.MechineName = mechineName;
        }

        public override bool Exec()
        {
            Udp.Send("发布" + this.ChezuName + "-" + this.MechineName);
            return true;
        }
    }
}
