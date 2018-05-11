using RemoteDeploy.Command;
using RemoteDeploy.Common;
using RemoteDeploy.ControlDispatcher;
using RemoteDeploy.Models.VOBC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TCT.ShareLib.LogManager;

namespace RemoteDeploy.EquData
{
    /// <summary>
    /// VOBCMMI设备类
    /// </summary>
    public class VOBCMMIDevice:VOBCDevice
    {
        /// <summary>
        /// VOBCMMI设备构造器
        /// </summary>
        /// <param name="product">该设备所属的产品</param>
        public VOBCMMIDevice(IProduct product)
            : base(product)
        {
            LogManager.InfoLog.LogProcInfo("VOBCMMIDevice", "VOBCMMIDevice", "创建VOBC产品" + product.ProductID + "MMI设备");
        }

        #region 成员变量

        private KeyValuePair<string,string> m_mmiBootRomPath =new KeyValuePair<string, string>( System.Windows.Forms.Application.StartupPath + CShareLib.VOBC_GEN_FILEPATH + "\\MMI\\mmi_bootloader","/data/vau/mmi");
        //private KeyValuePair<string, string> m_mmiDataPath = new KeyValuePair<string, string>(System.Windows.Forms.Application.StartupPath + CShareLib.VOBC_GEN_FILEPATH + "\\MMI\\mmi.fs", "/data/vau/mmi");
        private KeyValuePair<string, string> m_mmiCorePath = new KeyValuePair<string, string>(System.Windows.Forms.Application.StartupPath + CShareLib.VOBC_GEN_FILEPATH + "\\MMI\\mmi_linux", "/data/vau/mmi");
        private KeyValuePair<string, string> m_mmiConfigPath = new KeyValuePair<string, string>(System.Windows.Forms.Application.StartupPath + CShareLib.VOBC_GEN_FILEPATH + "\\MMI\\MMIConfig.xml", "/data/vau/mmi");
       
        #endregion

        /// <summary>
        /// VOBC产品MMI设备发送文件方法
        /// </summary>
        public override bool SendFile(VOBCProduct vobc)
        {

            //文件路径键值信息
            Dictionary<string, string> filePathList = new Dictionary<string, string>();

            //待烧录文件类型列表
            List<vobcFileType> fileTypeList = new List<vobcFileType>();

            //获取烧录文件配置
            DeployConfiState configState = vobc.DeployConfigCheck;

            //检查引导文件勾选状态
            if (configState.IsBootLoaderCheck)
            {
                filePathList.Add(m_mmiBootRomPath.Key, m_mmiBootRomPath.Value);
                fileTypeList.Add(vobcFileType.BootLoader);
            }
            //检查内核文件勾选状态
            if (configState.IsCoreCheck)
            {
                filePathList.Add(m_mmiCorePath.Key, m_mmiCorePath.Value);
                fileTypeList.Add(vobcFileType.CORE);
            }
            //检查数据文件勾选状态
            /*if (configState.IsDataCheck)
            {
                filePathList.Add(m_mmiDataPath.Key, m_mmiDataPath.Value);
                fileTypeList.Add(vobcFileType.DATA);
            }*/
            //检查配置文件勾选状态
            if (configState.IsNvRamCheck)
            {
                filePathList.Add(m_mmiConfigPath.Key, m_mmiConfigPath.Value);
                fileTypeList.Add(vobcFileType.NVRAM);
            }

            ///构造VOBC产品ATO设备的checkfile
            m_vobcCheckFile.vobcSystemType = vobcSystemType.MMI;
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
        /// 执行VOBC产品MMI设备的部署过程
        /// </summary>
        public override void DeployExec()
        {
            //设置子子系统的执行条件
            base.deployExecCondition = (base.BelongProduct as VOBCProduct).SentCheckState.m_mmiSent;

            //当前正在处理的子系统类型
            base.vobcSysType = vobcSystemType.MMI;

            //执行父类中的部署执行函数
            base.DeployExec();
        }
    }
}
