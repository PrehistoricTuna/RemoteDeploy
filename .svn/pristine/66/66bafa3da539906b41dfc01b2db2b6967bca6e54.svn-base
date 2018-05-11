using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteDeploy.Observer
{
    /// <summary>
    /// 观察者接口
    /// </summary>
    public abstract class IObserver
    {
        /// <summary>
        /// 添加数据刷新订阅
        /// </summary>
        /// <param name="subject">订阅者</param>
        public void AddProcess(ISubject subject)
        {
            subject.SubjectEvent += new ISubject.SubjectEventHandler(SubjectEvent);
        }
        /// <summary>
        /// 添加界面打印订阅
        /// </summary>
        /// <param name="subject"></param>
        public void AddReport(ISubject subject)
        {
            subject.ReportEvent += new ISubject.ReportEventHandler(ReportEvent);
        }
        /// <summary>
        /// 数据刷新抽象类
        /// </summary>
        public abstract void SubjectEvent();
        /// <summary>
        /// 界面打印抽象类
        /// </summary>
        /// <param name="report"></param>
        public abstract void ReportEvent(string report);
    }
}
