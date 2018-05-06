using RemoteDeploy.Command;
using RemoteDeploy.Common;
using RemoteDeploy.ControlDispatcher;
using RemoteDeploy.Models.VOBC;
using System;
using System.Collections.Generic;
using System.Threading;
using TCT.ShareLib.LogManager;

namespace RemoteDeploy.EquData
{
    public class VOBCATPDevice:VOBCDevice
    {
        public VOBCATPDevice(IProduct product)
            : base(product)
        {
            LogManager.InfoLog.LogProcInfo("VOBCATPDevice", "VOBCATPDevice", "创建VOBC产品" + product.ProductID + "ATP设备");
        }

        #region 成员变量

        private KeyValuePair<string,string> m_atpBootRomPath = new KeyValuePair<string,string>(System.Windows.Forms.Application.StartupPath + CShareLib.VOBC_GEN_FILEPATH + "\\ATP\\atp_bootloader.bin","/data/vau/atp");
        private KeyValuePair<string, string> m_atpDataPath = new KeyValuePair<string, string>(System.Windows.Forms.Application.StartupPath + CShareLib.VOBC_GEN_FILEPATH + "\\ATP\\atp.fs", "/data/vau/atp");
        private KeyValuePair<string, string> m_atpCorePath = new KeyValuePair<string, string>(System.Windows.Forms.Application.StartupPath + CShareLib.VOBC_GEN_FILEPATH + "\\ATP\\atp_core", "/data/vau/atp");
        private KeyValuePair<string, string> m_atpNvRamPath = new KeyValuePair<string, string>(System.Windows.Forms.Application.StartupPath + CShareLib.VOBC_GEN_FILEPATH + "\\ATP\\atp_nvram.txt", "/data/vau/atp");
        
        #endregion

        /// <summary>
        /// VOBC产品ATP设备发送文件方法
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
                filePathList.Add(m_atpBootRomPath.Key, m_atpBootRomPath.Value);
                fileTypeList.Add(vobcFileType.BootLoader);
            }
            //检查内核文件勾选状态
            if (configState.IsCoreCheck)
            {
                filePathList.Add(m_atpCorePath.Key, m_atpCorePath.Value);
                fileTypeList.Add(vobcFileType.CORE);
            }
            //检查数据文件勾选状态
            if (configState.IsDataCheck)
            {
                filePathList.Add(m_atpDataPath.Key, m_atpDataPath.Value);
                fileTypeList.Add(vobcFileType.DATA);
            }
            //检查配置文件勾选状态
            if (configState.IsNvRamCheck)
            {
                filePathList.Add(m_atpNvRamPath.Key, m_atpNvRamPath.Value);
                fileTypeList.Add(vobcFileType.NVRAM);
            }

            ///构造VOBC产品ATO设备的checkfile
            m_vobcCheckFile.vobcSystemType = vobcSystemType.ATP_1;
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
        /// 执行VOBC产品ATP设备的部署过程
        /// </summary>
        public override void DeployExec()
        {
            //设置子子系统的执行条件
            base.deployExecCondition = (base.BelongProduct as VOBCProduct).SentCheckState.m_atpSent;

            //当前正在处理的子系统类型
            base.vobcSysType = vobcSystemType.ATP_1;

            //执行父类中的部署执行函数
            base.DeployExec();
        }
    }
}
