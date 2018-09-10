using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteDeploy.EquStateData;
using System.ComponentModel;
using System.Xml;
namespace RemoteDeploy.EquData
{
    public class DeviceState
    {
        #region 变量

        /// <summary>
        /// 应用版本号信息
        /// </summary>
        public string SoftVersion = string.Empty;

        /// <summary>
        /// 数据版本号信息
        /// </summary>
        public string DataVersion = string.Empty;

        /// <summary>
        /// 工作状态信息
        /// </summary>
        public string State = string.Empty;

        /// <summary>
        /// 预检结果
        /// </summary>
        public bool PreCheckResult = true;

        /// <summary>
        /// 预检失败原因（预检失败填写  预检成功空）
        /// </summary>
        public string PreCheckFailReason = string.Empty; 

        #endregion

        #region 构造函数

        /// <summary>
        /// 无参构造函数
        /// </summary>
        public DeviceState()
        {
        }

        /// <summary>
        /// 有参数构造函数
        /// </summary>
        /// <param name="_state">设备运行状态</param>
        /// <param name="_softVer">应用版本号信息</param>
        /// <param name="_dataVer">数据版本号信息</param>
        public DeviceState(string _state, string _softVer, string _dataVer)
        {
            State = _state;
            SoftVersion = _softVer;
            DataVersion = _dataVer;
        } 

        #endregion
    }

    /// <summary>
    /// 产品设备包含的硬件板卡设备接口，如CC，ATP等，子子系统接口类
    /// </summary>
    public abstract class IDevice
    {
        #region 成员变量
        protected string m_deviceType = "未知";
        protected string m_deviceName = "未知";
        //private IStateData state;
        protected DeviceState m_deviceState = new DeviceState();
        protected IProduct m_belongProduct;
        private string m_processState = "0%";
        //private int m_successFileCount = 0;
        #endregion

        #region 属性

        public IProduct BelongProduct
        {
            get { return m_belongProduct; }
        }
        public string DeviceType
        {
            get { return m_deviceType; }
        }
        public string State
        {
            get { return m_deviceState.State; }
            set
            { m_deviceState.State = value; }
        }
        public string ProcessState
        {
            get { return m_processState; }
            set { m_processState = value; }
        }
        public string SoftVersion
        {
            get { return m_deviceState.SoftVersion; }
            set { m_deviceState.SoftVersion = value; }
        }
        public string DataVersion
        {
            get { return m_deviceState.DataVersion; }
            set { m_deviceState.DataVersion = value; }
        }
        public bool PreCheckResult
        {
            get { return m_deviceState.PreCheckResult; }
            set { m_deviceState.PreCheckResult = value; }
        }
        public string PreCheckFailReason
        {
            get { return m_deviceState.PreCheckFailReason; }
            set { m_deviceState.PreCheckFailReason = value; }
        }
        //public int UpdateSuccessFileCount
        //{
        //    get { return m_successFileCount; }
        //    set { m_successFileCount = value; }
        //}
        public string DeviceName
        {
            get { return m_deviceName; }
        }

        #endregion

        /// <summary>
        /// 设备类接口构造函数
        /// </summary>
        /// <param name="product">所属产品接口对象作为参数</param>
        public IDevice(IProduct product)
        {
            m_belongProduct = product;
        }

        /// <summary>
        /// 设备类型加载xml数据抽象接口
        /// </summary>
        /// <param name="xmlNode">对应设备类型数据节点</param>
        public abstract void LoadXml(XmlNode xmlNode);

        /// <summary>
        /// 设备类型执行烧录操作抽象接口
        /// </summary>
        /// <param name="configState">部署文件配置状态参数</param>
        public abstract void RunDeploy(DeployConfiState configState);

    }
}
