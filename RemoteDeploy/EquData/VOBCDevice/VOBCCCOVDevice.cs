using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteDeploy.Command;
using RemoteDeploy.ControlDispatcher;
using System.IO;
using RemoteDeploy.Common;
using RemoteDeploy.Models.VOBC;
using TCT.ShareLib.LogManager;

namespace RemoteDeploy.EquData
{
    public class VOBCCCOVDevice:VOBCDevice
    {
        public VOBCCCOVDevice(IProduct product)
            : base(product)
        {
            LogManager.InfoLog.LogProcInfo("VOBCCCOVDevice", "VOBCCCOVDevice", "创建VOBC产品" + product.ProductID + "CCOV设备");
        }

        #region 成员变量

        private KeyValuePair<string, string> m_ccovBootRomPath = new KeyValuePair<string, string>(System.Windows.Forms.Application.StartupPath + CShareLib.VOBC_GEN_FILEPATH + "\\CCOVRSOV\\bootrom.sys", "/data/vau/ccov");
        private KeyValuePair<string, string> m_ccovDataPath = new KeyValuePair<string, string>(System.Windows.Forms.Application.StartupPath + CShareLib.VOBC_GEN_FILEPATH + "\\CCOVRSOV\\ccov.fs", "/data/vau/ccov");
        private KeyValuePair<string, string> m_ccovCorePath = new KeyValuePair<string, string>(System.Windows.Forms.Application.StartupPath + CShareLib.VOBC_GEN_FILEPATH + "\\CCOVRSOV\\vxWorks", "/data/vau/ccov");
        //private KeyValuePair<string, string> m_ccovNvRamPath = new KeyValuePair<string, string>(System.Windows.Forms.Application.StartupPath + CShareLib.VOBC_GEN_FILEPATH + "\\CCOVRSOV\\ccov.ini", "/data/vau/ccov");
        private KeyValuePair<string, string> m_ccovIniPath = new KeyValuePair<string, string>(System.Windows.Forms.Application.StartupPath + CShareLib.VOBC_GEN_FILEPATH + "\\CCOVRSOV\\config.ini", "/data/vau/ccov");
        
        #endregion

        /// <summary>
        /// VOBC产品CCOV设备发送文件方法
        /// </summary>
        public override bool SendFile(VOBCProduct vobc)
        {

            if(base.BelongProduct.ProductID.Contains("TC1"))
            {
                //CCOV统一使用一个配置文件config.ini，将红/蓝网关IP信息和WGB设备厂商信息写在一起
                File.Copy(System.Windows.Forms.Application.StartupPath + CShareLib.VOBC_GEN_FILEPATH + "\\Red\\config.ini", m_ccovIniPath.Key, true);
            }
            else if (base.BelongProduct.ProductID.Contains("TC2"))
            {
                //CCOV统一使用一个配置文件config.ini，将红/蓝网关IP信息和WGB设备厂商信息写在一起
                File.Copy(System.Windows.Forms.Application.StartupPath + CShareLib.VOBC_GEN_FILEPATH + "\\Blue\\config.ini", m_ccovIniPath.Key, true);
            }
            else
            {
                throw new MyException("非法列车！");
            }

            //文件路径键值信息
            Dictionary<string, string> filePathList = new Dictionary<string, string>();

            //待烧录文件类型列表
            List<vobcFileType> fileTypeList = new List<vobcFileType>();

            //获取烧录文件配置
            DeployConfiState configState = vobc.DeployConfigCheck;

            //检查引导文件勾选状态
            if (configState.IsBootLoaderCheck)
            {
                filePathList.Add(m_ccovBootRomPath.Key, m_ccovBootRomPath.Value);
                fileTypeList.Add(vobcFileType.BootLoader);
            }
            //检查内核文件勾选状态
            if (configState.IsCoreCheck)
            {
                filePathList.Add(m_ccovCorePath.Key, m_ccovCorePath.Value);
                fileTypeList.Add(vobcFileType.CORE);
            }
            //检查数据文件勾选状态
            if (configState.IsDataCheck)
            {
                filePathList.Add(m_ccovDataPath.Key, m_ccovDataPath.Value);
                fileTypeList.Add(vobcFileType.DATA);
            }
            //检查配置文件勾选状态
            /*if (configState.IsNvRamCheck)
            {
                filePathList.Add(m_ccovNvRamPath.Key, m_ccovNvRamPath.Value);
                fileTypeList.Add(vobcFileType.NVRAM);
            }*/
            //检查ini配置文件勾选状态
            if (configState.IsIniCheck)
            {
                filePathList.Add(m_ccovIniPath.Key, m_ccovIniPath.Value);
                fileTypeList.Add(vobcFileType.CCOVConfig);
            }

            ///构造VOBC产品ATO设备的checkfile
            m_vobcCheckFile.vobcSystemType = vobcSystemType.CCOV;
            m_vobcCheckFile.vobcFilePathList = filePathList;
            m_vobcCheckFile.vobcFileTypeList = fileTypeList;
            vobc.CheckFileList.Add(m_vobcCheckFile);

            //执行父类中的发送文件函数
            return base.SendFile(vobc);
            
        }

        /// <summary>
        /// VOBC产品ATP设备校验文件方法
        /// </summary>
        public override bool CheckFile(VOBCProduct vobc)
        {
            return base.CheckFile(vobc);
        }

        /// <summary>
        /// 执行VOBC产品CCOV设备的部署过程
        /// </summary>
        public override void DeployExec()
        {
            //设置子子系统的执行条件
            base.deployExecCondition = (base.BelongProduct as VOBCProduct).SentCheckState.m_ccovSent;

            //当前正在处理的子系统类型
            base.vobcSysType = vobcSystemType.CCOV;

            //执行父类中的部署执行函数
            base.DeployExec();
        }
    }
}
