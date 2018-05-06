using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TCT.ShareLib.LogManager;

namespace RemoteDeploy.Observer
{
    /// <summary>
    /// 日志观察者
    /// </summary>
    public class LogObserver:IObserver
    {
        public override void SubjectEvent()
        {
            LogManager.InfoLog.LogProcInfo("LogObserver", "SubjectEvent", "监听到数据变化刷新订阅");
        }
        public override void ReportEvent(string report)
        {
            
        }
    }
}
