using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteDeploy.Observer
{
    /// <summary>
    /// 订阅主题类
    /// </summary>
    public abstract class ISubject
    {
        public delegate void SubjectEventHandler();
        public event SubjectEventHandler SubjectEvent;
        public delegate void ReportEventHandler(string report);
        public event ReportEventHandler ReportEvent;
        /// <summary>
        /// 数据刷新
        /// </summary>
        protected void Notify()
        {
            if (this.SubjectEvent != null)
            {
                this.SubjectEvent();
            }
        }
        /// <summary>
        /// 界面打印
        /// </summary>
        /// <param name="report"></param>
        protected void Report(string report)
        {
            if (this.ReportEvent != null)
            {
                //report = "【" + DateTime.Now.ToLongTimeString() + "】 " + report;
                report = "【" + DateTime.Now.ToString("yyyyMMdd HH:mm:ss") + "】 " + report;
                this.ReportEvent(report);
            }
        }
    }
}
