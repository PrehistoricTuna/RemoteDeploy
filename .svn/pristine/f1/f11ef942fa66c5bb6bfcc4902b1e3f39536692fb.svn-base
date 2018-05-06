using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using TCT.ShareLib.LogManager;

namespace RemoteDeploy.EquData
{
    /// <summary>
    /// 项目接口，如互联互通，新机场，不同项目加载数据不同
    /// </summary>
    public abstract class IProjectConsole
    {
        #region 成员变量
        private string m_name;
        private EmProductLine m_productLine = EmProductLine.NONE;
        private List<IProContainer> m_projducts = new List<IProContainer>();///包含的产品集合
        #endregion
        #region 属性
        /// <summary>
        /// 产品线类型
        /// </summary>
        public EmProductLine ProductLine
        {
            get { return m_productLine; }
            set { m_productLine = value; }
        }
        /// <summary>
        /// 线路名称
        /// </summary>
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }
        /// <summary>
        /// 包含的产品容器集合
        /// </summary>
        public List<IProContainer> Projducts
        {
            get { return m_projducts; }
        }
        #endregion
        

        /// <summary>
        /// 产品线数据加载
        /// </summary>
        /// <param name="nodeList"></param>
        public void LoadXml(XmlNodeList nodeList)
        {
            foreach (XmlNode node in nodeList)
            {
                string containerName = node.Attributes[CShareLib.XML_CONTAINER_NAME].InnerText;
                switch (containerName)
                {
                    case "VOBC":
                        VOBCContainer vobcContainer = new VOBCContainer();
                        if (m_projducts.Exists((IProContainer temp) => temp.Name == EmContainerType.VOBC))
                        {
                            throw new MyException("重复的VOBC产品实体！");
                        }
                        else
                        {
                            AddContainer(vobcContainer, node);
                        }
                        break;
                    case "ZC":
                        ZCContainer zcContainer = new ZCContainer();
                        if (m_projducts.Exists((IProContainer temp) => temp.Name == EmContainerType.ZC))
                        {
                            throw new MyException("重复的ZC产品实体！");
                        }
                        else
                        {
                            AddContainer(zcContainer, node);
                        }
                        break;
                    default:
                        throw new MyException("未知设备类型：" + containerName);
                }
                
            }
        }
        private void AddContainer(IProContainer container,XmlNode node)
        {
            container.LoadXml(node);
            m_projducts.Add(container);
        }
    }
}
