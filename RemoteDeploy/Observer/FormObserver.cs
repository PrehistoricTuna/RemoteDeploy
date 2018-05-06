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
        public FormObserver(MainWindow mainWindow)
        {
            m_mainWindow = mainWindow;
        }
        public override void SubjectEvent()
        {
            m_mainWindow.container_EBackData();
        }
        public override void ReportEvent(string report)
        {
            m_mainWindow.ProductReport(report);
        }
        public void ColorEvent()
        {
            m_mainWindow.DataGridView_Change_Color();
        }
        public void AddColorEvent(DataModify subject)
        {
            subject .ColorEvent+= new DataModify.ColorEventHandler(ColorEvent);
        }
    }
}
