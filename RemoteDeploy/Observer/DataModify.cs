using RemoteDeploy.EquData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteDeploy.Observer
{
    /// <summary>
    /// 数据变动主题
    /// </summary>
    public class DataModify:ISubject
    {
        public delegate void ColorEventHandler();
        public event ColorEventHandler ColorEvent;
        /// <summary>
        /// 通知刷新界面
        /// </summary>
        public void Modify()
        {
            Notify();
        }
        /// <summary>
        /// 刷新颜色
        /// </summary>
        public void Color()
        {
            if (this.ColorEvent != null)
            {
                this.ColorEvent();
            }
        }
    }
}
