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
        public void AddProcess(ISubject subject)
        {
            subject.SubjectEvent += new ISubject.SubjectEventHandler(SubjectEvent);
        }
        public void AddReport(ISubject subject)
        {
            subject.ReportEvent += new ISubject.ReportEventHandler(ReportEvent);
        }
        public abstract void SubjectEvent();
        public abstract void ReportEvent(string report);
    }
}
