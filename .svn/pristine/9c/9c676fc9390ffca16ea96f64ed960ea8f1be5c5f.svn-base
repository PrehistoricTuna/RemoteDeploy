using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using TCT.ShareLib.LogManager;
using RemoteDeploy.NetworkService;
using AutoBurnInterface;

namespace RemoteDeploy.EquData
{
    public class ZCAddCheckState
    {
        public bool m_ccSent = false;
        public bool m_pu13Sent = false;
        public bool m_pu24Sent = false;
        public bool m_comSent = false;
        public void Reset()
        {
            m_ccSent = false;
            m_pu13Sent = false;
            m_pu24Sent = false;
            m_comSent = false;
        }
    }
    public class ZCProduct:IProduct
    {
        private AutoBurnPush autoBurnPush;
        private List<BurnFileInfo> bfis = new List<BurnFileInfo>();
        private List<UpdateInfo> uis = new List<UpdateInfo>();
        public ZCAddCheckState AddState = new ZCAddCheckState();
        public AutoBurnPush AutoBurnPush
        {
            get { return autoBurnPush; }
            set { autoBurnPush = value; }
        }

        public List<BurnFileInfo> Bfis
        {
            get
            {
                return bfis;
            }

            set
            {
                bfis = value;
            }
        }

        public List<UpdateInfo> Uis
        {
            get
            {
                return uis;
            }

            set
            {
                uis = value;
            }
        }

        public override void LoadXml(XmlNode xmlNode)
        {
            m_productID = xmlNode.Attributes[CShareLib.XML_PRODUCTID].InnerText;
            m_productName = xmlNode.Attributes[CShareLib.XML_PRODUCTNAME].InnerText;
            m_productState = xmlNode.Attributes[CShareLib.XML_PRODUCT_STATE].InnerText;
            m_ip = xmlNode.Attributes[CShareLib.XML_PRODUCT_IP].InnerText;
            m_port = xmlNode.Attributes[CShareLib.XML_PRODUCT_PORT].InnerText;
            m_cProductLine = CDeviceDataFactory.Instance.ProjectConsole.ProductLine;
            ///遍历所有子节点，加载所属子子系统的数据
            foreach (XmlNode node in xmlNode.ChildNodes)
            {
                string type = node.Attributes[CShareLib.XML_DEVICE_TYPE].InnerText;
                ZCDevice device = CreateZCDevice(type);
                if (device != null)
                {
                    device.LoadXml(node);
                    m_cBelongsDevice.Add(device);
                    LogManager.InfoLog.LogConfigurationInfo("线路数据", "ZC产品设备数据", "完成加载ZC设备" + this.ProductID + "的子子系统：" + device.DeviceName + "数据");
                }
                else
                {
                    throw new MyException("非法设备类型：" + type);
                }
            }
        }

        internal bool IsReadyUpdate()
        {
            bool rtnValue = true;
            foreach(string type in CSelectedDeviceType)
            {
                switch(type)
                {
                    case "CC":
                        rtnValue &= AddState.m_ccSent;
                        break;
                    case "COM":
                        rtnValue &= AddState.m_comSent;
                        break;
                    case "PU13":
                        rtnValue &= AddState.m_pu13Sent;
                        break;
                    case "PU24":
                        rtnValue &= AddState.m_pu24Sent;
                        break;
                    default:
                        break;
                }
            }
            return rtnValue;
        }

        /// <summary>
        /// ZC设备工厂
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private ZCDevice CreateZCDevice(string type)
        {
            ZCDevice device;
            switch (type)
            {
                case "PU13":
                    device = new ZCPU13Device(this);
                    break;
                case "PU24":
                    device = new ZCPU24Device(this);
                    break;
                case "CC":
                    device = new CcDevice(this);
                    break;
                case "COM":
                    device = new CommDevice(this);
                    break;
                default:
                    device = new ZCDevice(this);
                    break;
            }
            return device;
        }
        protected override bool FileUpdateExec()
        {
            Report.ReportWindow("ZC设备" + m_productID + "正在上传文件...");
            if (autoBurnPush == null)
            {
                return false;
            }
            //上传文件
            string pbfErr = autoBurnPush.IRequest.PushBurnFiles(bfis);
            if (pbfErr.Equals(string.Empty))
            {
                try
                {
                    Report.ReportWindow("ZC设备" + m_productID + "正在部署文件...");
                    autoBurnPush.IRequest.Update(Uis);
                    Report.ReportWindow("ZC设备" + m_productID + "部署成功！");
                    LogManager.InfoLog.LogProcInfo("ZCProduct", "FileUpdateExec", "ZC设备" + m_productID + "部署成功！");
                }
                catch (Exception exex)
                {
                    //err
                    Report.ReportWindow("ZC设备" + m_productID + "更新失败停止部署,失败原因：" + exex.Message);
                    LogManager.InfoLog.LogProcError("ZCProduct", "FileUpdateExec", "ZC设备" + m_productID + "更新文件失败,失败原因：" + pbfErr);
                    return false;
                }
            }
            else
            {
                //err
                Report.ReportWindow("ZC设备" + m_productID + "上传文件失败停止部署,失败原因：" + pbfErr);
                LogManager.InfoLog.LogProcError("ZCProduct", "FileUpdateExec", "ZC设备" + m_productID + "上传文件失败,失败原因：" + pbfErr);
                return false;
            }
            return true;
        }
        protected override void GenConfigHLHT()
        {
            LogManager.InfoLog.LogProcInfo("ZCProduct", "GenConfigHLHT", "生成ZC设备"+m_productID+"配置文件（未实现）");
        }
        public override void WaitForUpdateResult()
        {
            throw new NotImplementedException();
        }
        public string GetDeviceNameByBurnType(BurnDevice type)
        {
            foreach (ZCDevice device in m_cBelongsDevice)
            {
                if (device.BurnDeviceType == type)
                {
                    return device.DeviceName;
                }
            }
            return "";
        }
        //public bool PreCheck()
        //{
        //    Dictionary<BurnDevice, string> pcBstrs = autoBurnPush.IRequest.PreCheck(bds);
        //    bool preOk = true;
        //    foreach (var pcBstr in pcBstrs)
        //    {
        //        if (!pcBstr.Value.Equals(string.Empty))
        //        {
        //            preOk = false;
        //            //err
        //        }
        //    }
        //    return preOk;
        //}
    }
}
