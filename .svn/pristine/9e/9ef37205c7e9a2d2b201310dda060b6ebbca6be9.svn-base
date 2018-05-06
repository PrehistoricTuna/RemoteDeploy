using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using TCT.ShareLib.LogManager;
using AutoBurnInterface;

namespace RemoteDeploy.EquData
{
    public class ZCDevice:IDevice
    {
        private BurnDevice m_burnDeviceType = (BurnDevice)(-1);

        public BurnDevice BurnDeviceType
        {
            get { return m_burnDeviceType; }
        }
        public ZCDevice(IProduct product)
            : base(product)
        {
        }
        public override void LoadXml(XmlNode xmlNode)
        {
            m_deviceType = xmlNode.Attributes[CShareLib.XML_DEVICE_TYPE].InnerText;
            m_deviceName = xmlNode.Attributes[CShareLib.XML_DEVICE_NAME].InnerText;
            m_deviceState.SoftVersion = xmlNode.Attributes[CShareLib.XML_DEVICE_STATE].InnerText;
            InitDeviceBurnType(m_deviceName);
        }
        private void InitDeviceBurnType(string deviceName)
        {
            switch (deviceName)
            {
                case "CC1":
                    m_burnDeviceType = BurnDevice.Ccov1;
                    break;
                case "CC2":
                    m_burnDeviceType = BurnDevice.Ccov2;
                    break;
                case "PU1":
                    m_burnDeviceType = BurnDevice.Host1;
                    break;
                case "PU2":
                    m_burnDeviceType = BurnDevice.Host2;
                    break;
                case "PU3":
                    m_burnDeviceType = BurnDevice.Host3;
                    break;
                case "PU4":
                    m_burnDeviceType = BurnDevice.Host4;
                    break;
                case "COM1":
                    m_burnDeviceType = BurnDevice.Ftsm1;
                    break;
                case "COM2":
                    m_burnDeviceType = BurnDevice.Ftsm2;
                    break;
                default:
                    break;

            }
        }
        public override void RunDeploy(DeployConfiState configState)
        {
            ZCProduct zc = base.BelongProduct as ZCProduct;
            zc.DeployConfigCheck = configState;

            if (zc.InProcess == false)
            {
                LogManager.InfoLog.LogProcInfo("VOBCDevice", "RunDeploy", "开始ZC产品：" + zc.ProductID + "部署过程");
                BelongProduct.Report.ReportWindow("开始ZC产品：" + zc.ProductID + "部署过程");
                //重置类型文件添加状态
                zc.AddState.Reset();
                zc.Bfis.Clear();
                zc.Uis.Clear();
                CDeviceDataFactory.Instance.ZcContainer.dataModify.Color();
                zc.InProcess = true;

            }    
            DeployExec();
            if(zc.IsReadyUpdate())
            {
                zc.FileUpdate();
            }
        }
        /// <summary>
        /// ZC子子系统设备部署执行（更新复位）执行虚方法
        /// </summary>
        public virtual void DeployExec()
        {
            MessageBox.Show("未定义VOBC设备部署过程：" + base.DeviceType + "名称：" + base.DeviceName);
        }
    }
}
