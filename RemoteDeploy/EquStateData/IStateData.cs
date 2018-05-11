using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteDeploy.EquStateData
{
    /// <summary>
    /// 状态数据抽象类
    /// </summary>
    public abstract class IStateData
    {
        private bool m_bIsOnline = false;

        public virtual bool IsOnline
        {
            get { return m_bIsOnline; }
        }
    }
}
