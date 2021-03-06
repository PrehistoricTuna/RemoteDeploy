﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteDeploy.EquData;
using System.Xml;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using RemoteDeploy.Command;
using RemoteDeploy.ControlDispatcher;
using RemoteDeploy.Common;
using RemoteDeploy.Models.VOBC;
using RemoteDeploy.DataPack;
using System.Threading;
using TCT.ShareLib.LogManager;
using RemoteDeploy.Observer;
using System.Data.OleDb;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Timers;

namespace RemoteDeploy.EquData
{
    /// <summary>
    /// VOBC产品的各子子系统的文件发送和校验完成情况，初始未完成
    /// </summary>
    public class VobcSentCheckState
    {
        public bool m_atpSent = false;
        public bool m_atoSent = false;
        public bool m_comSent = false;
        public bool m_mmiSent = false;
        public bool m_ccovSent = false;
    }

    /// <summary>
    /// VOBC产品类，继承产品抽象类接口
    /// </summary>
    public class VOBCProduct : IProduct
    {
        #region 成员变量

        //发送校验检查状态
        public VobcSentCheckState SentCheckState = new VobcSentCheckState();
        public int recvCheckCount = 0;

        //vobc文件烧录列表
        private List<VobcCheckFile> m_checkFileList = new List<VobcCheckFile>();

        public int skipCountMax = 30;

        private System.Timers.Timer timerHB = new System.Timers.Timer(15000);        

        public bool timerEnable = false;

        /// <summary>
        /// 更新请求标志位，用于DataPack打包
        /// </summary>
        public byte _atpUpdateFile;
        public byte _atoUpdateFile;
        public byte _ccovUpdateFile;
        public byte _mmiUpdateFile;
        public byte _comUpdateFile; 

        /// <summary>
        /// 更新文件状态存储类对象
        /// </summary>
        public VOBCUpdateFileState _updateFileState = new VOBCUpdateFileState();

        #endregion

        #region 属性

        /// <summary>
        /// VOBC产品的文件发送请求回复状态
        /// </summary>
        public bool FileState { get; set; }

        /// <summary>
        /// VOBC产品的文件校验结果回复状态
        /// </summary>
        public bool CheckState { get; set; }

        /// <summary>
        /// vobc文件烧录列表
        /// </summary>
        public List<VobcCheckFile> CheckFileList
        {
            get { return m_checkFileList; }
            set { m_checkFileList = value; }
        }

        /// <summary>
        /// vobc状态信息
        /// </summary>
        public VOBCStateInfoClass VobcStateInfo{get;set;}

        #endregion

        #region 系统自带的函数调用

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool WritePrivateProfileString(
            string lpAppName, string lpKeyName, string lpString, string lpFileName);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetPrivateProfileString(
            string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString,
            int nSize, string lpFileName);

        #endregion

        /// <summary>
        /// VOBC产品的数据加载
        /// </summary>
        /// <param name="xmlNode"></param>
        public override void LoadXml(XmlNode xmlNode)
        {
            m_productID = xmlNode.Attributes[CShareLib.XML_PRODUCTID].InnerText;
            m_productName = xmlNode.Attributes[CShareLib.XML_PRODUCTNAME].InnerText;
            m_productState = xmlNode.Attributes[CShareLib.XML_PRODUCT_STATE].InnerText;
            m_ip = xmlNode.Attributes[CShareLib.XML_PRODUCT_IP].InnerText;
            m_port = xmlNode.Attributes[CShareLib.XML_PRODUCT_PORT].InnerText;
            m_cProductLine = CDeviceDataFactory.Instance.ProjectConsole.ProductLine;
            ///遍历所有子节点，加载所属子子系统的数据
            foreach (XmlNode node in xmlNode.ChildNodes)
            {
                string type = node.Attributes[CShareLib.XML_DEVICE_TYPE].InnerText;
                VOBCDevice device = CreateVOBCDevice(type);
                if (device != null)
                {
                    device.LoadXml(node);
                    m_cBelongsDevice.Add(device);
                    LogManager.InfoLog.LogConfigurationInfo("线路数据", "VOBC产品设备数据", "完成加载VOBC" + this.ProductID + "的子子系统：" + device.DeviceName + "数据");
                }
                else
                {
                    LogManager.InfoLog.LogCommunicationError("MainWindow", "ProductReport","非法设备类型"+type);
                }
            }
        }

        /// <summary>
        /// 实现生成互联互通产品线配置文件的接口方法
        /// </summary>
        protected override void GenConfigHLHT()
        {
            try
            {

                //用户勾选了配置文件 才去生成配置文件
                if (DeployConfigCheck.IsIniCheck)
                {

                    //刷新界面上的日志信息
                    Report.ReportWindow("VOBC" + m_productID + "正在生成配置文件");

                    //当前正在处理的VOBC产品
                    LogManager.InfoLog.LogProcInfo("VOBCProduct", "GenConfigHLHT", "正在生成ATPATO配置文件......");

                    //分别生成ATP  ATO 的读写配置文件
                    GenATPATOConfig();

                    //生成CCOV配置文件
                    GenCCOV();

                    //刷新界面上的日志信息
                    Report.ReportWindow("VOBC" + m_productID + "配置文件生成完毕！");

                }
                else 
                {
                   //未勾选配置文件选项 不处理
                }

                //每次检测FileState前先进行重置
                FileState = false;

                //发送文件传输请求
                VOBCCommand requestCommasd = new VOBCCommand(m_ip, Convert.ToInt32(m_port), m_productID, vobcCommandType.fileTransRequest);
                CommandQueue.instance.m_CommandQueue.Enqueue(requestCommasd);

                ///重置子子系统文件发送校验状态
                SentCheckState = new VobcSentCheckState();

                //清空校验信息列表
                m_checkFileList.Clear();

                int skipCount = 0;
                while (FileState == false)
                {
                    if (skipCount > skipCountMax)
                    {
                        SkipFlag = true;
                        InProcess = false;
                        //StepOne = true;
                        skipCount = 0;
                        LogManager.InfoLog.LogProcInfo("VOBCProduct", "GenConfigHLHT", "未收到下位机允许上传回复超时或下位机拒绝上传文件");
                        Report.ReportWindow("VOBC" + m_productID + "未收到下位机允许上传回复超时或下位机拒绝上传文件");
                        CDeviceDataFactory.Instance.VobcContainer.SetProductState(Ip, Convert.ToInt32(Port), "更新失败");
                        //CDeviceDataFactory.Instance.VobcContainer.dataModify.Color();
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

            }
            catch (MyException e)
            {
                LogManager.InfoLog.LogCommunicationError("VOBCProduct", "GenConfigHLHT", e.Message);
                SkipFlag = true;
                InProcess = false;
                //StepOne = true;
                CDeviceDataFactory.Instance.VobcContainer.SetProductState(Ip, Convert.ToInt32(Port), "更新失败");
                CDeviceDataFactory.Instance.VobcContainer.dataModify.Color();
            }
        }

        /// <summary>
        /// 实现文件更新执行接口方法
        /// </summary>
        protected override bool FileUpdateExec()
        {
            bool result = true;
            Report.ReportWindow("VOBC" + m_productID + "更新标识已下发至通信控制器,请勿终止部署.如此阶段发生异常情况,请重启车载设备两次再重新开始部署!");
            LogManager.InfoLog.LogProcInfo("VOBCProduct", "FileUpdateExec", "发送VOBC产品" + m_productID + "的文件更新命令");
            VOBCCommand fileUpdateCommand = new VOBCCommand(m_ip, Convert.ToInt32(m_port), m_productID, vobcCommandType.startUpdateFile);
            CommandQueue.instance.m_CommandQueue.Enqueue(fileUpdateCommand);
            ///等待待重启状态
            bool rev = false;
            int skipCount = 0;
            do
            {
                ///如果文件状态为true
                if (ProductState == "待重启")
                {
                    LogManager.InfoLog.LogProcInfo("VOBCProduct", "FileUpdateExec", "VOBC产品" + m_productID + "处于待重启状态,发送循环建链,已结束部署第一阶段");
                    rev = true;
                    StepOne = false;
                }
                else
                {
                    //计数15次 未收到允许更新就跳出循环结束
                    if (skipCount > 30)
                    {
                        SkipFlag = true;
                        InProcess = false;
                        //StepOne = true;
                        skipCount = 0;
                        result = false;
                        Report.ReportWindow("未收到VOBC" + m_productID + "的允许更新回复超时");
                        //LogManager.InfoLog.LogProcInfo("VOBCProduct", "FileUpdateExec", "未收到VOBC产品" + m_productID + "的允许更新回复超时");
                        CDeviceDataFactory.Instance.VobcContainer.SetProductDeviceState(Ip, Convert.ToInt32(Port), "更新失败");
                        CDeviceDataFactory.Instance.VobcContainer.SetProductState(Ip, Convert.ToInt32(Port), "更新失败");

                        //Modified @ 7.25
                        //断开连接
                        CTcpClient.Socket_TCPClient_Dispose();
                        timerHB.Dispose();
                        CDeviceDataFactory.Instance.VobcContainer.dataModify.Color();
                        return result;
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

            //待重启状态后，等待80秒发送一次建链帧至命令队列
            //Thread.Sleep(80000);

            //从待重启状态开始pingVOBC的IP端口网络，直至ping不通则证明已关机
            bool online = true;
            Ping ping = new Ping();
            do
            {
                PingReply reply = ping.Send(m_ip);
                if (reply.Status != IPStatus.Success)
                {
                    online = false;
                    LogManager.InfoLog.LogProcInfo("VOBCProduct", "FileUpdateExec", "VOBC产品" + m_productID + "已关闭");
                }
                else
                {
                    Thread.Sleep(3000);
                }
            } while (online == true);

            //进入重连
            do
            {
                if (!StepOne)
                {
                    PingReply reply = ping.Send(m_ip);
                    if (reply.Status == IPStatus.Success)
                    {
                        online = true;
                        LogManager.InfoLog.LogCommunicationInfo("VOBCProduct", "FileUpdateExec", "二次建链ping通，等待5秒开始执行建链");
                        Report.ReportWindow("VOBC产品" + m_productID + "二次建链ping通，等待5秒开始执行建链");
                        Thread.Sleep(5000);
                    }
                    else
                    {
                        Thread.Sleep(3000);
                    }
                }
                else 
                {
                    result = false;
                    return result;
                }
            } while (online == false);

            VOBCCommand buildCommand = new VOBCCommand(m_ip, Convert.ToInt32(m_port), m_productID, vobcCommandType.rebuildLink);
            CommandQueue.instance.m_CommandQueue.Enqueue(buildCommand);
            skipCount = 0;
            rev = false;
            do
            {
                ///如果文件状态为true
                if (ProductState == "正常")
                {
                    Report.ReportWindow("VOBC产品" + m_productID + "已完成重连，开始执行第二阶段程序");
                    LogManager.InfoLog.LogProcInfo("VOBCProduct", "FileUpdateExec", "VOBC产品" + m_productID + "完成第二阶段重连");
                    rev = true;
                }
                else
                {
                    //重连倒计时
                    if (skipCount > 15)
                    {
                        //SkipFlag = true;
                        skipCount = 0;
                        //result = false;
                        //Report.ReportWindow("VOBC产品" + m_productID + "第二阶段网络重连超时！继续重连");
                        //CDeviceDataFactory.Instance.VobcContainer.SetProductState(Ip,Convert.ToInt32(Port), "重连失败");
                        //CDeviceDataFactory.Instance.VobcContainer.dataModify.Modify();
                        //return result;
                        for (int i = 0; i < 3; i++)
                        {
                            if (ProductState != "正常")
                            {
                                CommandQueue.instance.m_CommandQueue.Enqueue(new VOBCCommand(Ip, Convert.ToInt32(Port), ProductID, vobcCommandType.rebuildLink));
                                Thread.Sleep(15000);
                            }
                            else
                            {
                                Report.ReportWindow("VOBC产品" + m_productID + "已完成重连，开始执行第二阶段程序");
                                result = true;
                                return result;
                            }
                        }
                        if (ProductState != "正常")
                        {
                            Report.ReportWindow("VOBC产品" + m_productID + "第二阶段网络重连超时，更新失败！请重启车载设备两次再重新开始部署");
                            SkipFlag = true;
                            InProcess = false;
                            //Modified @ 7.24
                            StepOne = true;
                            result = false;
                            //Modified @ 9.10
                            CDeviceDataFactory.Instance.VobcContainer.SetProductDeviceState(Ip, Convert.ToInt32(Port), "更新失败");
                            CDeviceDataFactory.Instance.VobcContainer.SetProductState(Ip, Convert.ToInt32(Port), "更新失败");                           
                            return result;
                        }
                        else
                        {
                            Report.ReportWindow("VOBC产品" + m_productID + "已完成重连，开始执行第二阶段程序");
                            result = true;
                            return result;
                        }
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

            //记录日志
            //LogManager.InfoLog.LogProcInfo("VOBCProduct", "FileUpdateExec", "VOBC产品" + m_productID + "处于正常状态");

            ///进入下一个产品的部署过程之前，等待当前产品的更新状态回复结果
            //LogManager.InfoLog.LogProcInfo("VOBCProduct", "WaitForUpdateResult", "等待VOBC产品:" + m_productID + "的部署更新结果");
            ///WaitForUpdateResult();

            return result;
        }

        /// <summary>
        /// 创建VOBC设备工厂
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private VOBCDevice CreateVOBCDevice(string type)
        {
            //创建VOBC服务实例
            VOBCDevice device = null ;

            //依据数据的设备类型 返回不同的实体对象
            switch (type)
            {
                case "CCOV":
                    device = new VOBCCCOVDevice(this);
                    break;
                case "COM":
                    device = new VOBCCOMDevice(this);
                    break;
                case "ATP":
                    device = new VOBCATPDevice(this);
                    break;
                case "ATO":
                    device = new VOBCATODevice(this);
                    break;
                case "MMI":
                    device = new VOBCMMIDevice(this);
                    break;
                default:
                    device = new VOBCDevice(this);
                    break;
            }

            return device;

        }

        /// <summary>
        /// 检查VOBC产品是否全部发送和校验
        /// </summary>
        /// <returns></returns>
        public bool IsFileChecked()
        {
            
            //记录日志
            LogManager.InfoLog.LogProcInfo("VOBCProduct", "IsFileChecked", "检查VOBC产品：" + m_productID + "的部署文件是否全部发送和校验完成");
            
            //返回值定义
            bool rtnValue = true;

            //计算校验结果
            foreach (string type in base.CSelectedDeviceType)
            {
                rtnValue &= CheckVeriTypeFile(type);
            }

            //记录日志
            LogManager.InfoLog.LogProcInfo("VOBCProduct", "IsFileChecked", "VOBC产品" + m_productID + "的部署文件校验结果为" + rtnValue.ToString());
            
            return rtnValue;
        
        }

        /// <summary>
        /// 检查VOBC产品是否全部更新成功
        /// </summary>
        /// <returns></returns>
        //public bool IsFileUpdateResultChecked()
        //{
        //    //记录日志
        //    LogManager.InfoLog.LogProcInfo("VOBCProduct", "IsFileChecked", "检查VOBC产品：" + m_productID + "的部署文件是否全部更新成功");

        //    //返回值定义
        //    bool rtnValue = true;

        //    //计算校验结果
        //    foreach (string type in base.CSelectedDeviceType)
        //    {
        //        rtnValue &= CheckUpdateResultTypeFile(type);
        //    }

        //    //记录日志
        //    LogManager.InfoLog.LogProcInfo("VOBCProduct", "IsFileChecked", "VOBC产品" + m_productID + "的部署文件校验结果为" + rtnValue.ToString());

        //    return rtnValue;

        //}

        /// <summary>
        /// 等待VOBC子子系统烧录结果
        /// </summary>
        public override void WaitForUpdateResult()
        {
            
            //foreach(vobcSystemType type in GetVobcSystemListByType(vobcSystemType sType))
            //如有任一子设备更新失败则认为该产品全部更新失败，因此反向判断：回复的VOBC产品下的子子系统更新结果存在失败的，更新失败 Modified @ 4.25
            if (this.CSelectedDevice.FindAll(tar => tar.State == "更新失败").Count > 0)
            {
                //记录部署失败日志信息
                LogManager.InfoLog.LogProcError("VOBCProduct", "WaitForUpdateResult", "接收到VOBC产品:" + m_productID + "的部署更新失败消息");
                if(InProcess == true)
                {
                    //该产品已部署失败就停止并禁用心跳计时器
                    timerHB.Close();
                    timerEnable = false;

                    //刷新界面日志信息
                    Report.ReportWindow("VOBC" + m_productID + "更新失败！请重启车载设备重新开始部署");
                    SkipFlag = true;
                    InProcess = false;
                    //StepOne = true;
                    CDeviceDataFactory.Instance.VobcContainer.SetProductState(Ip, Convert.ToInt32(Port), "更新失败");
                    //CDeviceDataFactory.Instance.VobcContainer.dataModify.Color();
                    //CDeviceDataFactory.Instance.VobcContainer.dataModify.ColorEvent();
                    //DataModify.ColorEventHandler

                    //断开TCP连接并释放资源 Modified @ 7.7
                    CTcpClient.Socket_TCPClient_Dispose();
                    //释放内存
                    GC.Collect();
                }


                ////记录部署成功日志信息
                //LogManager.InfoLog.LogProcError("VOBCProduct", "WaitForUpdateResult", "VOBC产品:" + m_productID + "的部署更新成功,发送重置命令");

                ////发送复位指令
                //VOBCCommand resetCommand = new VOBCCommand(m_ip, Convert.ToInt32(m_port), m_productID, vobcCommandType.systemReset);
                //CommandQueue.instance.m_CommandQueue.Enqueue(resetCommand);

                ////刷新界面日志信息
                //Report.ReportWindow("VOBC设备" + m_productID + "更新成功！");

                ////跳出循环标志置为true
                //rev = true;


            }
            //VOBC产品下的子子系统跟新结果均为  更新成功
            else if ((this.CSelectedDevice.FindAll(tar => tar.State == "更新成功").Count == CSelectedDevice.Count) && (this.CSelectedDevice.FindAll(tar => tar.State == "更新成功").Count != 0))
            {
                //该产品已完成部署就停止并禁用心跳计时器
                timerHB.Close();
                timerEnable = false;

                //刷新界面日志信息
                Report.ReportWindow("VOBC" + m_productID + "更新成功！");
                InProcess = false;
                //StepOne = true;
                CDeviceDataFactory.Instance.VobcContainer.SetProductState(Ip, Convert.ToInt32(Port), "更新成功");
                CDeviceDataFactory.Instance.VobcContainer.dataModify.Color();

                //记录部署成功日志信息
                //LogManager.InfoLog.LogProcError("VOBCProduct", "WaitForUpdateResult", "VOBC产品:" + m_productID + "的部署更新成功,发送重置命令");

                //发送复位指令
                VOBCCommand resetCommand = new VOBCCommand(m_ip, Convert.ToInt32(m_port), m_productID, vobcCommandType.systemReset);
                CommandQueue.instance.m_CommandQueue.Enqueue(resetCommand);

                //断开TCP连接并释放资源 Modified @ 7.7
                CTcpClient.Socket_TCPClient_Dispose();
                //释放内存
                GC.Collect();
            }
            else
            {
                //需要修改外层等待更新成功开始的时机是更新进度100%后
                //Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// 检查文件发送及校验文件类型
        /// </summary>
        /// <param name="type">子子系统类型</param>
        /// <returns></returns>
        public bool CheckVeriTypeFile(string type)
        {
            //记录日志
            LogManager.InfoLog.LogProcInfo("VOBCProduct", "CheckVeriTypeFile", "开始检查VOBC产品：" + m_productID + "的" + type + "子子系统类型部署文件校验结果");
           
            //返回值
            bool rtnValue = true;

            //非空防护
            if (_updateFileState == null)
            {
                LogManager.InfoLog.LogProcWarning("VOBCProduct", "CheckVeriTypeFile", "文件状态UpdateFileState实体不存在");
                return false;
            }
            else
            {
                switch (type)
                {
                    case "CCOV":
                        rtnValue &= (DeployConfigCheck.IsBootLoaderCheck == _updateFileState.CcovBootVeriFlag);
                        rtnValue &= (DeployConfigCheck.IsCoreCheck == _updateFileState.CcovCoreVeriFlag);
                        rtnValue &= (DeployConfigCheck.IsDataCheck == _updateFileState.CcovDataVeriFlag);
                        rtnValue &= (DeployConfigCheck.IsIniCheck == _updateFileState.CcovConfigVeriFlag);
                        break;
                    case "ATP":
                        rtnValue &= (DeployConfigCheck.IsBootLoaderCheck == _updateFileState.AtpBootVeriFlag);
                        rtnValue &= (DeployConfigCheck.IsCoreCheck == _updateFileState.AtpCoreVeriFlag);
                        rtnValue &= (DeployConfigCheck.IsDataCheck == _updateFileState.AtpDataVeriFlag);
                        rtnValue &= (DeployConfigCheck.IsNvRamCheck == _updateFileState.AtpNvramVeriFlag);
                        break;
                    case "ATO":
                        rtnValue &= (DeployConfigCheck.IsBootLoaderCheck == _updateFileState.AtoBootVeriFlag);
                        rtnValue &= (DeployConfigCheck.IsCoreCheck == _updateFileState.AtoCoreVeriFlag);
                        rtnValue &= (DeployConfigCheck.IsDataCheck == _updateFileState.AtoDataVeriFlag);
                        rtnValue &= (DeployConfigCheck.IsNvRamCheck == _updateFileState.AtoNvramVeriFlag);
                        break;
                    case "MMI":
                        rtnValue &= (DeployConfigCheck.IsBootLoaderCheck == _updateFileState.MmiBootVeriFlag);
                        rtnValue &= (DeployConfigCheck.IsCoreCheck == _updateFileState.MmiCoreVeriFlag);
                        rtnValue &= (DeployConfigCheck.IsNvRamCheck == _updateFileState.MmiNvramVeriFlag);
                        break;
                    case "COM":
                        rtnValue &= (DeployConfigCheck.IsBootLoaderCheck == _updateFileState.ComBootVeriFlag);
                        rtnValue &= (DeployConfigCheck.IsCoreCheck == _updateFileState.ComCoreVeriFlag);
                        break;
                    default:
                        break;
                }

                //Report.ReportWindow(ProductID + "的" + type + "已校验完成，结果为：" + rtnValue);
                //记录日志
                LogManager.InfoLog.LogProcInfo("VOBCProduct", "CheckVeriTypeFile", type + "子子系统部署文件校验结果为" + rtnValue.ToString());

            }

            return rtnValue;
        }

        /// <summary>
        /// 检查文件更新成功类型
        /// </summary>
        /// <param name="type">子子系统类型</param>
        /// <returns></returns>
        public bool CheckUpdateResultTypeFile(string type)
        {
            //记录日志
            LogManager.InfoLog.LogProcInfo("VOBCProduct", "CheckUpdateResultTypeFile", "开始检查VOBC产品：" + m_productID + "的" + type + "子子系统类型部署文件更新结果");

            //返回值
            bool rtnValue = true;

            //非空防护
            if (DataAnalysis.UpdateFileState == null)
            {
                LogManager.InfoLog.LogProcWarning("VOBCProduct", "CheckUpdateResultTypeFile", "文件状态UpdateFileState实体不存在");
                return false;
            }
            else
            {
                switch (type)
                {
                    case "CCOV":
                        rtnValue &= (DeployConfigCheck.IsBootLoaderCheck == DataAnalysis.UpdateFileState.CcovBootCompleteFlag);
                        rtnValue &= (DeployConfigCheck.IsCoreCheck == DataAnalysis.UpdateFileState.CcovCoreCompleteFlag);
                        rtnValue &= (DeployConfigCheck.IsDataCheck == DataAnalysis.UpdateFileState.CcovDataCompleteFlag);
                        rtnValue &= (DeployConfigCheck.IsIniCheck == DataAnalysis.UpdateFileState.CcovConfigCompleteFlag);
                        break;
                    case "ATP":
                        rtnValue &= (DeployConfigCheck.IsBootLoaderCheck == DataAnalysis.UpdateFileState.AtpBootCompleteFlag);
                        rtnValue &= (DeployConfigCheck.IsCoreCheck == DataAnalysis.UpdateFileState.AtpCoreCompleteFlag);
                        rtnValue &= (DeployConfigCheck.IsDataCheck == DataAnalysis.UpdateFileState.AtpDataCompleteFlag);
                        rtnValue &= (DeployConfigCheck.IsNvRamCheck == DataAnalysis.UpdateFileState.AtpNvramCompleteFlag);
                        break;
                    case "ATO":
                        rtnValue &= (DeployConfigCheck.IsBootLoaderCheck == DataAnalysis.UpdateFileState.AtoBootCompleteFlag);
                        rtnValue &= (DeployConfigCheck.IsCoreCheck == DataAnalysis.UpdateFileState.AtoCoreCompleteFlag);
                        rtnValue &= (DeployConfigCheck.IsDataCheck == DataAnalysis.UpdateFileState.AtoDataCompleteFlag);
                        rtnValue &= (DeployConfigCheck.IsNvRamCheck == DataAnalysis.UpdateFileState.AtoNvramCompleteFlag);
                        break;
                    case "MMI":
                        rtnValue &= (DeployConfigCheck.IsBootLoaderCheck == DataAnalysis.UpdateFileState.MmiBootCompleteFlag);
                        rtnValue &= (DeployConfigCheck.IsCoreCheck == DataAnalysis.UpdateFileState.MmiCoreCompleteFlag);
                        rtnValue &= (DeployConfigCheck.IsNvRamCheck == DataAnalysis.UpdateFileState.MmiNvramCompleteFlag);
                        break;
                    case "COM":
                        rtnValue &= (DeployConfigCheck.IsBootLoaderCheck == DataAnalysis.UpdateFileState.ComBootCompleteFlag);
                        rtnValue &= (DeployConfigCheck.IsCoreCheck == DataAnalysis.UpdateFileState.ComCoreCompleteFlag);
                        break;
                    default:
                        break;
                }

                //记录日志
                LogManager.InfoLog.LogProcInfo("VOBCProduct", "CheckUpdateResultTypeFile", type + "子子系统部署文件更新结果为" + rtnValue.ToString());

            }

            return rtnValue;
        }

        /// <summary>
        /// 生成VOBC产品的CCOV配置文件
        /// </summary>
        private void GenCCOV()
        {

            try
            {
                //获取蓝网文件目录地址
                string folderPath = System.Windows.Forms.Application.StartupPath + CShareLib.VOBC_GEN_FILEPATH + "\\Blue";

                //获取蓝网文件地址
                string filePath = folderPath + "\\ccov.ini";

                //写入IP信息
                WritePrivateProfileString("Internet", "LocalIP", m_ip, filePath);

                //写入子网掩码信息
                WritePrivateProfileString("Internet", "Netmask", CDeviceDataFactory.Instance.VobcContainer.VobcNetMask, filePath);

                //写入网关信息
                WritePrivateProfileString("Internet", "Gateway", CDeviceDataFactory.Instance.VobcContainer.VobcBlueGateWay, filePath);

                //切换至红网文件目录地址
                folderPath = System.Windows.Forms.Application.StartupPath + CShareLib.VOBC_GEN_FILEPATH + "\\Red";

                //获取红网文件地址
                filePath = folderPath + "\\ccov.ini";

                //写入IP信息
                WritePrivateProfileString("Internet", "LocalIP", m_ip, filePath);

                //写入子网掩码信息
                WritePrivateProfileString("Internet", "Netmask", CDeviceDataFactory.Instance.VobcContainer.VobcNetMask, filePath);

                //写入网关信息
                WritePrivateProfileString("Internet", "Gateway", CDeviceDataFactory.Instance.VobcContainer.VobcRedGateWay, filePath);
                
                //日志记录
                LogManager.InfoLog.LogProcInfo("VOBCProduct", "GenCCOV", "生成ccov.ini文件成功！");
            }
            catch
            {
                LogManager.InfoLog.LogCommunicationError("VOBCProduct", "GenCCOV", "写入ccov.ini文件出错！");

                //刷新界面上的日志信息
                Report.ReportWindow("VOBC" + m_productID + "写入ccov.ini文件时出错");
            }

        }

        /// <summary>
        /// 生成ATP和ATO相关的配置文件
        /// </summary>
        private void GenATPATOConfig()
        {
            try
            {

                //odbc模式读取数据
                DataSet dataSet = ExcelIO.ReadDataByOledb(System.Windows.Forms.Application.StartupPath + CShareLib.VOBC_EXCEL_CONFIG_FILEPATH);

                //开始文件生成
                ATPWrite(dataSet.Tables[0]);
                ATOWrite(dataSet.Tables[0]);
                ATPRead(dataSet.Tables[0]);
                ATORead(dataSet.Tables[0]);
            }
            catch (Exception ex) 
            {
                LogManager.InfoLog.LogCommunicationError("VOBCProduct", "GenATPATOConfig", "生成ATPATO配置文件时出错！"+ex.Message);

                //刷新界面上的日志信息
                Report.ReportWindow("VOBC" + m_productID + "生成ATPATO配置文件时出错！");
            }

        }

        /// <summary>
        /// ATPWrite文件生成
        /// </summary>
        /// <param name="dtNvram">dtNvram数据表</param>
        private void ATPWrite(System.Data.DataTable dtNvram)
        {
            LogManager.InfoLog.LogProcInfo("VOBCProduct", "ATPWrite", "开始写入ATPWrite文件");
            DateTime dtNow = DateTime.Now;
            string folderPath = System.Windows.Forms.Application.StartupPath + CShareLib.VOBC_GEN_FILEPATH;
            string filePath = folderPath + "\\ATP\\atp_nvram.txt";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);//若文件夹不存在，则创建文件夹

            }

            System.IO.FileStream fs = new System.IO.FileStream(filePath, FileMode.OpenOrCreate);
            //调用StreamWriter方法，准备写入数据
            System.IO.StreamWriter w = new System.IO.StreamWriter(fs);

            try
            {
                #region DataGrid读入

                w.Write("设定轮径值 w:" + dtNvram.Rows[0][1].ToString().Substring(2, 4) + " "
                    + dtNvram.Rows[0][2].ToString() + " "
                    + Convert.ToInt32(dtNvram.Rows[0][3].ToString()).ToString("X4").Substring(0, 2) + " "
                    + Convert.ToInt32(dtNvram.Rows[0][3].ToString()).ToString("X4").Substring(2, 2) + " "
                    + (~Convert.ToInt32(dtNvram.Rows[0][3].ToString())).ToString("X").Substring(4, 2) + " "
                    + (~Convert.ToInt32(dtNvram.Rows[0][3].ToString())).ToString("X").Substring(6, 2) + "\r\n");
                w.Write("设定1.2端 w:" + dtNvram.Rows[1][1].ToString().Substring(2, 4) + " "
                    + dtNvram.Rows[1][2].ToString() + " "
                    + dtNvram.Rows[1][3].ToString().Substring(2, 2) + " "
                    + dtNvram.Rows[1][3].ToString().Substring(4, 2) + " "
                    + (~Convert.ToInt32(dtNvram.Rows[1][3].ToString().Substring(2, 2), 16)).ToString("X").Substring(6, 2) + " "
                    + (~Convert.ToInt32(dtNvram.Rows[1][3].ToString().Substring(4, 2), 16)).ToString("X").Substring(6, 2)
                    + "\r\n");
                w.Write("设定车组号信息	w:" + dtNvram.Rows[2][1].ToString().Substring(2, 4) + " "
                    + dtNvram.Rows[2][2].ToString() + " "
                    + dtNvram.Rows[2][3].ToString().Substring(2, 2) + " " + dtNvram.Rows[2][3].ToString().Substring(4, 2) + " "
                    + (~Convert.ToInt32(dtNvram.Rows[2][3].ToString().Substring(2, 2), 16)).ToString("X").Substring(6, 2) + " "
                    + (~Convert.ToInt32(dtNvram.Rows[2][3].ToString().Substring(4, 2), 16)).ToString("X").Substring(6, 2)
                    + "\r\n");
                w.Write("设定雷达校正系数 w:" + dtNvram.Rows[3][1].ToString().Substring(2, 4) + " "
                    + dtNvram.Rows[3][2].ToString() + " "
                    + (Convert.ToInt32(float.Parse(dtNvram.Rows[3][3].ToString()) * 100000)).ToString("X8").Substring(0, 2) + " "
                    + (Convert.ToInt32(float.Parse(dtNvram.Rows[3][3].ToString()) * 100000)).ToString("X8").Substring(2, 2) + " "
                    + (Convert.ToInt32(float.Parse(dtNvram.Rows[3][3].ToString()) * 100000)).ToString("X8").Substring(4, 2) + " "
                    + (Convert.ToInt32(float.Parse(dtNvram.Rows[3][3].ToString()) * 100000)).ToString("X8").Substring(6, 2) + " "
                    + (~Convert.ToInt32(float.Parse(dtNvram.Rows[3][3].ToString()) * 100000)).ToString("X").Substring(0, 2) + " "
                    + (~Convert.ToInt32(float.Parse(dtNvram.Rows[3][3].ToString()) * 100000)).ToString("X").Substring(2, 2) + " "
                    + (~Convert.ToInt32(float.Parse(dtNvram.Rows[3][3].ToString()) * 100000)).ToString("X").Substring(4, 2) + " "
                    + (~Convert.ToInt32(float.Parse(dtNvram.Rows[3][3].ToString()) * 100000)).ToString("X").Substring(6, 2)
                    + "\r\n");
                w.Write("设定轮径值组 w:" + dtNvram.Rows[4][1].ToString().Substring(2, 4) + " "
                    + dtNvram.Rows[4][2].ToString() + " "
                    + Convert.ToInt32(dtNvram.Rows[4][3].ToString()).ToString("X4").Substring(0, 2) + " "
                    + Convert.ToInt32(dtNvram.Rows[4][3].ToString()).ToString("X4").Substring(2, 2) + " "
                    + Convert.ToInt32(dtNvram.Rows[4][3].ToString()).ToString("X4").Substring(0, 2) + " "
                    + Convert.ToInt32(dtNvram.Rows[4][3].ToString()).ToString("X4").Substring(2, 2) + " "
                    + Convert.ToInt32(dtNvram.Rows[4][3].ToString()).ToString("X4").Substring(0, 2) + " "
                    + Convert.ToInt32(dtNvram.Rows[4][3].ToString()).ToString("X4").Substring(2, 2) + " "
                    + Convert.ToInt32(dtNvram.Rows[4][3].ToString()).ToString("X4").Substring(0, 2) + " "
                    + Convert.ToInt32(dtNvram.Rows[4][3].ToString()).ToString("X4").Substring(2, 2) + " "
                    + Convert.ToInt32(dtNvram.Rows[4][3].ToString()).ToString("X4").Substring(0, 2) + " "
                    + Convert.ToInt32(dtNvram.Rows[4][3].ToString()).ToString("X4").Substring(2, 2) + " 00 00 00 00 "
                    + (~Convert.ToInt32(dtNvram.Rows[4][3].ToString())).ToString("X").Substring(4, 2) + " "
                    + (~Convert.ToInt32(dtNvram.Rows[4][3].ToString())).ToString("X").Substring(6, 2) + " "
                    + (~Convert.ToInt32(dtNvram.Rows[4][3].ToString())).ToString("X").Substring(4, 2) + " "
                    + (~Convert.ToInt32(dtNvram.Rows[4][3].ToString())).ToString("X").Substring(6, 2) + " "
                    + (~Convert.ToInt32(dtNvram.Rows[4][3].ToString())).ToString("X").Substring(4, 2) + " "
                    + (~Convert.ToInt32(dtNvram.Rows[4][3].ToString())).ToString("X").Substring(6, 2) + " "
                    + (~Convert.ToInt32(dtNvram.Rows[4][3].ToString())).ToString("X").Substring(4, 2) + " "
                    + (~Convert.ToInt32(dtNvram.Rows[4][3].ToString())).ToString("X").Substring(6, 2) + " "
                    + (~Convert.ToInt32(dtNvram.Rows[4][3].ToString())).ToString("X").Substring(4, 2) + " "
                    + (~Convert.ToInt32(dtNvram.Rows[4][3].ToString())).ToString("X").Substring(6, 2)
                    + " FF FF FF FF" + "\r\n");

                #endregion

                LogManager.InfoLog.LogProcInfo("VOBCProduct", "ATPWrite", "生成ATP_NvramWriteCmd.txt文件成功！");
            }
            catch (Exception ex)
            {
                LogManager.InfoLog.LogCommunicationError("VOBCProduct", "ATPWrite", "生成ATP_NvramWriteCmd.txt文件出错！" + ex.Message);
                MessageBox.Show("生成ATP_NvramWriteCmd.txt文件出错！");
            }
            finally 
            {
                w.Close();
                fs.Close();
            }

        }

        /// <summary>
        /// ATOWrite文件生成
        /// </summary>
        /// <param name="dtNvram"></param>
        private void ATOWrite(System.Data.DataTable dtNvram)
        {
            LogManager.InfoLog.LogProcInfo("VOBCProduct", "ATOWrite", "开始写入ATOWrite文件");
            DateTime dtNow = DateTime.Now;
            string folderPath = System.Windows.Forms.Application.StartupPath + CShareLib.VOBC_GEN_FILEPATH;
            string filePath = folderPath + "\\ATO\\ato_nvram.txt";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);//若文件夹不存在，则创建文件夹
            }

            System.IO.FileStream fs = new System.IO.FileStream(filePath, FileMode.OpenOrCreate);
            //调用StreamWriter方法，准备写入数据
            System.IO.StreamWriter w = new System.IO.StreamWriter(fs);

            try
            {
                #region DataGrid读入
                w.Write("第一组PID参数修改 w:" + dtNvram.Rows[10][1].ToString().Substring(2, 4) + " "
                    + dtNvram.Rows[10][2].ToString() + " "
                   + (Convert.ToInt32(float.Parse(dtNvram.Rows[10][3].ToString()) * 100)).ToString("X2") + " "
                   + (Convert.ToInt32(float.Parse(dtNvram.Rows[10][4].ToString()) * 100)).ToString("X2") + " "
                   + (Convert.ToInt32(float.Parse(dtNvram.Rows[10][5].ToString()) * 100)).ToString("X2") + " "
                   + (~Convert.ToInt32(float.Parse(dtNvram.Rows[10][3].ToString()) * 100)).ToString("X").Substring(6, 2) + " "
                   + (~Convert.ToInt32(float.Parse(dtNvram.Rows[10][4].ToString()) * 100)).ToString("X").Substring(6, 2) + " "
                   + (~Convert.ToInt32(float.Parse(dtNvram.Rows[10][5].ToString()) * 100)).ToString("X").Substring(6, 2) + "\r\n");
                w.Write("第一组PID参数修改 w:" + dtNvram.Rows[11][1].ToString().Substring(2, 4) + " "
                    + dtNvram.Rows[11][2].ToString() + " "
                   + (Convert.ToInt32(float.Parse(dtNvram.Rows[11][3].ToString()) * 100)).ToString("X2") + " "
                   + (Convert.ToInt32(float.Parse(dtNvram.Rows[11][4].ToString()) * 100)).ToString("X2") + " "
                   + (Convert.ToInt32(float.Parse(dtNvram.Rows[11][5].ToString()) * 100)).ToString("X2") + " "
                   + (~Convert.ToInt32(float.Parse(dtNvram.Rows[11][3].ToString()) * 100)).ToString("X").Substring(6, 2) + " "
                   + (~Convert.ToInt32(float.Parse(dtNvram.Rows[11][4].ToString()) * 100)).ToString("X").Substring(6, 2) + " "
                   + (~Convert.ToInt32(float.Parse(dtNvram.Rows[11][5].ToString()) * 100)).ToString("X").Substring(6, 2) + "\r\n");
                if (dtNvram.Rows[12][3].ToString().Contains("-"))
                {
                    w.Write("设定停车点距离微调值 w:" + dtNvram.Rows[12][1].ToString().Substring(2, 4) + " "
                        + dtNvram.Rows[12][2].ToString() + " 00 "
                        + Convert.ToInt32(dtNvram.Rows[12][3].ToString().Replace("-", "")).ToString("X2") + " FF "
                        + (~Convert.ToInt32(dtNvram.Rows[12][3].ToString().Replace("-", ""))).ToString("X").Substring(6, 2) + "\r\n");
                }
                else
                {
                    w.Write("设定停车点距离微调值 w:" + dtNvram.Rows[12][1].ToString().Substring(2, 4) + " "
                        + dtNvram.Rows[12][2].ToString() + " 01 "
                        + Convert.ToInt32(dtNvram.Rows[12][3].ToString()).ToString("X2") + " FE "
                        + (~Convert.ToInt32(dtNvram.Rows[12][3].ToString())).ToString("X").Substring(6, 2) + "\r\n");
                }
                w.Write("设定停车目标制动率计算系数 w:" + dtNvram.Rows[13][1].ToString().Substring(2, 4) + " "
                    + dtNvram.Rows[13][2].ToString() + " "
                    + Convert.ToInt32(float.Parse(dtNvram.Rows[13][3].ToString()) * 10000).ToString("X4").Substring(0, 2) + " "
                    + Convert.ToInt32(float.Parse(dtNvram.Rows[13][3].ToString()) * 10000).ToString("X4").Substring(2, 2) + " "
                    + Convert.ToInt32(dtNvram.Rows[13][4].ToString()).ToString("X2") + " "
                    + (~Convert.ToInt32(float.Parse(dtNvram.Rows[13][3].ToString()) * 10000)).ToString("X").Substring(4, 2) + " "
                    + (~Convert.ToInt32(float.Parse(dtNvram.Rows[13][3].ToString()) * 10000)).ToString("X").Substring(6, 2) + " "
                    + (~Convert.ToInt32(dtNvram.Rows[13][4].ToString())).ToString("X").Substring(6, 2) + "\r\n");
                w.Write("巡航阶段惰行门限 w:" + dtNvram.Rows[14][1].ToString().Substring(2, 4) + " "
                    + dtNvram.Rows[14][2].ToString() + " "
                    + Convert.ToInt32(dtNvram.Rows[14][3].ToString()).ToString("X4").Substring(0, 2) + " "
                    + Convert.ToInt32(dtNvram.Rows[14][3].ToString()).ToString("X4").Substring(2, 2) + " "
                    + (~Convert.ToInt32(dtNvram.Rows[14][3].ToString())).ToString("X").Substring(4, 2) + " "
                    + (~Convert.ToInt32(dtNvram.Rows[14][3].ToString())).ToString("X").Substring(6, 2) + "\r\n");
                w.Write("设定车组号信息	w:" + dtNvram.Rows[15][1].ToString().Substring(2, 4) + " "
                    + dtNvram.Rows[15][2].ToString() + " "
                    + dtNvram.Rows[15][3].ToString().Substring(2, 2) + " " + dtNvram.Rows[15][3].ToString().Substring(4, 2) + " "
                    + (~Convert.ToInt32(dtNvram.Rows[15][3].ToString().Substring(2, 2), 16)).ToString("X").Substring(6, 2) + " "
                    + (~Convert.ToInt32(dtNvram.Rows[15][3].ToString().Substring(4, 2), 16)).ToString("X").Substring(6, 2)
                    + "\r\n");
                w.Write("设定头尾端信息 w:" + dtNvram.Rows[16][1].ToString().Substring(2, 4) + " "
                    + dtNvram.Rows[16][2].ToString() + " "
                    + dtNvram.Rows[16][3].ToString().Substring(2, 2) + " " + dtNvram.Rows[16][3].ToString().Substring(4, 2) + " "
                    + (~Convert.ToInt32(dtNvram.Rows[16][3].ToString().Substring(2, 2), 16)).ToString("X").Substring(6, 2) + " "
                    + (~Convert.ToInt32(dtNvram.Rows[16][3].ToString().Substring(4, 2), 16)).ToString("X").Substring(6, 2)
                    + "\r\n");
                #endregion
                LogManager.InfoLog.LogProcInfo("VOBCProduct", "ATOWrite", "生成ATO_NvramWriteCmd.txt文件成功！");
                //MessageBox.Show("生成ATO_NvramWriteCmd.txt文件成功！");
            }
            catch (Exception exp)
            {
                LogManager.InfoLog.LogProcError("VOBCProduct", "ATOWrite", "生成ATO_NvramWriteCmd.txt文件出错！" + exp.Message);
                MessageBox.Show("生成ATO_NvramWriteCmd.txt文件出错！");
            }
            finally
            {
                w.Close();
                fs.Close();
            }
        }

        /// <summary>
        /// ATPRead文件生成
        /// </summary>
        /// <param name="dtNvram"></param>
        private void ATPRead(System.Data.DataTable dtNvram)
        {
            LogManager.InfoLog.LogProcInfo("VOBCProduct", "ATPRead", "开始写入ATPRead文件");
            DateTime dtNow = DateTime.Now;
            string folderPath = System.Windows.Forms.Application.StartupPath + CShareLib.VOBC_GEN_FILEPATH;
            string filePath = folderPath + "\\" + "ATP_NvramReadCmd.txt";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);//若文件夹不存在，则创建文件夹
            }
            System.IO.FileStream fs = new System.IO.FileStream(filePath, FileMode.OpenOrCreate);
            //调用StreamWriter方法，准备写入数据
            System.IO.StreamWriter w = new System.IO.StreamWriter(fs);
            try
            {
                w.Write("读取轮径值 r:" + dtNvram.Rows[5][1].ToString().Substring(2, 4) + " "
                    + dtNvram.Rows[5][2].ToString() + "\r\n");
                w.Write("读取1、2端设置信息 r:" + dtNvram.Rows[6][1].ToString().Substring(2, 4) + " "
                    + dtNvram.Rows[6][2].ToString() + "\r\n");
                w.Write("读取车组号信息 r:" + dtNvram.Rows[7][1].ToString().Substring(2, 4) + " "
                    + dtNvram.Rows[7][2].ToString() + "\r\n");
                w.Write("读取雷达校正系数 r:" + dtNvram.Rows[8][1].ToString().Substring(2, 4) + " "
                    + dtNvram.Rows[8][2].ToString() + "\r\n");
                w.Write("读取轮径值 r:" + dtNvram.Rows[9][1].ToString().Substring(2, 4) + " "
                    + dtNvram.Rows[9][2].ToString() + "\r\n");
                LogManager.InfoLog.LogProcInfo("VOBCProduct", "ATPRead", "生成ATP_NvramReadCmd.txt文件成功！");
                //MessageBox.Show("生成ATP_NvramReadCmd.txt文件成功！");
            }
            catch (Exception expe)
            {
                LogManager.InfoLog.LogProcError("VOBCProduct", "ATPRead", "生成ATP_NvramReadCmd.txt文件出错！" + expe.Message);
                MessageBox.Show("生成ATP_NvramReadCmd.txt文件出错！");
            }
            finally
            {
                w.Close();
                fs.Close();
            }
        }

        /// <summary>
        /// ATORead文件生成
        /// </summary>
        /// <param name="dtNvram"></param>
        private void ATORead(System.Data.DataTable dtNvram)
        {
            LogManager.InfoLog.LogProcInfo("VOBCProduct", "ATORead", "开始生成ATORead文件！");
            DateTime dtNow = DateTime.Now;
            string folderPath = System.Windows.Forms.Application.StartupPath + CShareLib.VOBC_GEN_FILEPATH;
            string filePath = folderPath + "\\" + "ATO_NvramReadCmd.txt";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);//若文件夹不存在，则创建文件夹
            }
            System.IO.FileStream fs = new System.IO.FileStream(filePath, FileMode.OpenOrCreate);
            //调用StreamWriter方法，准备写入数据
            System.IO.StreamWriter w = new System.IO.StreamWriter(fs);
            try
            {
                w.Write("第一组PID参数读取 r:" + dtNvram.Rows[17][1].ToString().Substring(2, 4) + " "
                    + dtNvram.Rows[17][2].ToString() + "\r\n");
                w.Write("第二组PID参数读取 r:" + dtNvram.Rows[18][1].ToString().Substring(2, 4) + " "
                    + dtNvram.Rows[18][2].ToString() + "\r\n");
                w.Write("停车点距离微调参数读取 r:" + dtNvram.Rows[19][1].ToString().Substring(2, 4) + " "
                    + dtNvram.Rows[19][2].ToString() + "\r\n");
                w.Write("停车目标制动率计算系数读取 r:" + dtNvram.Rows[20][1].ToString().Substring(2, 4) + " "
                    + dtNvram.Rows[20][2].ToString() + "\r\n");
                w.Write("巡航阶段惰行门限 r:" + dtNvram.Rows[21][1].ToString().Substring(2, 4) + " "
                    + dtNvram.Rows[21][2].ToString() + "\r\n");
                w.Write("设定车组号信息 r:" + dtNvram.Rows[22][1].ToString().Substring(2, 4) + " "
                    + dtNvram.Rows[22][2].ToString() + "\r\n");
                //w.Write("设定头尾端信息 r:" + dtNvram.Rows[23][1].ToString().Substring(2, 4) + " "
                //    + dtNvram.Rows[23][2].ToString() + "\r\n");
                LogManager.InfoLog.LogProcInfo("VOBCProduct", "ATORead", "生成ATO_NvramReadCmd.txt文件成功！");
                ///MessageBox.Show("生成ATO_NvramReadCmd.txt文件成功！");
            }
            catch (Exception expe)
            {
                LogManager.InfoLog.LogProcError("VOBCProduct", "ATORead", "生成ATP_NvramReadCmd.txt文件出错！" + expe.Message);
                MessageBox.Show("生成ATO_NvramReadCmd.txt文件出错！");
            }
            finally
            {
                w.Close();
                fs.Close();
            }
        }

        /// <summary>
        /// 心跳计时器处理函数
        /// </summary>
        public void HeartBeatTimerInit()
        {
            timerHB.Dispose();
            timerHB = new System.Timers.Timer(15000);
            timerHB.AutoReset = false;            
            timerHB.Start();
            timerHB.Elapsed += new System.Timers.ElapsedEventHandler(timerHB_Elapsed);
        }

        /// <summary>
        /// 跳维持超时事件--已超时 将提示故障等信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerHB_Elapsed(object sender, EventArgs e)
        {
            //断开TCP连接并释放资源
            CTcpClient.Socket_TCPClient_Dispose();

            //断链后初始化计时器允许标志
            timerEnable = false;

            //设置当前产品全部设备状态为故障
            foreach (IDevice device in CBelongsDevice)
            {
                device.State = "故障";
            }

            //设置标志位
            //Modified @ 9.10
            if (InProcess)
            {
                SkipFlag = true;
            }
            InProcess = false;
            timerHB.Dispose();
            //StepOne = true;
            //设置该产品通信状态为中断
            CDeviceDataFactory.Instance.VobcContainer.SetProductFailReason(Ip, Convert.ToInt32(Port), "与下位机通信中断");
            
            
            CDeviceDataFactory.Instance.VobcContainer.SetProductState(Ip, Convert.ToInt32(Port), "中断");
            
                       

            //向消息窗口汇报
            Report.ReportWindow(ProductID + "超过通信中断判定时间未收到心跳信息，断开连接！请重新开始部署");

            //记录日志
            LogManager.InfoLog.LogProcInfo("VOBCCommand", "TcpVobc_EBackData", ProductID + "超过通信中断判定时间未收到心跳信息，断开连接！");
        }

        /// <summary>
        /// 关闭超时计时timer并释放资源
        /// </summary>
        public void TimerClose()
        {
            timerHB.Dispose();
        }

        
    }


}
