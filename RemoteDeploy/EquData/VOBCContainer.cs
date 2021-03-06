﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using TCT.ShareLib.LogManager;
using RemoteDeploy.Common;
using System.Threading;

namespace RemoteDeploy.EquData
{
    public class VOBCContainer : IProContainer
    {

        #region 成员变量
        private string m_vobcBlueGateWay = "";
        private string m_vobcRedGateWay = "";
        private string m_vobcNetMask = "";
        #endregion

        #region 属性
        /// <summary>
        /// VOBC篮网网关
        /// </summary>
        public string VobcBlueGateWay
        {
            get { return m_vobcBlueGateWay; }
        }
        /// <summary>
        /// VOBC红网网关
        /// </summary>
        public string VobcRedGateWay
        {
            get { return m_vobcRedGateWay; }
        }
        /// <summary>
        /// VOBC掩码
        /// </summary>
        public string VobcNetMask
        {
            get { return m_vobcNetMask; }
        }
        /// <summary>
        /// VOBC产品集合容器构造函数
        /// </summary>
        public VOBCContainer()
        {
            Name = EmContainerType.VOBC;
        }
        #endregion

        /// <summary>
        /// VOBC产品集合容器的数据加载
        /// </summary>
        /// <param name="node"></param>
        public override void LoadXml(XmlNode node)
        {
            m_vobcBlueGateWay = node.Attributes[CShareLib.VOBC_BLUE_GATEWAY].Value;
            m_vobcRedGateWay = node.Attributes[CShareLib.VOBC_RED_GATEWAY].Value;
            m_vobcNetMask = node.Attributes[CShareLib.VOBC_NET_MASK].Value;

            ///遍历容器内的所有子节点，加载各个产品数据
            foreach (XmlNode xmlNode in node.ChildNodes)
            {
                VOBCProduct vobcProduct = new VOBCProduct();
                vobcProduct.LoadXml(xmlNode);
                LogManager.InfoLog.LogConfigurationInfo("线路数据", "VOBC产品数据", "完成加载VOBC产品设备：" + vobcProduct.ProductID);
                this.Add(vobcProduct);
            }
        }

        /// <summary>
        /// 设置VOBC某产品的文件请求回复状态
        /// </summary>
        /// <param name="ip">需设置产品的IP</param>
        /// <param name="port">需设置产品的端口信息</param>
        /// <param name="state">文件请求回复状态</param>
        /// <returns></returns>
        public bool SetVOBCDeviceFileState(string ip,int port, bool state)
        {
            foreach (VOBCProduct product in this)
            {
                if (product.Ip == ip && Convert.ToInt32(product.Port) == port)
                {
                    LogManager.InfoLog.LogProcInfo("VOBCContainer", "SetVOBCDeviceFileState", "设置VOBC：" + product.ProductID + "的文件请求回复状态为" + state.ToString());
                    product.FileState = state;

                }
            }

            return true;
        }

        /// <summary>
        /// 设置某VOBC产品的文件校验回复请求状态
        /// </summary>
        /// <param name="ip">设置的VOBC产品的IP</param>
        /// <param name="state">文件校验回复请求状态</param>
        public void SetVOBCDeviceCheckState(string ip, bool state)
        {
            foreach (VOBCProduct product in this)
            {
                if (product.Ip == ip)
                {
                    LogManager.InfoLog.LogProcInfo("VOBCContainer", "SetVOBCDeviceCheckState", "设置VOBC：" + product.Name + "的文件校验回复状态为" + state.ToString());
                    product.CheckState = state;

                }
            }
        }

        /// <summary>
        /// 外部接口设置VOBC某产品实体的某设备烧录进度状态属性
        /// </summary>
        /// <param name="ip">vobc产品IP</param>
        /// <param name="deviceProc">要设置的进度信息（百分比）</param>
        /// <param name="type">产品子系统类型</param>
        /// <param name="file">产品子系统文件类型</param>
        /// <returns>无</returns>
        public void SetProductVOBCDeviceProc(string ip, int deviceProc, vobcSystemType type, vobcFileType file)
        {
            //根据IP获取对应的product内容
            IProduct product = this.Find(x => (x.Ip == ip));

            //product非空验证
            if (null !=product)
            {
                //依据检索出的product 根据产品实体名称 获取对应的device内容
                IDevice device = product.CBelongsDevice.Find(y => y.DeviceName == CommonMethod.GetStringByType(type));

                //device非空验证
                if (null != device)
                {
                    //设置进度信息
                    device.ProcessState = CommonMethod.GetFileByType(file) + ":" + deviceProc.ToString()+"%";

                    if(deviceProc!=100)
                    {
                        device.State = "更新执行中";
                    }

                    //如果进度为100%，设置状态为下发完成
                    if (deviceProc==100)
                    {
                        device.State = "下发完成";
                    }

                    ///通知界面数据变化
                    dataModify.Modify();

                    //记录日志
                    LogManager.InfoLog.LogProcInfo("IProContainer", "SetProductVOBCDeviceProcState", "设置IP为" + ip + "的产品" + product.ProductID + "中的设备" + device.DeviceName + "的进度为" + deviceProc);

                    //待定：未收到更新回复超时
                    //if (device.State == "下发完成")
                    //{
                    //    int skipCount = 0;
                    //    while ((device.State != "更新成功") || (device.State != "更新失败"))
                    //    {
                    //        if (skipCount > 30)
                    //        {
                    //            device.State = "更新失败";
                    //            LogManager.InfoLog.LogProcInfo("VOBCProduct", "GenConfigHLHT", "未收到下位机允许上传回复超时或下位机拒绝上传文件");

                    //            break;
                    //        }
                    //        else
                    //        {
                    //            //跳出计数+1
                    //            skipCount++;

                    //            //休眠1秒
                    //            Thread.Sleep(1000);
                    //        }
                    //    }
                    //}
                }
                else
                {
                    //记录异常日志
                    LogManager.InfoLog.LogProcError("IProContainer", "SetProductVOBCDeviceProcState", "设置IP为" + ip + "的产品" + product.ProductID + "中的设备" + CommonMethod.GetStringByType(type) + "的进度时，未检索到该device");
                }
            }
            else
            {
                //记录异常日志
                LogManager.InfoLog.LogProcError("IProContainer", "SetProductVOBCDeviceProcState", "设置VOBC产品实体烧录进度时，未获取到IP为" + ip + "的产品");
            }
   


        }



        /// <summary>
        /// 外部接口设置VOBC某产品实体的某设备烧录成功状态，仅用于更新烧录成功失败状态，区别于常规状态设置SetProductDeviceState
        /// </summary>
        /// <param name="ip">vobc产品IP</param>
        /// <param name="systype">产品子系统类型</param>
        /// <param name="fileType">产品子系统更新文件类型</param>
        /// <returns>无</returns>
        public void SetProductVOBCDeviceState(string ip,
            vobcSystemType systype,Common.vobcFileType fileType)
        {

            //更新状态
            bool updateState = true;

            //根据IP获取对应的product内容
            VOBCProduct product = this.Find(x => (x.Ip == ip)) as VOBCProduct;

            //product非空验证
            if (null != product)
            {

                //依据检索出的product 根据产品实体名称 获取对应的device内容
                IDevice device = product.CBelongsDevice.Find(y => y.DeviceName == CommonMethod.GetStringByType(systype));

                //device非空验证
                if (null != device)
                {
                    //根据子子系统类型  获取该子子系统的更新结果
                    switch (systype)
                    {
                        case vobcSystemType.ATP_1:
                        case vobcSystemType.ATP_2:
                        case vobcSystemType.ATP_3:
                            updateState=product.CheckUpdateResultTypeFile("ATP");
                            break;
                        case vobcSystemType.ATO_1:
                        case vobcSystemType.ATO_2:
                            updateState = product.CheckUpdateResultTypeFile("ATO");
                            break;
                        case vobcSystemType.COM_1:
                        case vobcSystemType.COM_2:
                            updateState = product.CheckUpdateResultTypeFile("COM");
                            break;
                        case vobcSystemType.MMI:
                            updateState = product.CheckUpdateResultTypeFile("MMI");
                            break;
                        case vobcSystemType.CCOV:
                            updateState = product.CheckUpdateResultTypeFile("CCOV");
                            break;
                        default:
                            //TODO  不处理
                            break;
                    }

                    //根据子子系统更新结果 赋值更新状态
                    device.State = updateState ? "更新成功" : "更新失败";

                    ///通知界面数据变化
                    base.dataModify.Modify();

                    //记录日志
                    LogManager.InfoLog.LogProcInfo("IProContainer", "SetProductVOBCDeviceProcState", "设置IP为" + ip + "的产品" + product.ProductID + "中的设备" + device.DeviceName + "的烧录状态为成功");
                }
                else
                {
                    //记录异常日志
                    LogManager.InfoLog.LogProcError("IProContainer", "SetProductVOBCDeviceProcState", "设置IP为" + ip + "的产品" + product.ProductID + "中的设备" + CommonMethod.GetStringByType(systype) + "的烧录状态时，未检索到该device");
                }
            }
            else
            {
                //记录异常日志
                LogManager.InfoLog.LogProcError("IProContainer", "SetProductVOBCDeviceProcState", "设置VOBC产品实体烧录状态时，未获取到IP为" + ip + "的产品");
            }



        }

        /// <summary>
        /// 设置产品的VOBC设备的状态函数
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <param name="systype">VOBC子系统标识</param>
        /// <param name="updateState">更新状态</param>
        public void SetProductVOBCDeviceState(string ip,
            vobcSystemType systype, int updateState)
        {
            //根据IP获取对应的product内容
            VOBCProduct product = this.Find(x => (x.Ip == ip)) as VOBCProduct;

            //product非空验证
            if (null != product)
            {

                //依据检索出的product 根据产品实体名称 获取对应的device内容
                IDevice device = product.CBelongsDevice.Find(y => y.DeviceName == CommonMethod.GetStringByType(systype));

                //device非空验证
                if (null != device)
                {
                    if (updateState == CommonConstValue.constValueHEX55)
                    {
                        device.State = "更新成功";
                    }
                    else if (updateState == CommonConstValue.constValueHEXAA)
                    {
                        device.State = "更新失败";
                    }
                    else
                    {
                        device.State = "更新失败";
                    }

                    //任一系更新失败则整个相关设备更新失败

                    ///通知界面数据变化
                    base.dataModify.Modify();

                    //记录日志
                    LogManager.InfoLog.LogProcInfo("IProContainer", "SetProductVOBCDeviceProcState", "设置IP为" + ip + "的产品" + product.ProductID + "中的设备" + device.DeviceName + "的烧录状态为" + device.State);
                    product.Report.ReportWindow(ip + "的产品" + product.ProductID + "中的设备" + device.DeviceName + "的烧录状态为" + device.State);
                }
                else
                {
                    //记录异常日志
                    LogManager.InfoLog.LogProcError("IProContainer", "SetProductVOBCDeviceProcState", "设置IP为" + ip + "的产品" + product.ProductID + "中的设备" + CommonMethod.GetStringByType(systype) + "的烧录状态时，未检索到该device");
                    product.Report.ReportWindow(ip + "的产品" + product.ProductID + "中的设备" + CommonMethod.GetStringByType(systype) + "的烧录状态时，未检索到该device");
                }
            }
            else
            {
                //记录异常日志
                LogManager.InfoLog.LogProcError("IProContainer", "SetProductVOBCDeviceProcState", "设置VOBC产品实体烧录状态时，未获取到IP为" + ip + "的产品");
                product.Report.ReportWindow("设置VOBC产品实体烧录状态时，未获取到IP为" + ip + "的产品");
            }
        }
    }
}
