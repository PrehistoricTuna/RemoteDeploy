using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteDeploy.SendRecv.Send;
using System.Threading.Tasks;

namespace RemoteDeploy.SendRecv
{
    public class SendRecv
    {
        /// <summary>
        /// 是否继续执行
        /// </summary>
        public bool continueExe = false;

        public virtual void Init()
        {
            continueExe = true;
            Task task = new Task(Begin);
            task.Start();
        }

        public virtual void Begin()
        { }

        public void End()
        {
            continueExe = false;
        }
    }
}
