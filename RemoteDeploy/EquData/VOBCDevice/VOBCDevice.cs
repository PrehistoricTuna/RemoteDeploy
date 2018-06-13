﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;
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

        //执行部署的条件（子类中赋值）
        public bool deployExecCondition = false;

        //校验文件实体类
        public VobcCheckFile m_vobcCheckFile = new VobcCheckFile();

        //当前正在处理的VOBC子系统类型
        public vobcSystemType vobcSysType = vobcSystemType.INVALID;

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
            //if (CDeviceDataFactory.Instance.CurrentDeployProduct == null ||
            //    CDeviceDataFactory.Instance.CurrentDeployProduct.ProductID != this.BelongProduct.ProductID)
            if(vobc.InProcess == false && vobc.SkipFlag == false)
            {
              
                ///清空烧录过程状态
                vobc.ResetDeviceProcState();
                CDeviceDataFactory.Instance.VobcContainer.dataModify.Modify();

                //预检状态信息赋值
                vobc.DeployConfigCheck = configState;
                vobc.recvCheckCount = 0;

                //开始部署前清空更新成功文件计数
                UpdateSuccessFileCount = 0;
                #region 给予更新状态实例赋初始值--依据用户在界面中勾选的烧录文件类型

                DataAnalysis._updateFileState = new VOBCUpdateFileState();

                DataAnalysis._updateFileState.AtoBootCompleteFlag = configState.IsBootLoaderCheck;
                DataAnalysis._updateFileState.AtpBootCompleteFlag = configState.IsBootLoaderCheck;
                DataAnalysis._updateFileState.ComBootCompleteFlag = configState.IsBootLoaderCheck;
                DataAnalysis._updateFileState.MmiBootCompleteFlag = configState.IsBootLoaderCheck;
                DataAnalysis._updateFileState.CcovBootCompleteFlag = configState.IsBootLoaderCheck;

                DataAnalysis._updateFileState.AtoCoreCompleteFlag = configState.IsCoreCheck;
                DataAnalysis._updateFileState.AtpCoreCompleteFlag = configState.IsCoreCheck;
                DataAnalysis._updateFileState.ComCoreCompleteFlag = configState.IsCoreCheck;
                DataAnalysis._updateFileState.MmiCoreCompleteFlag = configState.IsCoreCheck;
                DataAnalysis._updateFileState.CcovCoreCompleteFlag = configState.IsCoreCheck;

                DataAnalysis._updateFileState.AtoDataCompleteFlag = configState.IsDataCheck;
                DataAnalysis._updateFileState.AtpDataCompleteFlag = configState.IsDataCheck;
                //DataAnalysis._updateFileState.MmiDataCompleteFlag = configState.IsDataCheck;
                DataAnalysis._updateFileState.CcovDataCompleteFlag = configState.IsDataCheck;

                DataAnalysis._updateFileState.AtoNvramCompleteFlag = configState.IsNvRamCheck;
                DataAnalysis._updateFileState.AtpNvramCompleteFlag = configState.IsNvRamCheck;
                DataAnalysis._updateFileState.MmiNvramCompleteFlag = configState.IsNvRamCheck;
                //DataAnalysis._updateFileState.CcovNvramCompleteFlag = configState.IsNvRamCheck;

                DataAnalysis._updateFileState.CcovConfigCompleteFlag = configState.IsIniCheck;

                #endregion

                //设备预检
                //string preCheckResult = String.Empty;//Modified @ 4.28，待改回
                string preCheckResult = PreCheck(vobc.VobcStateInfo);

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
                    vobc.Report.ReportWindow("VOBC设备" + vobc.ProductID + "更新失败！");
                }
                else
                {
                    //记录日志
                    LogManager.InfoLog.LogProcInfo("VOBCDevice", "RunDeploy", "开始VOBC产品：" + base.BelongProduct.ProductID + "部署过程");

                    //设置跳过标志为false  后续代码根据此标志 执行具体部署
                    vobc.SkipFlag = false;
                    
                    //生成配置文件
                    BelongProduct.GenConfig();
                    vobc.InProcess = true;
                }
                

            }

            //如果跳过标志为假，执行部署
            if ((!vobc.SkipFlag) && (vobc.StepOne))
            {
                vobc.InProcess = true;
                CDeviceDataFactory.Instance.VobcContainer.dataModify.Color();
                DeployExec();
                ////该设备部署过程处理失败，跳出标志位真
                if(vobc.SkipFlag == true)
                {
                    ///当前处于部署之中
                    if (vobc.InProcess == true)
                    {
                        //刷新界面日志信息
                        vobc.Report.ReportWindow("VOBC设备" + vobc.ProductID + "更新失败：执行过程中出现失败环节");
                        vobc.InProcess = false;
                    } 
                    ///通知刷新背景色
                    CDeviceDataFactory.Instance.VobcContainer.dataModify.Color();
                }
            }
            else if(vobc.StepOne)
            {
                LogManager.InfoLog.LogProcInfo("VOBCDevice", "RunDeploy", base.BelongProduct.ProductID + "部署过程失败：生成配置文件前的相关环节失败或未收到允许上传的回复");
                vobc.Report.ReportWindow("更新失败：生成配置文件前的相关环节失败或未收到允许上传的回复");
                CDeviceDataFactory.Instance.VobcContainer.dataModify.Color();
            }
            else
            {
                LogManager.InfoLog.LogProcInfo("VOBCDevice", "RunDeploy", base.BelongProduct.ProductID + "部署第二阶段再次进入RunDeploy事件记录");
            }
        }

        /// <summary>
        /// VOBC设备预检
        /// </summary>
        /// <param name="vobc">VOBC产品车载状态信息</param>
        /// <returns>预检失败原因明细</returns>
        private string PreCheck(VOBCStateInfoClass vInfo)
        {
            string failReason = String.Empty;

            //未进行状态获取操作
            if (null == vInfo)
            {
                failReason = "未完成‘状态获取’操作";
            }
            //零速
            else if (vInfo.OperationSpeed != 0)
            {
                failReason = "预检车辆非0速";
            }
            //车辆位置
            else if (vInfo.TrainPosition != "非正线")
            {
                failReason = "预检车辆位置：" + vInfo.TrainPosition;
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
            //COM状态
            else if (vInfo.ComStatus != "正常")
            {
                failReason = "预检COM状态为：" + vInfo.ComStatus;
            }
            else
            {
                //TODO
            }

            return failReason;
        } 
        #endregion

        #region 虚方法实现

        /// <summary>
        /// VOBC子子系统设备发送部署文件的虚方法
        /// </summary>
        public virtual bool SendFile(VOBCProduct vobc)
        {
            //执行结果
            bool excuteResult = true;

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
                    LogManager.InfoLog.LogProcInfo("VOBCATODevice", "SendFile", "已收到文件请求回复，发送VOBC产品：" + base.BelongProduct.ProductID + "的ATO部署文件");

                    //界面信息打印
                    vobc.Report.ReportWindow("VOBC设备" + vobc.ProductID + " 子子系统设备:" + DeviceType + "正在发送部署文件......");

                    //创建校验文件数据帧
                    VOBCCommand sendFileCommand = new VOBCCommand(vobc.Ip,
                        Convert.ToInt32(vobc.Port), vobc.ProductID,
                        vobcCommandType.sendFile, m_vobcCheckFile);
                    CommandQueue.instance.m_CommandQueue.Enqueue(sendFileCommand);

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
            bool excuteResult = true;

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

            //ATO子子系统的发送和接收校验状态
            if (!deployExecCondition)
            {
                //记录日志
                LogManager.InfoLog.LogProcInfo(this.GetType().Name, "DeployExec", "开始VOBC产品：" + base.BelongProduct.ProductID + "的子子系统：" + base.DeviceType + "部署过程");

                //发送文件
                if(!SendFile(vobc))
                {
                    LogManager.InfoLog.LogProcError(this.GetType().Name, "DeployExec", "VOBC产品：" + base.BelongProduct.ProductID + "的子子系统：" + base.DeviceType + "发送文件超时，部署失败！");
                    vobc.Report.ReportWindow("VOBC产品：" + base.BelongProduct.ProductID + "的子子系统：" + base.DeviceType + "发送文件超时，部署失败！");
                    return;
                }

                //校验文件
                if(!CheckFile(vobc))
                {
                    LogManager.InfoLog.LogProcError(this.GetType().Name, "DeployExec", "VOBC产品：" + base.BelongProduct.ProductID + "的子子系统：" + base.DeviceType + "校验文件超时，部署失败！");
                    vobc.Report.ReportWindow( "VOBC产品：" + base.BelongProduct.ProductID + "的子子系统：" + base.DeviceType + "校验文件超时，部署失败！");
                    return;
                }

                //检查VOBC产品是否全部发送和校验
                if (vobc.IsFileChecked())
                {
                    //记录日志
                    LogManager.InfoLog.LogProcInfo(this.GetType().Name, "DeployExec", "VOBC设备" + base.BelongProduct.ProductID + "所有文件校验通过，开始更新过程");

                    //刷新界面显示内容
                    vobc.Report.ReportWindow("VOBC设备" + vobc.ProductID + "部署文件校验成功！");

                    //执行文件更新流程
                    if (!BelongProduct.FileUpdate())
                    {
                        vobc.SkipFlag = true;
                    }

                }
                else
                {
                    ///校验不通过次数+1
                    vobc.recvCheckCount++;
                }
                if(vobc.recvCheckCount==vobc.CSelectedDeviceType.Count)
                {
                    ///接收到全部device的校验回复，依然失败，校验失败
                    vobc.Report.ReportWindow("VOBC设备" + vobc.ProductID + "部署文件校验失败！");
                    vobc.SkipFlag = true;    
                }

            }
            else 
            {
              //TODO
            }
        } 

        #endregion

    }
}
