using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml;
using RemoteDeploy.Command;
using RemoteDeploy.ControlDispatcher;
using RemoteDeploy.Common;
using RemoteDeploy.Observer;
using RemoteDeploy.NetworkService;

namespace RemoteDeploy.EquData
{
    /// <summary>
    /// 产品对象接口，单个VOBC，ZC设备
    /// </summary>
    public abstract class IProduct
    {
        #region 成员变量
        protected List<string> m_cSelectedDeviceType = new List<string>();///包含的硬件设备
        protected List<IDevice> m_cBelongsDevice = new List<IDevice>();///包含的硬件设备
        protected List<IDevice> m_cSelectedDevice = new List<IDevice>();///包含的需要部署的设备
        protected string m_productID;
        protected string m_productName;
        protected string m_productState;
        protected string m_ip = "";
        protected string m_port = "";
        protected EmProductLine m_cProductLine;
        private Socket_TCPClient m_cTcpClient = null;
        private WindowReport report = new WindowReport();
        private bool m_skipFlag = false;
        public DeployConfiState DeployConfigCheck = new DeployConfiState();
        public bool InProcess = false;
        #endregion

        #region 属性
        public WindowReport Report
        {
            get { return report; }
            set { report = value; }
        }
        /// <summary>
        /// 被选中的子子系统类型
        /// </summary>
        public List<string> CSelectedDeviceType
        {
            get { return m_cSelectedDeviceType; }
            set { m_cSelectedDeviceType = value; }
        }
        /// <summary>
        /// 产品IP地址
        /// </summary>
        public string Ip
        {
            get { return m_ip; }
            set { m_ip = value; }
        }
        
        /// <summary>
        /// 产品的端口
        /// </summary>
        public string Port
        {
            get { return m_port; }
            set { m_port = value; }
        }
        /// <summary>
        /// 产品系统名称
        /// </summary>
        public string Name
        {
            get { return m_productName; }
            set { m_productName = value; }
        }
        /// <summary>
        /// 产品标识名
        /// </summary>
        public string ProductID
        {
            get { return m_productID; }
            set { m_productID = value; }
        }
        /// <summary>
        /// 产品状态
        /// </summary>
        public string ProductState
        {
            get { return m_productState; }
            set { m_productState = value; }
        }
        /// <summary>
        /// 包含的子子系统设备
        /// </summary>
        public List<IDevice> CBelongsDevice
        {
            get { return m_cBelongsDevice; }
        }
        /// <summary>
        /// 需要部署的子子系统设备
        /// </summary>
        public List<IDevice> CSelectedDevice
        {
            get { return m_cSelectedDevice; }
            set { m_cSelectedDevice = value; }
        }
        /// <summary>
        /// 跳过标志
        /// </summary>
        public bool SkipFlag
        {
            get { return m_skipFlag; }
            set { m_skipFlag = value; }
        }

        public Socket_TCPClient CTcpClient
        {
            get { return m_cTcpClient; }
            set { m_cTcpClient = value; }
        }

        public int DeployProcess()
        {
            int rtnValue = 0;
            if(CSelectedDevice.Count == 0)
            {
                return rtnValue;
            }
            foreach(IDevice device in CSelectedDevice)
            {
                if(device.State == "更新成功")
                {
                    rtnValue++;
                }    
            }
            return (rtnValue*100)/CSelectedDevice.Count;
        }
        #endregion

        public IProduct()
        {

        }
        public abstract void LoadXml(XmlNode xmlNode);
        
      
        /// <summary>
        /// 生成配置文件过程
        /// </summary>
        public void GenConfig()
        {
            ///TODO;返回值可枚举
            switch (m_cProductLine)
            {
                case EmProductLine.HLHT:
                    GenConfigHLHT();
                    break;
                case EmProductLine.FAO:
                    break;
                default:
                    break;
            }

        }
        /// <summary>
        /// 文件更新过程
        /// </summary>
        public bool FileUpdate()
        {
            bool result = false;
            switch (m_cProductLine)
            {
                case EmProductLine.HLHT:
                    result = FileUpdateExec();
                    break;
                case EmProductLine.FAO:
                    break;
                default:
                    break;
            }
            return result;
        }
        /// <summary>
        /// 等待文件更新结果接口方法
        /// </summary>
        public abstract void WaitForUpdateResult();

        /// <summary>
        /// 互联互通产品线生成配置文件抽象方法
        /// </summary>
        protected abstract void GenConfigHLHT();

        /// <summary>
        /// 互联互通产品线执行文件更新抽象方法
        /// </summary>
        protected abstract bool FileUpdateExec();
        public void ResetDeviceProcState()
        {
            foreach (IDevice device in CBelongsDevice)
            {
                device.PreCheckFailReason = "";
                device.PreCheckResult = true;
                device.ProcessState = "0%";
            }
        }
    }
}
