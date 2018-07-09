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
    /// <summary>
    /// VOBCATP设备类
    /// </summary>
    public class VOBCATPDevice:VOBCDevice
    {
        /// <summary>
        /// VOBCATP设备的构造器
        /// </summary>
        /// <param name="product">该设备所属的产品</param>
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
        //执行部署的条件（子类中赋值）
        public bool deployExecCondition = false;

        #endregion


        /// <summary>
        /// 重写函数“获取待发送文件列表并校验文件是否存在”
        /// </summary>
        /// <param name="vobc">VOBC产品对象类</param>
        /// <returns>执行结果  成功or失败</returns>
        public override bool GetFileListAndCheckExist(VOBCProduct vobc)
        {
            //处理结果
            bool dealResult = true;

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



            //有文件需要发送 
            if (filePathList.Count > 0)
            {
                //文件是否全部真实存在 若有任何一个不存在 该子子系统将不进行烧录
                bool isFileAllExist = true;

                foreach (string filePath in filePathList.Keys)
                {
                    //文件存在
                    if (System.IO.File.Exists(filePath))
                    {
                        //文件存在 不执行操作
                    }
                    //文件不存在
                    else
                    {
                        //不是所有文件都存在
                        isFileAllExist = false;

                        //处理失败
                        dealResult = false;

                        //启用跳过标志 将不执行该子子系统的部署
                        vobc.SkipFlag = true;

                        //不在执行过程中
                        vobc.InProcess = false;

                        //日志信息
                        string logMsg = "VOBC设备" + vobc.ProductID + " 子子系统设备:" + DeviceType + "的待发送文件不存在，文件地址应为：" + filePath;

                        //记录日志
                        LogManager.InfoLog.LogCommunicationInfo("VOBCATPDevice", "GetFileListAndCheckExist", logMsg);

                        //界面信息打印
                        vobc.Report.ReportWindow(logMsg);
                    }
                }

                //如果文件都存在 数据添加到集合队列中
                if (isFileAllExist)
                {
                    ///构造VOBC产品ATP设备的checkfile
                    m_vobcCheckFile.vobcSystemType = vobcSystemType.ATP_1;
                    m_vobcCheckFile.vobcFilePathList = filePathList;
                    m_vobcCheckFile.vobcFileTypeList = fileTypeList;
                    vobc.CheckFileList.Add(m_vobcCheckFile);

                }
                else
                {
                    //无操作
                }

            }
            //没有文件需要发送
            else
            {
                //处理失败
                dealResult = false;

                //启用跳过标志 将不执行该子子系统的部署
                vobc.SkipFlag = true;

                //不在执行过程中
                vobc.InProcess = false;

                //日志信息
                string logMsg = "VOBC设备" + vobc.ProductID + " 子子系统设备:" + DeviceType + "没有文件需要发送，部署停止，请检查选择并重新开始部署！";

                //记录日志
                LogManager.InfoLog.LogCommunicationInfo("VOBCATPDevice", "GetFileListAndCheckExist", logMsg);

                //界面信息打印
                vobc.Report.ReportWindow(logMsg);

                ///通知刷新背景色
                CDeviceDataFactory.Instance.VobcContainer.dataModify.Color();
            }

            return dealResult;

        }

        /// <summary>
        /// VOBC产品ATP设备发送文件方法
        /// </summary>
        public override bool SendFile(VOBCProduct vobc)
        {

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
            deployExecCondition = (base.BelongProduct as VOBCProduct).SentCheckState.m_atpSent;

            //当前正在处理的子系统类型
            base.vobcSysType = vobcSystemType.ATP_1;

            if (!deployExecCondition)
            {
                //执行父类中的部署执行函数
                base.DeployExec();
            }
            else
            {
                ;//nothing
            }
        }
    }
}
