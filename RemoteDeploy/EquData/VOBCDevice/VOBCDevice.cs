using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.IO;
using TCT.ShareLib.LogManager;
using RemoteDeploy.Command;
using RemoteDeploy.Common;
using RemoteDeploy.ControlDispatcher;
using System.Threading;
using RemoteDeploy.Models.VOBC;
using RemoteDeploy.DataPack;

namespace RemoteDeploy.EquData
{

    /// <summary>
    /// VOBC子子系统设备类，继承子子系统设备接口类
    /// </summary>
    public class VOBCDevice : IDevice
    {
        #region 变量

        //计数  非正常情况跳出循环
        private int skipCountMax = 30;

        //校验文件实体类
        public VobcCheckFile m_vobcCheckFile = new VobcCheckFile();

        //当前正在处理的VOBC子系统类型
        public vobcSystemType vobcSysType = vobcSystemType.INVALID;

        public int UpdateSuccessFileCount;

        #endregion

        #region 构造函数

        /// <summary>
        /// VOBC产品子子系统设备类构造函数
        /// </summary>
        /// <param name="product">所属产品接口实体作为参数</param>
        public VOBCDevice(IProduct product)
            : base(product)
        {

        }

        #endregion

        #region 功能函数

        /// <summary>
        /// VOBC产品子子系统设备加载数据实现方法
        /// </summary>
        /// <param name="xmlNode"></param>
        public override void LoadXml(XmlNode xmlNode)
        {
            m_deviceType = xmlNode.Attributes[CShareLib.XML_DEVICE_TYPE].InnerText;
            m_deviceName = xmlNode.Attributes[CShareLib.XML_DEVICE_NAME].InnerText;
            m_deviceState.State = xmlNode.Attributes[CShareLib.XML_DEVICE_STATE].InnerText;
        }

        /// <summary>
        /// VOBC产品子子系统设备执行部署实现方法
        /// </summary>
        /// <param name="configState">预检状态信息</param>
        public override void RunDeploy(DeployConfiState configState)
        {
            //获取当前正在处理的vobc产品类对象
            VOBCProduct vobc = base.BelongProduct as VOBCProduct;

            //当前为VOBC产品第一个烧录设备  执行产品烧录前的准备过程
            if (vobc.InProcess == false && vobc.SkipFlag == false)
            {

                ///清空烧录过程状态
                vobc.ResetDeviceProcState();

                //通知界面刷新
                CDeviceDataFactory.Instance.VobcContainer.dataModify.Modify();

                //预检状态信息赋值
                vobc.DeployConfigCheck = configState;
                vobc.recvCheckCount = 0;

                //开始部署前清空更新成功文件计数
                UpdateSuccessFileCount = 0;

                #region 给予更新状态实例赋初始值--依据用户在界面中勾选的烧录文件类型
                vobc._updateFileState = new VOBCUpdateFileState();

                vobc._updateFileState.AtoBootCompleteFlag = configState.IsBootLoaderCheck;
                vobc._updateFileState.AtpBootCompleteFlag = configState.IsBootLoaderCheck;
                vobc._updateFileState.ComBootCompleteFlag = configState.IsBootLoaderCheck;
                vobc._updateFileState.MmiBootCompleteFlag = configState.IsBootLoaderCheck;
                vobc._updateFileState.CcovBootCompleteFlag = configState.IsBootLoaderCheck;

                vobc._updateFileState.AtoCoreCompleteFlag = configState.IsCoreCheck;
                vobc._updateFileState.AtpCoreCompleteFlag = configState.IsCoreCheck;
                vobc._updateFileState.ComCoreCompleteFlag = configState.IsCoreCheck;
                vobc._updateFileState.MmiCoreCompleteFlag = configState.IsCoreCheck;
                vobc._updateFileState.CcovCoreCompleteFlag = configState.IsCoreCheck;

                vobc._updateFileState.AtoDataCompleteFlag = configState.IsDataCheck;
                vobc._updateFileState.AtpDataCompleteFlag = configState.IsDataCheck;
                vobc._updateFileState.CcovDataCompleteFlag = configState.IsDataCheck;

                vobc._updateFileState.AtoNvramCompleteFlag = configState.IsNvRamCheck;
                vobc._updateFileState.AtpNvramCompleteFlag = configState.IsNvRamCheck;
                vobc._updateFileState.MmiNvramCompleteFlag = configState.IsNvRamCheck;

                vobc._updateFileState.CcovConfigCompleteFlag = configState.IsIniCheck;

                #endregion
                //获取对端状态
                VOBCProduct oppovobc = CDeviceDataFactory.Instance.GetOppositeProductByIDEnd(vobc.ProductID);
                //设备预检
                //string preCheckResult = PreCheck(vobc.VobcStateInfo, oppovobc.VobcStateInfo);
                string preCheckResult = String.Empty;

                //预检结论等于空即预检成功  不等于空 预检失败
                if (preCheckResult != String.Empty)
                {

                    //设置VOBC产品下的子子系统预检状态
                    foreach (IDevice device in vobc.CBelongsDevice)
                    {
                        device.PreCheckResult = false;
                        device.PreCheckFailReason = preCheckResult;
                    }

                    //刷新界面
                    CDeviceDataFactory.Instance.VobcContainer.dataModify.Modify();

                    //记录警告日志
                    LogManager.InfoLog.LogProcWarning("VOBCDevice", "RunDeploy", "VOBC产品：" + base.BelongProduct.ProductID + "预检失败，原因" + preCheckResult);
                    LogManager.InfoLog.LogProcWarning("VOBCDevice", "RunDeploy", "VOBC产品：" + base.BelongProduct.ProductID + "部署未执行，部署进程结束");

                    vobc.SkipFlag = true;
                    vobc.InProcess = false;
                    vobc.Report.ReportWindow("VOBC" + vobc.ProductID + "更新失败！请检查是否满足预检通过条件并重新开始部署");
                    ///通知刷新背景色
                    CDeviceDataFactory.Instance.VobcContainer.dataModify.Color();
                }
                else
                {
                    //设置VOBC产品下的子子系统预检状态
                    foreach (IDevice device in vobc.CBelongsDevice)
                    {
                        device.PreCheckResult = true;
                        device.PreCheckFailReason = preCheckResult;
                    }

                    //记录日志
                    LogManager.InfoLog.LogProcInfo("VOBCDevice", "RunDeploy", "开始VOBC产品：" + base.BelongProduct.ProductID + "部署过程");

                    //设置跳过标志为false  后续代码根据此标志 执行具体部署
                    vobc.SkipFlag = false;

                    //生成配置文件
                    BelongProduct.GenConfig();

                    //当前处于部署阶段
                    vobc.InProcess = true;
                }

                ////用完清空VobcStateInfo（有误）
                //vobc.VobcStateInfo = null;
                //oppovobc.VobcStateInfo = null;
            }

            //如果跳过标志为假且当前处在部署第一阶段，执行部署
            if ((!vobc.SkipFlag) && (vobc.StepOne))
            {
                vobc.InProcess = true;
                CDeviceDataFactory.Instance.VobcContainer.dataModify.Color();

                //执行部署
                DeployExec();

                //若执行部署过程中 跳出标志被置为true 
                if (vobc.SkipFlag == true)
                {
                    ///当前处于部署之中
                    if (vobc.InProcess == true)
                    {
                        //刷新界面日志信息
                        vobc.Report.ReportWindow("VOBC" + vobc.ProductID + "更新失败：执行过程中出现失败环节，请重新开始部署");
                        vobc.InProcess = false;
                    }

                    ///通知刷新背景色
                    CDeviceDataFactory.Instance.VobcContainer.dataModify.Color();

                }
            }
            //当前处于第一阶段但是跳过标志为true，则更新失败
            else if (vobc.StepOne)
            {
                LogManager.InfoLog.LogProcInfo("VOBCDevice", "RunDeploy", base.BelongProduct.ProductID + "部署过程失败：生成配置文件前的相关环节失败或未收到允许上传的回复");
                //vobc.Report.ReportWindow("更新失败！（生成配置文件前的相关环节失败或未收到允许上传的回复）");
                //刷新界面
                CDeviceDataFactory.Instance.VobcContainer.dataModify.Modify();
                CDeviceDataFactory.Instance.VobcContainer.dataModify.Color();
            }
            //当前处于第二阶段，任何在第二阶段运行至此处的设备不予理会，第一阶段结束后则证明更新标志为已下发
            else
            {

            }
        }

        /// <summary>
        /// VOBC设备预检
        /// </summary>
        /// <param name="vobc">VOBC产品车载状态信息</param>
        /// <returns>预检失败原因明细</returns>
        private string PreCheck(VOBCStateInfoClass vInfo, VOBCStateInfoClass oppovInfo)
        {

            string failReason = String.Empty;

            //未进行状态获取操作
            if (null == vInfo)
            {
                failReason = "未完成‘状态获取’操作或未获取到状态";
            }
            else if (null == oppovInfo)
            {
                failReason = "未完成对端‘状态获取’操作或未获取到对端状态";
            }
            //ATP状态
            else if (vInfo.AtpStatus != "正常")
            {
                failReason = "预检ATP状态为：" + vInfo.AtpStatus;
            }
            //ATO状态
            else if (vInfo.AtoStatus != "正常")
            {
                failReason = "预检ATO状态为：" + vInfo.AtoStatus;
            }
            //MMI状态
            else if (vInfo.MmiStatus != "正常")
            {
                failReason = "预检MMI状态为：" + vInfo.MmiStatus;
            }
            //CCOV状态
            else if (vInfo.AtpTftpStatus != "正常")
            {
                failReason = "预检CCOV状态为：" + vInfo.AtpTftpStatus;
            }
            else if (vInfo.PreResult != true)
            {
                //通控给出的总预检结果                
                failReason = "本端预检失败";

                //零速
                if (vInfo.IsSteady != true)
                {
                    failReason = "预检车辆非0速";
                }
                //车辆位置
                else if (vInfo.TrainPosition != "无位置")
                {
                    failReason = "预检车辆：" + vInfo.TrainPosition;
                }
                else
                {
                    //TODO
                }
            }
            else if (oppovInfo.PreResult != true)
            {
                //对端通控给出的总预检结果                
                failReason = "对端预检失败";
            }
            else
            {
                //什么都不做
            }
            return failReason;
        }

        /// <summary>
        /// VOBC待烧录设备是否存在检查
        /// </summary>
        /// <param name="vobc">要校验的文件集合</param>
        /// <returns>成功/失败</returns>
        private List<string> FilePreCheck(VOBCProduct vobc)
        {
            return null;
        }
        #endregion

        #region 虚方法实现

        /// <summary>
        /// 获取待发送文件列表并校验文件是否存在
        /// </summary>
        /// <param name="vobc">VOBC产品对象类</param>
        /// <returns>执行结果  成功or失败</returns>
        public virtual bool GetFileListAndCheckExist(VOBCProduct vobc)
        {
            return false;
        }

        /// <summary>
        /// VOBC子子系统设备发送部署文件的虚方法
        /// </summary>
        public virtual bool SendFile(VOBCProduct vobc)
        {
            //执行结果
            bool excuteResult = false;

            //计数  非正常情况跳出循环
            int skipCount = 0;

            //do while循环正常执行跳出的条件
            bool rev = false;

            do
            {
                ///如果文件状态为true
                if (vobc.FileState)
                {
                    //日志记录
                    LogManager.InfoLog.LogProcInfo("VOBCATODevice", "SendFile", "已收到文件请求回复，发送VOBC产品：" + base.BelongProduct.ProductID + "的部署文件");

                    //界面信息打印
                    vobc.Report.ReportWindow("VOBC" + vobc.ProductID + " 子子系统设备:" + DeviceType + "正在发送部署文件......");

                    //创建发送文件数据帧
                    VOBCCommand sendFileCommand = new VOBCCommand(vobc.Ip,
                        Convert.ToInt32(vobc.Port), vobc.ProductID,
                        vobcCommandType.sendFile, m_vobcCheckFile);

                    //添加命令到队列中
                    CommandQueue.instance.m_CommandQueue.Enqueue(sendFileCommand);

                    excuteResult = true;

                    //执行成功 修改标志
                    rev = true;

                }
                else
                {
                    //计数15次 跳出循环
                    if (skipCount > skipCountMax)
                    {
                        vobc.SkipFlag = true;
                        excuteResult = false;
                        break;
                    }
                    else
                    {
                        //跳出计数+1
                        skipCount++;

                        //休眠1秒
                        Thread.Sleep(1000);
                    }

                }


            } while (rev == false);


            return excuteResult;
        }

        /// <summary>
        /// VOBC子子系统设备校验文件的虚方法
        /// </summary>
        public virtual bool CheckFile(VOBCProduct vobc)
        {
            //执行结果
            bool excuteResult = false;

            //文件校验请求日志记录
            LogManager.InfoLog.LogProcInfo(this.GetType().Name, "CheckFile", "发送VOBC产品：" + base.BelongProduct.ProductID + "的部署文件校验信息请求");

            //发送校验请求
            VOBCCommand checkCommand = new VOBCCommand(vobc.Ip,
                        Convert.ToInt32(vobc.Port), vobc.ProductID,
                        vobcCommandType.checkFile, m_vobcCheckFile);
            CommandQueue.instance.m_CommandQueue.Enqueue(checkCommand);

            //计数  非正常情况跳出循环
            int skipCount = 0;

            //do while循环正常执行跳出的条件
            bool rev = false;

            do
            {

                ///如果接收校验状态为true
                if (vobc.CheckState)
                {
                    //记录日志
                    LogManager.InfoLog.LogProcInfo(this.GetType().Name, "CheckFile", "已收到VOBC产品：" + base.BelongProduct.ProductID + "的部署文件校验回复");

                    //设置处理标志为true
                    rev = true;

                    //重置接收校验状态为false
                    vobc.CheckState = false;

                    //根据子子系统类型 设置相应帧标志
                    switch (vobcSysType)
                    {
                        case vobcSystemType.ATP_1:
                            vobc.SentCheckState.m_atpSent = true;
                            break;
                        case vobcSystemType.ATO_1:
                            vobc.SentCheckState.m_atoSent = true;
                            break;
                        case vobcSystemType.CCOV:
                            vobc.SentCheckState.m_ccovSent = true;
                            break;
                        case vobcSystemType.COM_1:
                            vobc.SentCheckState.m_comSent = true;
                            break;
                        case vobcSystemType.MMI:
                            vobc.SentCheckState.m_mmiSent = true;
                            break;
                        default:
                            //TODO
                            break;
                    }

                    excuteResult = true;
                }
                else
                {
                    //计数15次 未收到校验通过更新就跳出循环结束
                    if (skipCount > 60)
                    {
                        vobc.SkipFlag = true;
                        excuteResult = false;
                        break;
                    }
                    else
                    {
                        //跳出计数+1
                        skipCount++;

                        //休眠1秒
                        Thread.Sleep(1000);
                    }

                }
            } while (rev == false);

            return excuteResult;
        }

        /// <summary>
        /// VOBC子子系统设备部署执行（更新复位）执行虚方法
        /// </summary>
        public virtual void DeployExec()
        {

            //获取VOBC产品实例
            VOBCProduct vobc = base.BelongProduct as VOBCProduct;

            //验证需要发送的文件是否存在(返回true 则代表文件检查成功且已发送了文件传输请求)
            if (GetFileListAndCheckExist(vobc))
            {

                ////记录日志
                //LogManager.InfoLog.LogProcInfo(this.GetType().Name, "DeployExec", "开始VOBC产品：" + base.BelongProduct.ProductID + "的子子系统：" + base.DeviceType + "部署过程");

                //发送文件
                if (!SendFile(vobc))
                {
                    LogManager.InfoLog.LogProcError(this.GetType().Name, "DeployExec", "VOBC产品：" + base.BelongProduct.ProductID + "的子子系统：" + base.DeviceType + "发送文件超时，部署失败！");
                    vobc.Report.ReportWindow("VOBC产品：" + base.BelongProduct.ProductID + "的子子系统：" + base.DeviceType + "发送文件超时，部署失败！");
                    vobc.SkipFlag = true;
                    vobc.InProcess = false;
                    CDeviceDataFactory.Instance.VobcContainer.SetProductDeviceState(vobc.Ip, Convert.ToInt32(vobc.Port), "发送失败");
                    CDeviceDataFactory.Instance.VobcContainer.SetProductState(vobc.Ip, Convert.ToInt32(vobc.Port), "更新失败");
                    return;
                }

                //执行文件校验操作
                if (!CheckFile(vobc))
                {
                    LogManager.InfoLog.LogProcError(this.GetType().Name, "DeployExec", "VOBC产品：" + base.BelongProduct.ProductID + "的子子系统：" + base.DeviceType + "校验文件超时或未通过，部署失败！");
                    vobc.Report.ReportWindow("VOBC产品：" + base.BelongProduct.ProductID + "的子子系统：" + base.DeviceType + "校验文件超时或未通过，部署失败！");
                    vobc.SkipFlag = true;
                    vobc.InProcess = false;
                    CDeviceDataFactory.Instance.VobcContainer.SetProductDeviceState(vobc.Ip, Convert.ToInt32(vobc.Port),"校验失败");
                    CDeviceDataFactory.Instance.VobcContainer.SetProductState(vobc.Ip,Convert.ToInt32(vobc.Port),"更新失败");
                    return;
                }

                //检查VOBC产品是否全部发送和校验
                if (vobc.IsFileChecked())
                {
                    //记录日志
                    LogManager.InfoLog.LogProcInfo(this.GetType().Name, "DeployExec", "VOBC" + base.BelongProduct.ProductID + "所有文件校验通过，开始更新过程");

                    //刷新界面显示内容
                    vobc.Report.ReportWindow("VOBC" + vobc.ProductID + "部署文件校验成功！");

                    //执行文件更新流程
                    if (!BelongProduct.FileUpdate())
                    {
                        vobc.SkipFlag = true;
                        vobc.InProcess = false;
                    }

                }
                else
                {
                    ///校验不通过次数+1
                    vobc.recvCheckCount++;
                }
                if (vobc.recvCheckCount == vobc.CSelectedDeviceType.Count)
                {
                    ///接收到全部device的校验回复，依然失败，校验失败
                    vobc.Report.ReportWindow("VOBC" + vobc.ProductID + "部署文件校验失败！");
                    vobc.SkipFlag = true;
                    vobc.InProcess = false;
                    CDeviceDataFactory.Instance.VobcContainer.SetProductFailReason(vobc.Ip, Convert.ToInt32(vobc.Port), "文件校验未通过");
                    CDeviceDataFactory.Instance.VobcContainer.SetProductState(vobc.Ip, Convert.ToInt32(vobc.Port), "更新失败");
                }
            }
            else
            {
                //暂时什么都不做
            }

        }

        #endregion

    }
}
