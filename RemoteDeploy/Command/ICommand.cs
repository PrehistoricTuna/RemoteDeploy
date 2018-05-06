using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteDeploy.Command
{
    /// <summary>
    /// 命令接口
    /// </summary>
    public abstract class ICommand
    {
        /// <summary>
        /// 命令文本
        /// </summary>
        protected string m_sentence;
        /// <summary>
        /// 执行命令
        /// </summary>
        /// <returns>执行是否成功</returns>
        public abstract bool Exec();
    }
}
