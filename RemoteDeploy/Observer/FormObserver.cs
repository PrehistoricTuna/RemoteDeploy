using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteDeploy.Observer
{
    /// <summary>
    /// 界面观察者
    /// </summary>
    public class FormObserver:IObserver
    {
        private MainWindow m_mainWindow = null;
        /// <summary>
        /// 界面观察者
        /// </summary>
        /// <param name="mainWindow">主窗体实例</param>
        public FormObserver(MainWindow mainWindow)
        {
            m_mainWindow = mainWindow;
        }
        /// <summary>
        /// vobc状态刷新
        /// </summary>
        public override void SubjectEvent()
        {
            m_mainWindow.container_EBackData();
        }
        /// <summary>
        /// 界面打印区刷新
        /// </summary>
        /// <param name="report"></param>
        public override void ReportEvent(string report)
        {
            m_mainWindow.ProductReport(report);
        }
        /// <summary>
        /// DataGridView控件修改颜色
        /// </summary>
        public void ColorEvent()
        {
            m_mainWindow.DataGridView_Change_Color();
        }
        /// <summary>
        /// 增加颜色事件
        /// </summary>
        /// <param name="subject">数据变动订阅者</param>
        public void AddColorEvent(DataModify subject)
        {
            subject .ColorEvent+= new DataModify.ColorEventHandler(ColorEvent);
        }
    }
}
