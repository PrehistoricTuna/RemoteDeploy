using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteDeploy.Command;
using RemoteDeploy.ProcState;

namespace RemoteDeploy.ControlDispatcher
{
    /// <summary>
    /// 控制调度器接口
    /// </summary>
    public abstract class IControlDispatcher
    {
        private List<ICommand> m_cCommands = new List<ICommand>();
        /// <summary>
        /// 初始化调度器内的命令集合
        /// </summary>
        public abstract void InitCommands();

        /// <summary>
        ///执行调度器内的所有命令
        /// </summary>
        public void AutoRun()
        {

        }
    }
}
