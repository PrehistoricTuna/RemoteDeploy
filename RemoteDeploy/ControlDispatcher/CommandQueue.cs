using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteDeploy.Command;

namespace RemoteDeploy.ControlDispatcher
{
    public class CommandQueue
    {
        public Queue<ICommand> m_CommandQueue = new Queue<ICommand>();

        public static CommandQueue instance = new CommandQueue();
    }
}
