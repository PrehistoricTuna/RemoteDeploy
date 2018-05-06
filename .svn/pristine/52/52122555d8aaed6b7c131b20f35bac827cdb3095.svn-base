using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using RemoteDeploy.NetworkService;

namespace RemoteDeploy.Command
{
    public class InitCommand:ICommand
    {
        public override bool Exec()
        {
            Udp.Send("初始化！");
            return true;
        }
    }
}
