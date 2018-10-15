using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using RemoteDeploy.Observer;
using TCT.ShareLib.LogManager;
using RemoteDeploy.Models.VOBC;
using RemoteDeploy.Common;

namespace RemoteDeploy.EquData
{
    /// <summary>
    /// 产品集合容器接口，VOBC，ZC等
    /// </summary>
    public abstract class IProContainer:List<IProduct>
    {

        public DataModify dataModify = new DataModify();
        private EmContainerType name = EmContainerType.NONE;
        /// <summary>
        /// 容器类型
        /// </summary>
        public EmContainerType Name
        {
            get { return name; }
            set { name = value; }
        }
        /// <summary>
        /// 加载容器数据抽象方法
        /// </summary>
        /// <param name="node"></param>
        public abstract void LoadXml(XmlNode node);

        /// <summary>
        /// 外部接口设置某产品实体的状态属性
        /// </summary>
        /// <param name="ip">IP信息</param>
        /// <param name="Port">端口信息</param>
        /// <param name="state">状态信息</param>
        /// <returns></returns>
        public void SetProductState(string ip,int Port,string state)
        {

            foreach (IProduct product in this)
            {
                if (product.Ip == ip && product.Port==Convert.ToString(Port))
                {
                    product.ProductState = state;
                    LogManager.InfoLog.LogProcInfo("IProContainer", "SetProductState", "设置IP为" + product.Ip + "的产品" + product.Name + "状态为" + state);
                }
            }
            ///通知数据变化
            dataModify.Modify();
            dataModify.Color();
            
        }

        /// <summary>
        /// 外部接口设置某产品实体的状态属性（专用于收到建链回复）
        /// </summary>
        /// <param name="ip">IP信息</param>
        /// <param name="Port">端口信息</param>
        /// <param name="state">状态信息</param>
        /// <returns></returns>
        public void SetProductStateLink(string ip, int Port, string state)
        {

            foreach (IProduct product in this)
            {
                if (product.Ip == ip && product.Port == Convert.ToString(Port))
                {
                    product.ProductState = state;
                    LogManager.InfoLog.LogProcInfo("IProContainer", "SetProductState", "设置IP为" + product.Ip + "的产品" + product.Name + "状态为" + state);
                }
            }
            ///通知数据变化
            dataModify.Modify();
        }

        /// <summary>
        /// 外部接口设置VOBC状态信息
        /// </summary>
        /// <param name="ip">产品设备IP信息</param>
        /// <param name="Port">端口信息</param>
        /// <param name="vInfo">VOBC状态信息类实例</param>
        /// <returns></returns>
        public bool SetProductVobcStateInfo(string ip,int Port, VOBCStateInfoClass vInfo)
        {
            bool reValue = false;

            foreach (IProduct product in this)
            {
                if (product.Ip == ip && product.Port == Convert.ToString(Port))
                {
                    //防护 必须是VOBC产品
                    if (product is VOBCProduct)
                    {
                        (product as VOBCProduct).VobcStateInfo = vInfo;

                        reValue = true;
                    }
                    else
                    {
                        reValue = false;
                    }

                }
                else 
                {
                    reValue = false;
                }
            }

            return reValue;
        }

        /// <summary>
        /// 外部接口设置某产品实体的部署失败状态信息
        /// </summary>
        /// <param name="ip">产品设备IP信息</param>
        /// <param name="Port">端口信息</param>
        /// <param name="failReason">失败原因文本信息</param>
        public void SetProductFailReason(string ip,int Port, string failReason)
        {
            foreach (IProduct product in this)
            {
                if (product.Ip == ip && product.Port == Convert.ToString(Port))
                {
                    foreach (IDevice device in product.CBelongsDevice)
                    {
                        device.PreCheckFailReason = failReason;
                    }
                }
            }
            //通知数据变化
            dataModify.Modify();
        }

        /// <summary>
        /// 外部接口设置某产品实体的某设备状态信息（状态信息包含N个字段）
        /// </summary>
        /// <param name="ip">产品IP信息</param>
        /// <param name="Port">端口信息</param>
        /// <param name="deviceName">产品实体名称</param>
        /// <param name="state">状态信息</param>
        /// <returns></returns>
        public bool SetProductDeviceState(string ip, int Port, string deviceName, DeviceState deviceState)
        {
            foreach (IProduct product in this)
            {
                if (product.Ip == ip && product.Port == Convert.ToString(Port))
                {
                    foreach (IDevice device in product.CBelongsDevice)
                    {
                        if (device.DeviceName.Contains(deviceName))
                        {
                            device.State = deviceState.State;
                            device.SoftVersion = deviceState.SoftVersion;
                            device.DataVersion = deviceState.DataVersion;
                            //device.PreCheckResult = deviceState.PreCheckResult;
                            //device.PreCheckFailReason = deviceState.PreCheckFailReason;
                            LogManager.InfoLog.LogProcInfo("IProContainer", "SetProductState", "设置IP为" + product.Ip + "的产品" + product.Name + "中的设备" + device.DeviceName + "的状态为" + deviceState.State);
                        }

                    }
                }
            }

            ///通知数据变化
            ///dataModify.Modify();

            return true;
        }

        /// <summary>
        /// 外部接口设置某产品实体的某设备状态属性
        /// </summary>
        /// <param name="ip">产品IP信息</param>
        /// <param name="Port">端口信息</param>
        /// <param name="deviceName">产品实体名称</param>
        /// <param name="deviceStateName">状态名称</param>
        /// <returns></returns>
        public bool SetProductDeviceState(string ip,int Port,List<vobcSystemType> sTypeList, string deviceStateName)
        {
            //遍历产品实体列表
            foreach (IProduct product in this)
            {
                //获取指定IP的产品
                if (product.Ip == ip && product.Port == Convert.ToString(Port))
                {
                    //遍历该产品下的实体
                    foreach (IDevice device in product.CBelongsDevice)
                    {
                        foreach (vobcSystemType sType in sTypeList)
                        {
                            //获取指定名称的实体
                            if (device.DeviceName.Contains(CommonMethod.GetVobcSystemNameByType(sType)))
                            {
                                device.State = deviceStateName;
                                LogManager.InfoLog.LogProcInfo("IProContainer", "SetProductState", "设置IP为" + product.Ip + "的产品" + product.Name + "中的设备" + device.DeviceName + "的状态为" + deviceStateName);
                            }
                        }

                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 统一设置全部设备状态 Modified @ 9.10
        /// </summary>
        /// <param name="ip">产品IP信息</param>
        /// <param name="Port">端口信息</param>
        /// <param name="deviceState">设备需显示的状态</param>
        public void SetProductDeviceState(string ip, int Port, string deviceState)
        {
            foreach (IProduct product in this)
            {
                if (product.Ip == ip && product.Port == Convert.ToString(Port))
                {
                    foreach (IDevice device in product.CBelongsDevice)
                    {
                        device.State = deviceState;
                    }
                }
            }
            dataModify.Modify();
        }

    }
}
