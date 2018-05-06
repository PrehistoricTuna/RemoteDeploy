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
        public void Modify()
        {
            Notify();
        }
        public void Color()
        {
            if (this.ColorEvent != null)
            {
                this.ColorEvent();
            }
        }
    }
}
