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
    /// VOBCCOM设备类
    /// </summary>
    public class VOBCCOMDevice:VOBCDevice
    {
        /// <summary>
        /// VOBCCOM设备的构造器
        /// </summary>
        /// <param name="product">VOBCCOM设备的所属产品</param>
        public VOBCCOMDevice(IProduct product)
            : base(product)
        {
            LogManager.InfoLog.LogProcInfo("VOBCCOMDevice", "VOBCCOMDevice", "创建VOBC产品" + product.ProductID + "COM设备");
        }

        #region 成员变量
  
        private KeyValuePair<string, string> m_comBootRomPath = new KeyValuePair<string, string>(System.Windows.Forms.Application.StartupPath + CShareLib.VOBC_GEN_FILEPATH + "\\COMM\\com_bootloader.bin", "/data/vau/com");
        private KeyValuePair<string, string> m_comCorePath = new KeyValuePair<string, string>(System.Windows.Forms.Application.StartupPath + CShareLib.VOBC_GEN_FILEPATH + "\\COMM\\com_core", "/data/vau/com");
        
        #endregion

        /// <summary>
        /// VOBC产品COM设备发送文件方法
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
                filePathList.Add(m_comBootRomPath.Key, m_comBootRomPath.Value);
                fileTypeList.Add(vobcFileType.BootLoader);
            }
            //检查内核文件勾选状态
            if (configState.IsCoreCheck)
            {
                filePathList.Add(m_comCorePath.Key, m_comCorePath.Value);
                fileTypeList.Add(vobcFileType.CORE);
            }

            ///构造VOBC产品ATO设备的checkfile
            m_vobcCheckFile.vobcSystemType = vobcSystemType.COM_1;
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
        /// 执行VOBC产品COM设备的部署过程
        /// </summary>
        public override void DeployExec()
        {
            //设置子子系统的执行条件
            base.deployExecCondition = (base.BelongProduct as VOBCProduct).SentCheckState.m_comSent;

            //当前正在处理的子系统类型
            base.vobcSysType = vobcSystemType.COM_1;

            //执行父类中的部署执行函数
            base.DeployExec();
        }
    }
}
