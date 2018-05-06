using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using RemoteDeploy.Models.VOBC;
using TCT.ShareLib.LogManager;
using RemoteDeploy.Observer;

namespace RemoteDeploy.EquData
{
    /// <summary>
    /// 内存数据单例，当前项目内所有数据及接口封装
    /// </summary>
    public class CDeviceDataFactory
    {
        #region 成员变量
        private IProjectConsole m_projectConsole;
        private LogObserver logObs = new LogObserver();
        #endregion
        #region 属性
        /// <summary>
        /// 单例内的VOBC产品集合容器
        /// </summary>
        public VOBCContainer VobcContainer
        {
            get {
                VOBCContainer vobcContainer = Instance.ProjectConsole.Projducts.Find((IProContainer container) => container.Name == EmContainerType.VOBC) as VOBCContainer; 
                if (vobcContainer != null)
                {
                    return vobcContainer;
                }
                else
                {
                    throw new MyException("不存在VOBC产品实体！");
                }
            }
        }
        /// <summary>
        /// 单例内的ZC产品集合容器
        /// </summary>
        public ZCContainer ZcContainer
        {
            get
            {
                ZCContainer zcContainer = Instance.ProjectConsole.Projducts.Find((IProContainer container) => container.Name == EmContainerType.ZC) as ZCContainer;
                if (zcContainer != null)
                { 
                    return zcContainer;
                }
                else
                {
                    throw new MyException("不存在ZC产品实体！");
                }
            }
        }
        /// <summary>
        /// 当前的项目控制台实体
        /// </summary>
        public IProjectConsole ProjectConsole
        {
            get { return m_projectConsole; }
        }
        #endregion
        private CDeviceDataFactory() 
        {
            
        }

         /// <summary>
        /// 读取所选文件夹内的xml数据结构文件
        /// </summary>
        /// <returns></returns>
        public bool LoadXml(string dataPath)
        {
            FileInfo filePath = new FileInfo(dataPath);
            if (!filePath.Exists)
            {
                MessageBox.Show(filePath.FullName + " 配置文件不存在！");
                LogManager.InfoLog.LogConfigurationError(filePath.Name, "线路数据配置文件", "配置文件不存在");
                return false;
            }
            try
            {
                XmlDocument xmlDataItemsFile = new XmlDocument();
                xmlDataItemsFile.Load(dataPath);
                XmlNode rootNode = xmlDataItemsFile.SelectSingleNode(CShareLib.XML_ROOT);
                string projectType = rootNode.Attributes[CShareLib.XML_ROOT_PROTYPE].InnerText;
                switch (projectType)
                {
                    case "HLHTLine":
                        m_projectConsole = new HLHTLineConsole();
                        break;
                    default:
                        throw new MyException("未知类型项目" + projectType);

                }
                m_projectConsole.Name = rootNode.Attributes[CShareLib.XML_ROOT_PROTYPE].InnerText;
                XmlNodeList nodeList = rootNode.ChildNodes;
                m_projectConsole.LoadXml(nodeList);

            }
            catch (System.Xml.XPath.XPathException pathException)
            {
                throw new MyException(pathException.Message);
            }
            catch (System.Xml.XmlException xmlException)
            {
                throw new MyException(xmlException.Message);
            }
            catch (MyException ex)
            {
                MessageBox.Show(ex.ToString());
                LogManager.InfoLog.LogConfigurationError(filePath.Name, "线路数据配置文件", ex.ToString());
            }
            ///日志观察者订阅数据变化主题
            logObs.AddProcess(VobcContainer.dataModify);
            LogManager.InfoLog.LogConfigurationInfo(filePath.Name, "线路数据配置文件", "加载成功");
            return true;
        }
        public static readonly CDeviceDataFactory Instance = new CDeviceDataFactory();///定义加载设备配置数据单例
        public VOBCProduct GetProductByIpPort(string ip, int port)
        {
            foreach (VOBCProduct pro in VobcContainer)
            {
                if (pro.Ip == ip && pro.Port == port.ToString())
                {
                    return pro;
                }
            }
            return null;
        }
    }
}
