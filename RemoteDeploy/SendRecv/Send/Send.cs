﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteDeploy.ControlDispatcher;
using RemoteDeploy.Command;
using System.Threading;

namespace RemoteDeploy.SendRecv.Send
{
    public class Send : SendRecv
    {
        public override void Begin()
        {
            while (true)
            {
                if (continueExe)
                {
                    if (CommandQueue.instance.m_CommandQueue.Count != 0)
                    {
                        ICommand command = CommandQueue.instance.m_CommandQueue.Dequeue();
                        command.Exec();
                    }
                }
                else
                {
                    break;
                }
                Thread.Sleep(100);
            }
        }
    }
}