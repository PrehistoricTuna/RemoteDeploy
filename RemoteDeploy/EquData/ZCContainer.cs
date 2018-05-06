﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using TCT.ShareLib.LogManager;

namespace RemoteDeploy.EquData
{
    public class ZCContainer:IProContainer
    {
        private string m_ZCMIp = "";

        public string ZCMIp
        {
            get { return m_ZCMIp; }
            set { m_ZCMIp = value; }
        }
        public ZCContainer()
        {
            Name =EmContainerType.ZC;
        }
        public override void LoadXml(XmlNode node)
        {
            m_ZCMIp = node.Attributes["ZCMIp"].Value;
            ///遍历容器内的所有子节点，加载各个产品数据
            foreach (XmlNode xmlNode in node.ChildNodes)
            {
                ZCProduct zcProduct = new ZCProduct();
                zcProduct.LoadXml(xmlNode);
                LogManager.InfoLog.LogConfigurationInfo("线路数据", "ZC产品数据", "完成加载ZC产品设备：" + zcProduct.ProductID);
                this.Add(zcProduct);
            }
        }
        /// <summary>
        /// 外部接口设置某产品实体的某设备进度状态属性
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="deviceName"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public void SetProductZCDeviceProcState(string ip, string deviceProcState,string info)
        {
            foreach (IProduct product in this)
            {
                if (product.Ip == ip)
                {
                    foreach (IDevice device in product.CBelongsDevice)
                    {
                        device.ProcessState = deviceProcState;
                        LogManager.InfoLog.LogProcInfo("IProContainer", "SetProductZCDeviceProcState", "设置IP为" + product.Ip + "的产品" + product.ProductID + "中的设备" + device.DeviceName + "的进度为" + deviceProcState);

                    }
                    if (info != string.Empty)
                    {
                        product.Report.ReportWindow("ZC设备" + product.ProductID + "部署进度：" + info);
                        LogManager.InfoLog.LogProcError("ZCContainer", "SetProductZCDeviceProcState", "ZC设备" + product.ProductID + "部署进度：" + info);
                    }
                }
            }

            ///通知数据变化
            base.dataModify.Modify();
        }
        public void SetZCProc(int curRate, int totalRate, string info, string ip)
        {
            SetProductZCDeviceProcState(ip, curRate.ToString() + "/" + totalRate.ToString(), info);
        }
    }
}
