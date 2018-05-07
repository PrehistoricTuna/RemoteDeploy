using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteDeploy.Models.VOBC;
using RemoteDeploy.EquData;
using TCT.ShareLib.LogManager;
using System.Net;
using System.Timers;
using RemoteDeploy.Common;
using RemoteDeploy.Command;
using RemoteDeploy.ControlDispatcher;
using RemoteDeploy.NetworkService;

namespace RemoteDeploy.DataPack
{
    /// <summary>
    /// 数据解析类
    /// </summary>
    public static class DataAnalysis
    {

        #region VOBC通信帧类型常量值定义

        /// <summary>
        /// VOBC-文件更新回复帧
        /// </summary>
        private static readonly Int32 vobcResponseFrame_FileUpdateStart = 0x01;

        /// <summary>
        /// VOBC-文件校验回复帧
        /// </summary>
        private static readonly Int32 vobcResponseFrame_FileCheck = 0x02;

        /// <summary>
        /// VOBC-关闭连接回复帧
        /// </summary>
        private static readonly Int32 vobcResponseFrame_CloseLink = 0x03;

        /// <summary>
        /// VOBC-系统复位回复帧
        /// </summary>
        private static readonly Int32 vobcResponseFrame_SystemReset = 0x04;

        /// <summary>
        /// VOBC-文件更新进度报告帧
        /// </summary>
        private static readonly Int32 vobcResponseFrame_UpdatePersent = 0x05;

        /// <summary>
        /// VOBC-文件更新完成报告帧
        /// </summary>
        private static readonly Int32 vobcResponseFrame_UpdateFinish = 0x06;

        /// <summary>
        /// VOBC-列车状态报告帧
        /// </summary>
        private static readonly Int32 vobcResponseFrame_CarInfo = 0x07;

        /// <summary>
        /// VOBC-通控心跳回复帧
        /// </summary>
        private static readonly Int32 vobcResponseFrame_Heart = 0x08;

        /// <summary>
        /// VOBC-建链回复帧
        /// </summary>
        private static readonly Int32 vobcResponseFrame_OpenLink = 0x09;

        /// <summary>
        /// VOBC-远程重启回复帧
        /// </summary>
        private static readonly Int32 vobcResponseFrame_Restart = 0x0A;

        /// <summary>
        /// VOBC-文件上传请求回复帧
        /// </summary>
        private static readonly Int32 vobcResponseFrame_FileUpload = 0x0B;

        /// <summary>
        /// VOBC-停止更新回复帧
        /// </summary>
        private static readonly Int32 vobcResponseFrame_FileUpdateStop = 0x0C;

        /// <summary>
        /// VOBC-CCOV向上位机获取MD5码
        /// </summary>
        private static readonly Int32 vobcResponseFrame_CCOVGetMd5 = 0x0D;

        #endregion

        #region 变量

        /// <summary>
        /// 更新文件状态存储类对象
        /// </summary>
        public static VOBCUpdateFileState UpdateFileState
        {
            get
            {
                return _updateFileState;
            }

        }
        /// <summary>
        /// 更新文件状态存储类对象
        /// </summary>
        public static VOBCUpdateFileState _updateFileState = new VOBCUpdateFileState();

        /// <summary>
        /// 更新成功的文件数量计数
        /// </summary>
        private static int updateSuccessFileCount = 0;

        private static Timer timerHB = new Timer();

        public static bool timeout = false;

        private static bool timerEnable = false;

        #endregion

        #region VOBC通信帧解析

        /// <summary>
        /// 数据解析
        /// </summary>
        /// <param name="recvServerData"></param>
        public static void VOBCDataAnalysis(byte[] Data,Socket_TCPClient client)
        {
            VOBCProduct vobc = CDeviceDataFactory.Instance.GetProductByIpPort(client.ServerIP, client.ServerPort);

            try
            {
                //若有黏包 拆包处理
                List<byte[]> dataList = PacketDisassembly(Data);

                //遍历集合中数据进行处理
                foreach (byte[] recvServerData in dataList)
                {

                    //获取TCP客户端连接对象（服务端）的IP和端口
                    IPEndPoint IPPort = client.clientSocket.RemoteEndPoint as IPEndPoint;
                    string serverIP = Convert.ToString(IPPort.Address);
                    int serverPort = IPPort.Port;

                    //非空验证 防止异常出现
                    if (recvServerData == null)
                    {
                        LogManager.InfoLog.LogCommunicationInfo("DataAnalysis",
                            "VOBCDataAnalysis", "收到[" + serverIP + ":" + serverPort + "]的数据为空");
                        return;
                    }
                    /*①非空验证 防止异常出现 
                        *②CRC验证
                        */
                    else if (recvServerData != null && recvServerData.Length > 0)
                    {
                        //CRC校验结果
                        if (!CRC.CheckCRC32(recvServerData))
                        {
                            LogManager.InfoLog.LogCommunicationInfo("DataAnalysis",
                                "VOBCDataAnalysis", "收到[" + serverIP + ":" + serverPort + "]的数据校验CRC失败");
                            return;
                        }

                        //起始解析索引
                        int iter = 2;

                        //UDP调试代码
                        //string serverIP = "127.0.0.1";
                        //int serverPort = 50000; 

                        //心跳帧
                        if (recvServerData[iter] == vobcResponseFrame_Heart)
                        {
                            if (timerEnable)
                            {
                                HeartBeatTimerInit();
                            }
                        }
                        //建链回复帧
                        else if (recvServerData[iter] == vobcResponseFrame_OpenLink)
                        {
                            //记录日志
                            LogManager.InfoLog.LogCommunicationInfo("DataAnalysis", "VOBCDataAnalysis", "收到" + serverIP + "建链回复帧");

                            iter++;
                            //LogManager.InfoLog.LogCommunicationInfo("Socket_TCPClient", "Me_ReceiveMessage", "收到建链回复帧");

                            //获取VOBCContainer
                            IProContainer item = CDeviceDataFactory.Instance.ProjectConsole.Projducts.Find(tar =>
                                tar is VOBCContainer);

                            //判定回复的状态
                            if (recvServerData[iter] == CommonConstValue.constValueHEX55)
                            {
                                item.SetProductState(serverIP, "正常");

                                //启用并开始心跳超时计时器
                                timerEnable = true;
                                HeartBeatTimerInit();
                            }
                            else if (recvServerData[iter] == CommonConstValue.constValueHEXAA)
                            {
                                item.SetProductState(serverIP, "故障");
                            }
                            else
                            {
                                //TODO:非 CommonConstValue.constValueHEX55/CommonConstValue.constValueHEXAA的回执 暂不处理
                            }
                        }
                        //VOBC状态信息回复帧
                        else if (recvServerData[iter] == vobcResponseFrame_CarInfo)
                        {
                            //获取VOBCContainer
                            IProContainer item = CDeviceDataFactory.Instance.ProjectConsole.Projducts.Find(tar =>
                                tar is VOBCContainer);

                            //记录日志
                            LogManager.InfoLog.LogCommunicationInfo("DataAnalysis", "VOBCDataAnalysis", "收到VOBC状态回复帧");

                            //解析VOBC状态信息
                            VOBCStateInfoClass vobcInfo = GetVOBCInfo(recvServerData);

                            //设置VOBC状态信息
                            item.SetProductVobcStateInfo(serverIP, vobcInfo);

                            //解析后的数据回传界面
                            //ATP信息回传
                            DeviceState deviceState = new DeviceState(vobcInfo.AtpStatus, vobcInfo.AtpSoftVersion, vobcInfo.AtpDataVersion);
                            CDeviceDataFactory.Instance.VobcContainer.SetProductDeviceState(serverIP, "ATP", deviceState);

                            //ATO信息回传
                            deviceState = new DeviceState(vobcInfo.AtoStatus, vobcInfo.AtoSoftVersion, vobcInfo.AtoDataVersion);
                            CDeviceDataFactory.Instance.VobcContainer.SetProductDeviceState(serverIP, "ATO", deviceState);

                            //MMI信息回传
                            deviceState = new DeviceState(vobcInfo.MmiStatus, vobcInfo.MmiSoftVersion, "无");
                            CDeviceDataFactory.Instance.VobcContainer.SetProductDeviceState(serverIP, "MMI", deviceState);

                            //COM1信息回传
                            deviceState = new DeviceState(vobcInfo.ComStatus, vobcInfo.Com1SoftVersion, "无");
                            CDeviceDataFactory.Instance.VobcContainer.SetProductDeviceState(serverIP, "COM_1", deviceState);

                            //COM2信息回传
                            deviceState = new DeviceState(vobcInfo.ComStatus, vobcInfo.Com2SoftVersion, "无");
                            CDeviceDataFactory.Instance.VobcContainer.SetProductDeviceState(serverIP, "COM_2", deviceState);

                            //CCOV信息回传
                            deviceState = new DeviceState(vobcInfo.CCOVStatus, vobcInfo.CCOVSoftVersion, vobcInfo.CCOVDataVersion);
                            CDeviceDataFactory.Instance.VobcContainer.SetProductDeviceState(serverIP, "CC", deviceState);

                            //通知界面刷新
                            CDeviceDataFactory.Instance.VobcContainer.dataModify.Modify();

                        }
                        //文件上传请求回复帧
                        else if (recvServerData[iter] == vobcResponseFrame_FileUpload)
                        {
                            //记录日志
                            LogManager.InfoLog.LogCommunicationInfo("DataAnalysis", "VOBCDataAnalysis", "收到文件上传请求回复帧");
                            iter++;

                            //判定回复的状态
                            CDeviceDataFactory.Instance.VobcContainer.SetVOBCDeviceFileState(serverIP,
                                (recvServerData[iter] == CommonConstValue.constValueHEX55 ? true : false));

                        }
                        //文件校验请求回复帧
                        else if (recvServerData[iter] == vobcResponseFrame_FileCheck)
                        {
                            //记录日志
                            LogManager.InfoLog.LogCommunicationInfo("DataAnalysis", "VOBCDataAnalysis", "收到文件校验请求回复帧");

                            //校验文件类型值 转为8位二进制
                            string binarySysData = Convert.ToString(recvServerData[(iter + 2)], 2).PadLeft(8, '0');

                            //判定子子系统类型
                            switch ((recvServerData[(iter + 1)]))
                            {
                                case 0x01:
                                case 0x02:
                                case 0x03:
                                    SetVOBCATPCheckState(recvServerData, iter, binarySysData);
                                    break;
                                case 0x04:
                                case 0x05:
                                    SetVOBCATOCheckState(recvServerData, iter, binarySysData);
                                    break;
                                case 0x06:
                                case 0x07:
                                    SetVOBCCOMCheckState(recvServerData, iter, binarySysData);
                                    break;
                                case 0x08:
                                    SetVOBCMMICheckState(recvServerData, iter, binarySysData);
                                    break;
                                case 0x09:
                                    SetVOBCCCOVCheckState(recvServerData, iter, binarySysData);
                                    break;
                                default:
                                    //TODO  不处理
                                    break;
                            }

                            LogManager.InfoLog.LogProcInfo("DataAnalysis", "文件校验请求接收回复帧", "接收到文件校验结果信息");
                            //回执检查状态                         
                            CDeviceDataFactory.Instance.VobcContainer.SetVOBCDeviceCheckState(serverIP, _updateFileState.VeriResult);

                        }
                        //文件更新请求回复帧
                        else if (recvServerData[iter] == vobcResponseFrame_FileUpdateStart)
                        {
                            //记录日志
                            LogManager.InfoLog.LogCommunicationInfo("DataAnalysis", "VOBCDataAnalysis", "收到文件更新请求回复帧");

                            iter++;//这里有误，首先需要先判断更新请求回复帧，其中如有任何一系的任何一类文件失败，应显示

                            //设置烧录子子系统在界面中的显示状态--文件待重启
                            CDeviceDataFactory.Instance.VobcContainer.SetProductState(serverIP, "待重启");
                            CDeviceDataFactory.Instance.VobcContainer.SetProductDeviceState(serverIP,
                            CommonMethod.GetVobcSystemListByType(vobcSystemType.ALL),
                            Convert.ToString(CommonMethod.GetVobcDeployNameByType(vobcSystemDeployState.DevRestart)));
                            //通知界面刷新
                            CDeviceDataFactory.Instance.VobcContainer.dataModify.Modify();

                            //禁用并停止心跳超时计时器
                            timerEnable = false;
                            timerHB.Stop();
                            timerHB.Close();
                        }
                        //远程重启回复帧
                        else if (recvServerData[iter] == vobcResponseFrame_Restart)
                        {
                            //TODO:暂时不处理，下位机硬件暂不支持
                        }
                        //文件更新进度汇报帧
                        else if (recvServerData[iter] == vobcResponseFrame_UpdatePersent)
                        {
                            //记录日志
                            LogManager.InfoLog.LogCommunicationInfo("DataAnalysis", "VOBCDataAnalysis", "收到文件更新进度回复帧");

                            //计算传输百分比
                            int updatePercent = getUpdatePersent(recvServerData, iter);

                            iter++;

                            //获取子系统文件类型
                            Common.vobcSystemType sysType = getVobcSystemType(recvServerData, iter);

                            //获取更新的文件类型
                            Common.vobcFileType fileType = getVobcFileType(recvServerData, (iter + 1));

                            //设置进度信息
                            CDeviceDataFactory.Instance.VobcContainer.SetProductVOBCDeviceProc(serverIP, updatePercent, sysType, fileType);

                        }
                        //停止更新请求回复帧
                        else if (recvServerData[iter] == vobcResponseFrame_FileUpdateStop)
                        {
                            //记录日志
                            LogManager.InfoLog.LogCommunicationInfo("DataAnalysis", "VOBCDataAnalysis", "收到停止更新文件回复帧");

                            iter++;

                            //判定回复的状态
                            if (recvServerData[iter] == CommonConstValue.constValueHEX55)
                            {
                                //同意停止更新
                                //设置烧录子子系统在界面中的显示状态--停止更新
                                CDeviceDataFactory.Instance.VobcContainer.SetProductDeviceState(serverIP,
                                CommonMethod.GetVobcSystemListByType(vobcSystemType.ALL),
                                "停止更新");
                                //通知界面刷新
                                CDeviceDataFactory.Instance.VobcContainer.dataModify.Modify();
                            }
                            else if (recvServerData[iter] == CommonConstValue.constValueHEXAA)
                            {
                                //不同意停止更新
                                //设置烧录子子系统在界面中的显示状态--无法停止更新
                                CDeviceDataFactory.Instance.VobcContainer.SetProductDeviceState(serverIP,
                                CommonMethod.GetVobcSystemListByType(vobcSystemType.ALL),
                                "无法停止更新");
                                //通知界面刷新
                                CDeviceDataFactory.Instance.VobcContainer.dataModify.Modify();
                            }
                            else
                            {
                                //TODO:非 CommonConstValue.constValueHEX55/CommonConstValue.constValueHEXAA的回执 暂不处理
                            }
                        }
                        //文件更新成功汇报帧
                        else if (recvServerData[iter] == vobcResponseFrame_UpdateFinish)
                        {
                            //记录日志
                            LogManager.InfoLog.LogCommunicationInfo("DataAnalysis", "VOBCDataAnalysis", "收到文件更新完成回复帧");

                            //获取更新子子系统类型
                            Common.vobcSystemType sysType = getVobcSystemType(recvServerData, (iter + 1));

                            ////获取更新的文件类型
                            //Common.vobcFileType fileType = getVobcFileType(recvServerData, (iter + 2));

                            ////计算更新文件数量
                            //int updateFileCount = 0;
                            //for (int i = 0; i < vobc.CheckFileList.Count; i++)
                            //{
                            //    updateFileCount += vobc.CheckFileList[i].vobcFilePathList.Count;
                            //}

                            ////更新文件总数对比已校验更新文件数量
                            //if (updateFileCount > updateSuccessFileCount)
                            //{
                            //    //递增
                            //    updateSuccessFileCount++;

                            //    //判定子子系统类型
                            //    switch ((recvServerData[(iter + 1)]))
                            //    {
                            //        case 0x01:
                            //        case 0x02:
                            //        case 0x03:
                            //            SetVOBCATPcompleteState(recvServerData, fileType, iter + 3);
                            //            break;
                            //        case 0x04:
                            //        case 0x05:
                            //            SetVOBCATOcompleteState(recvServerData, fileType, iter + 3);
                            //            break;
                            //        case 0x06:
                            //        case 0x07:
                            //            SetVOBCCOMcompleteState(recvServerData, fileType, iter + 3);
                            //            break;
                            //        case 0x08:
                            //            SetVOBCMMIcompleteState(recvServerData, fileType, iter + 3);
                            //            break;
                            //        case 0x09:
                            //            SetVOBCCCOVcompleteState(recvServerData, fileType, iter + 3);
                            //            break;
                            //        default:
                            //            //TODO  不处理
                            //            break;
                            //    }
                            //}
                            ////全部更新完成 重置计数器并回传设置更新状态
                            //else
                            //{
                            //    //重置
                            //    updateSuccessFileCount = 0;

                            //}

                            //设置更新状态
                            CDeviceDataFactory.Instance.VobcContainer.SetProductVOBCDeviceState(serverIP, sysType, recvServerData[iter + 3]);

                            //当出现更新失败的设备时直接提示整车状态为更新失败
                            if (recvServerData[iter + 3] == CommonConstValue.constValueHEXAA)
                            {
                                CDeviceDataFactory.Instance.VobcContainer.SetProductState(serverIP, "更新失败");
                            }
                        }
                        //远程复位回复帧
                        else if (recvServerData[iter] == vobcResponseFrame_SystemReset)
                        {
                            iter++;

                            //判定系统复位请求回复信息
                            switch (recvServerData[iter])
                            {
                                case CommonConstValue.constValueHEX55:
                                    //TODO:已去除系统更新标志
                                    break;
                                case CommonConstValue.constValueHEXAA:
                                    //TODO:未去除系统更新标志
                                    break;
                                default:
                                    //TODO:非 CommonConstValue.constValueHEX55/CommonConstValue.constValueHEXAA的回执 暂不处理
                                    break;
                            }

                            iter++;

                            //判定应用启动请求回复信息
                            switch (recvServerData[iter])
                            {
                                case CommonConstValue.constValueHEX55:
                                    //TODO:Bootloader将启动应用
                                    break;
                                case CommonConstValue.constValueHEXAA:
                                    //TODO:Bootloader将不启动应用
                                    break;
                                default:
                                    //TODO:非 CommonConstValue.constValueHEX55/CommonConstValue.constValueHEXAA的回执 暂不处理
                                    break;
                            }
                        }
                        //断链请求回复帧
                        else if (recvServerData[iter] == vobcResponseFrame_CloseLink)
                        {
                            iter++;
                            //判定系统复位请求回复信息
                            switch (recvServerData[iter])
                            {
                                case CommonConstValue.constValueHEX55:
                                    //TODO:已关闭TCP连接
                                    break;
                                case CommonConstValue.constValueHEXAA:
                                    //TODO:未关闭TCP连接
                                    break;
                                default:
                                    //TODO:非 CommonConstValue.constValueHEX55/CommonConstValue.constValueHEXAA的回执 暂不处理
                                    break;
                            }

                        }
                        //CCOV向上位机获取MD5码
                        else if (recvServerData[iter] == vobcResponseFrame_CCOVGetMd5)
                        {

                            iter++;

                            LogManager.InfoLog.LogCommunicationInfo("DataAnalysis", "VOBCDataAnalysis", "收到" + serverIP + "的二次MD5请求");

                            //获取子系统文件类型
                            Common.vobcSystemType sysType = getVobcSystemType(recvServerData, iter);

                            //烧录阶段 二次MD5文件校验中
                            CDeviceDataFactory.Instance.VobcContainer.SetProductDeviceState(serverIP,
                            CommonMethod.GetVobcSystemListByType(sysType),
                            Convert.ToString(CommonMethod.GetVobcDeployNameByType(vobcSystemDeployState.FileCheck)));

                            //通知界面刷新
                            CDeviceDataFactory.Instance.VobcContainer.dataModify.Modify();

                            //通过子系统类型 获取该类型对应的更新文件列表
                            if (vobc != null)
                            {
                                VobcCheckFile checkFile = vobc.CheckFileList.Find((VobcCheckFile temp) => temp.vobcSystemType == sysType);

                                //如果为null 说明没有该子子系统更新  发送一个无效帧
                                if (null == checkFile)
                                {
                                    checkFile = new VobcCheckFile();
                                    checkFile.vobcSystemType = sysType;
                                    checkFile.vobcFileTypeList = new List<vobcFileType>();
                                    checkFile.vobcFilePathList = new Dictionary<string, string>();
                                }

                                //以校验帧形式 回复下位机进行第二次MD5校验
                                VOBCCommand command = new VOBCCommand(vobc.Ip, Convert.ToInt32(vobc.Port), vobc.ProductID, vobcCommandType.CcovGetMD5, checkFile);
                                CommandQueue.instance.m_CommandQueue.Enqueue(command);

                                //记录日志
                                LogManager.InfoLog.LogCommunicationInfo("DataAnalysis", "VOBCDataAnalysis", "向CCOV发送VOBC设备" + vobc.ProductID + "子子系统" + checkFile.vobcSystemType.ToString() + "的MD5校验信息");

                            }
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                LogManager.InfoLog.LogCommunicationError("Socket_TCPClient", "Me_ReceiveMessage", ex.Message + ex.StackTrace + ex.Source);
            }
        }

        #endregion

        #region 私有处理函数

        /// <summary>
        /// 拆包
        /// </summary>
        /// <returns></returns>
        private static List<byte[]> PacketDisassembly(byte[] Data)
        {
            //分包（若有多包数据黏在一起）
            List<byte[]> dataList = new List<byte[]>();

            //当前拆包索引
            int dataIndex = 0;

            //数据长度大于当前拆包索引则继续处理
            while (Data.Length > dataIndex)
            {
                //取数据长度
                string le = Convert.ToString(Data[dataIndex + 0], 16).PadLeft(2, '0') + Convert.ToString(Data[dataIndex + 1], 16).PadLeft(2, '0');
                Int32 dataLength = Convert.ToInt32(le, 16);

                //截取一包数据
                byte[] tmpData = Data.Skip(dataIndex).Take(dataLength + 2).ToArray();

                //索引赋值
                dataIndex += tmpData.Length;

                //数据添加至集合
                dataList.Add(tmpData);

                /*//判定帧头FFFE
                if (Data[0] == 0xFF && Data[1] == 0xFE)
                {
                    //取数据长度
                    string le = Convert.ToString(Data[dataIndex + 2], 16).PadLeft(2, '0') + Convert.ToString(Data[dataIndex + 3], 16).PadLeft(2, '0');
                    Int32 dataLength = Convert.ToInt32(le,16);

                    //截取一包数据
                    byte[] tmpData = Data.Skip(dataIndex).Take(dataLength + 6).ToArray();

                    //索引赋值
                    dataIndex += tmpData.Length;

                    //去除FFFE协议头尾
                    FFFE.FFFEAnalysis(ref tmpData);

                    //数据添加至集合
                    dataList.Add(tmpData);

                }
                else
                {
                    break;
                }*/
            }

            return dataList;

        }

        /// <summary>
        /// 获取vobc子系统类型
        /// </summary>
        /// <param name="recvServerData">数据帧</param>
        /// <param name="iter">子系统类型判定数据位索引</param>
        /// <returns>vobc子系统类型</returns>
        private static Common.vobcSystemType getVobcSystemType(byte[] recvServerData, int iter)
        {
            //声明enum对象
            Common.vobcSystemType sysType = new Common.vobcSystemType();

            //依据输入参数 识别出子系统类型
            switch (recvServerData[iter])
            {
                case 0x01:
                    sysType = Common.vobcSystemType.ATP_1;
                    break;
                case 0x02:
                    sysType = Common.vobcSystemType.ATP_2;
                    break;
                case 0x03:
                    sysType = Common.vobcSystemType.ATP_3;
                    break;
                case 0x04:
                    sysType = Common.vobcSystemType.ATO_1;
                    break;
                case 0x05:
                    sysType = Common.vobcSystemType.ATO_2;
                    break;
                case 0x06:
                    sysType = Common.vobcSystemType.COM_1;
                    break;
                case 0x07:
                    sysType = Common.vobcSystemType.COM_2;
                    break;
                case 0x08:
                    sysType = Common.vobcSystemType.MMI;
                    break;
                case 0x09:
                    sysType = Common.vobcSystemType.CCOV;
                    break;
                default:
                    sysType = Common.vobcSystemType.INVALID;
                    break;
            }

            return sysType;

        }

        /// <summary>
        /// 获取vobc更新文件类型
        /// </summary>
        /// <param name="recvServerData">数据帧</param>
        /// <param name="iter">更新文件类型判定数据位索引</param>
        /// <returns>更新文件类型</returns>
        private static Common.vobcFileType getVobcFileType(byte[] recvServerData, int iter)
        {
            //声明enum对象
            Common.vobcFileType fileType = new Common.vobcFileType();

            //依据输入参数 识别出子系统类型
            switch (recvServerData[iter])
            {
                case 0x01:
                    fileType = Common.vobcFileType.CORE;
                    break;
                case 0x02:
                    fileType = Common.vobcFileType.DATA;
                    break;
                case 0x04:
                    fileType = Common.vobcFileType.NVRAM;
                    break;
                case 0x08:
                    fileType = Common.vobcFileType.BootLoader;
                    break;
                case 0x16:
                    fileType = Common.vobcFileType.CCOVConfig;
                    break;
                default:
                    fileType = Common.vobcFileType.INVALID;
                    break;
            }

            return fileType;

        }

        /// <summary>
        /// 依据输入参数 计算更新百分比
        /// </summary>
        /// <param name="recvServerData"></param>
        /// <param name="iter"></param>
        /// <returns></returns>
        private static int getUpdatePersent(byte[] recvServerData, int iter)
        {
            //获取传输总帧数
            byte[] recvTotalNum = new byte[10];
            recvTotalNum[0] = recvServerData[iter + 6];
            recvTotalNum[1] = recvServerData[iter + 5];
            recvTotalNum[2] = recvServerData[iter + 4];
            recvTotalNum[3] = recvServerData[iter + 3];
            int totalNum = BitConverter.ToInt32(recvTotalNum, 0);

            //获取已传帧数
            byte[] recvCurrentNum = new byte[10];
            recvCurrentNum[0] = recvServerData[iter + 10];
            recvCurrentNum[1] = recvServerData[iter + 9];
            recvCurrentNum[2] = recvServerData[iter + 8];
            recvCurrentNum[3] = recvServerData[iter + 7];
            int currentNum = BitConverter.ToInt32(recvCurrentNum, 0);

            //返回进度百分比
            return Convert.ToInt32(((float)currentNum / totalNum) * 100);
        }

        /// <summary>
        /// 解析VOBC车辆状态信息数据帧
        /// </summary>
        /// <param name="recvServerData">数据</param>
        /// <returns>解析后的vobc状态信息实体类对象</returns>
        private static VOBCStateInfoClass GetVOBCInfo(byte[] recvServerData)
        {
            //初始下标
            int iter = 4;

            //VOBC状态信息实体类存储对象
            VOBCStateInfoClass _vobcInfo = new VOBCStateInfoClass();
            _vobcInfo.CCOVStatus = "正常";

            try
            {
                #region ATP（及COM板）状态提取

                //ATP应用版本号（4字节）
                _vobcInfo.AtpSoftVersion = GetMontageData(recvServerData, iter, 1, 4, ".");
                iter = iter + 4;

                //ATP数据版本号（8字节）
                _vobcInfo.AtpDataVersion = GetMontageData(recvServerData, iter, 2, 8, ".");
                iter = iter + 8;

                //ATP运行状态（1字节）
                _vobcInfo.AtpStatus = (recvServerData[iter] == CommonConstValue.constValueHEX55) ? "正常" : "故障";
                iter++;

                //COM板状态等同于ATP运行状态
                _vobcInfo.ComStatus = _vobcInfo.AtpStatus;

                #endregion

                #region ATO状态提取

                //ATO应用版本号（4字节）
                _vobcInfo.AtoSoftVersion = GetMontageData(recvServerData, iter, 1, 4, ".");
                iter = iter + 4;

                //ATO数据版本号（8字节）
                _vobcInfo.AtoDataVersion = GetMontageData(recvServerData, iter, 2, 8, ".");
                iter = iter + 8;

                //ATO运行状态（1字节）
                _vobcInfo.AtoStatus = (recvServerData[iter] == CommonConstValue.constValueHEX55) ? "正常" : "故障";
                iter++;

                #endregion

                #region CCOV状态提取

                //CCOV应用版本号（4字节）
                _vobcInfo.CCOVSoftVersion = GetMontageData(recvServerData, iter, 1, 4, ".");
                iter = iter + 4;

                //CCOV数据版本号（8字节）
                _vobcInfo.CCOVDataVersion = GetMontageData(recvServerData, iter, 2, 8, ".");
                iter = iter + 8;

                //CCOV的TFTP服务开启状态（1字节）
                _vobcInfo.AtpTftpStatus = (recvServerData[iter] == CommonConstValue.constValueHEX55) ? "正常" : "故障";
                iter++;

                #endregion

                #region MMI版本号提取

                //MMI应用版本号（4字节）
                _vobcInfo.MmiSoftVersion = GetMontageData(recvServerData, iter, 1, 4, ".");
                iter = iter + 4;

                #endregion

                #region COM板版本号提取

                //COM1应用版本号（4字节）
                _vobcInfo.Com1SoftVersion = GetMontageData(recvServerData, iter, 1, 4, ".");
                iter = iter + 4;

                //COM2应用版本号（4字节）
                _vobcInfo.Com2SoftVersion = GetMontageData(recvServerData, iter, 1, 4, ".");
                iter = iter + 4;

                #endregion

                #region MMI状态提取

                //ATP与MMI通信状态（1字节）
                _vobcInfo.MmiStatus = (recvServerData[iter] == CommonConstValue.constValueHEX55) ? "正常" : "故障";
                iter++;

                #endregion

                #region 其他状态提取
                //列车运行模式（1字节）
                if (recvServerData[iter] == 0x00)
                {
                    _vobcInfo.OperationMode = "CM";
                }
                else if (recvServerData[iter] == 0x01)
                {
                    _vobcInfo.OperationMode = "AM";
                }
                else if (recvServerData[iter] == 0x02)
                {
                    _vobcInfo.OperationMode = "RM";
                }

                iter++;
                //列车运行速度（2字节）
                _vobcInfo.OperationSpeed = Convert.ToInt32(GetMontageData(recvServerData, iter, 1, 2, ""));

                iter = iter + 2;
                //VOBC的IP地址（4字节）
                _vobcInfo.TrainIP = GetMontageData(recvServerData, iter, 1, 4, ".");

                iter = iter + 4;
                //无线网络状态（1字节）
                _vobcInfo.WirelessStatus = (recvServerData[iter] == CommonConstValue.constValueHEX55) ? "正常" : "故障";

                iter++;
                //无线关联信噪比（1字节）
                _vobcInfo.WirelessSNR = Convert.ToInt32(recvServerData[iter].ToString());

                iter++;
                //TC1端状态（1字节）
                if (recvServerData[iter] == 0x00)
                {
                    _vobcInfo.Tc1Status = "无效";
                }
                else if (recvServerData[iter] == CommonConstValue.constValueHEX55)
                {
                    _vobcInfo.Tc1Status = "正常";
                }
                else
                {
                    _vobcInfo.Tc1Status = "故障";
                }

                iter++;
                //TC2端状态（1字节）
                if (recvServerData[iter] == 0x00)
                {
                    _vobcInfo.Tc2Status = "无效";
                }
                else if (recvServerData[iter] == CommonConstValue.constValueHEX55)
                {
                    _vobcInfo.Tc2Status = "正常";
                }
                else
                {
                    _vobcInfo.Tc2Status = "故障";
                }

                iter++;
                //列车位置（1字节）
                if (recvServerData[iter] == CommonConstValue.constValueHEX55)
                {
                    _vobcInfo.TrainPosition = "正线";
                }
                else if (recvServerData[iter] == CommonConstValue.constValueHEXAA)
                {
                    _vobcInfo.TrainPosition = "非正线";
                }
                else
                {
                    _vobcInfo.TrainPosition = "无效";
                }

                #endregion
            }
            catch (Exception ex)
            {
                LogManager.InfoLog.LogCommunicationError("DataAnalysis", "GetVOBCInfo", ex.Message);
            }

            return _vobcInfo;
        }

        /// <summary>
        /// 获取拼接数据
        /// </summary>
        /// <param name="data">拼接数据数组</param>
        /// <param name="startIndex">需要拼接的数据起始索引</param>
        /// <param name="montageInterval">间隔多少字节拼接符号</param>
        /// <param name="length">共拼接多少长度</param>
        /// <param name="montageSign">拼接数据的连接字符串</param>
        /// <returns>拼接好的数据</returns>
        private static string GetMontageData(byte[] data, int startIndex,
            int montageInterval, int length, string montageSign)
        {

            string reValue = string.Empty;

            int loopCount = 1;
            //遍历数据并拼接
            for (int i = startIndex; i < startIndex + length; i++)
            {
                //拼接数据
                reValue += Convert.ToString(data[i]);

                //拼接.（最后一个字节后不加.）
                if (i != (startIndex + length - 1) && loopCount == montageInterval)
                {
                    reValue += montageSign;
                    loopCount = 1;
                }
                else
                {
                    loopCount++;
                }
            }

            return reValue;

        }

        #region 设置文件校验请求回复状态

        /// <summary>
        /// 设置VOBCATP校验状态
        /// </summary>
        /// <param name="recvServerData">数据</param>
        /// <param name="iter">帧类型码所在位索引下标</param>
        /// <param name="binarySysData">校验文件类型码 二进制值</param>
        private static void SetVOBCATPCheckState(byte[] recvServerData, int iter, string binarySysData)
        {

            //内核文件
            if (binarySysData[7] == '1')
            {
                _updateFileState.AtpCoreVeriFlag = (recvServerData[(iter + 3)] == 0x55) ? true : false;

                //异或 取得文件传输验证结果（false即验证失败）
                _updateFileState.VeriResult = _updateFileState.VeriResult & _updateFileState.AtpCoreVeriFlag;
            }
            //数据文件
            if (binarySysData[6] == '1')
            {
                _updateFileState.AtpDataVeriFlag = (recvServerData[(iter + 4)] == 0x55) ? true : false;

                //异或 取得文件传输验证结果（false即验证失败）
                _updateFileState.VeriResult = _updateFileState.VeriResult & _updateFileState.AtpDataVeriFlag;
            }
            //配置文件
            if (binarySysData[5] == '1')
            {
                _updateFileState.AtpNvramVeriFlag = (recvServerData[(iter + 5)] == 0x55) ? true : false;

                //异或 取得文件传输验证结果（false即验证失败）
                _updateFileState.VeriResult = _updateFileState.VeriResult & _updateFileState.AtpNvramVeriFlag;
            }
            //引导文件
            if (binarySysData[4] == '1')
            {
                _updateFileState.AtpBootVeriFlag = (recvServerData[(iter + 6)] == 0x55) ? true : false;

                //异或 取得文件传输验证结果（false即验证失败）
                _updateFileState.VeriResult = _updateFileState.VeriResult & _updateFileState.AtpBootVeriFlag;
            }


        }

        /// <summary>
        /// 设置VOBCATO校验状态
        /// </summary>
        /// <param name="recvServerData">数据</param>
        /// <param name="iter">帧类型码所在位索引下标</param>
        /// <param name="binarySysData">校验文件类型码 二进制值</param>
        private static void SetVOBCATOCheckState(byte[] recvServerData, int iter, string binarySysData)
        {

            //内核文件
            if (binarySysData[7] == '1')
            {
                _updateFileState.AtoCoreVeriFlag = (recvServerData[(iter + 3)] == 0x55) ? true : false;

                //异或 取得文件传输验证结果（false即验证失败）
                _updateFileState.VeriResult = _updateFileState.VeriResult & _updateFileState.AtoCoreVeriFlag;
            }
            //数据文件
            if (binarySysData[6] == '1')
            {
                _updateFileState.AtoDataVeriFlag = (recvServerData[(iter + 4)] == 0x55) ? true : false;

                //异或 取得文件传输验证结果（false即验证失败）
                _updateFileState.VeriResult = _updateFileState.VeriResult & _updateFileState.AtoDataVeriFlag;
            }
            //配置文件
            if (binarySysData[5] == '1')
            {
                _updateFileState.AtoNvramVeriFlag = (recvServerData[(iter + 5)] == 0x55) ? true : false;

                //异或 取得文件传输验证结果（false即验证失败）
                _updateFileState.VeriResult = _updateFileState.VeriResult & _updateFileState.AtoNvramVeriFlag;
            }
            //引导文件
            if (binarySysData[4] == '1')
            {
                _updateFileState.AtoBootVeriFlag = (recvServerData[(iter + 6)] == 0x55) ? true : false;

                //异或 取得文件传输验证结果（false即验证失败）
                _updateFileState.VeriResult = _updateFileState.VeriResult & _updateFileState.AtoBootVeriFlag;
            }


        }

        /// <summary>
        /// 设置VOBCCOM校验状态
        /// </summary>
        /// <param name="recvServerData">数据</param>
        /// <param name="iter">帧类型码所在位索引下标</param>
        /// <param name="binarySysData">校验文件类型码 二进制值</param>
        private static void SetVOBCCOMCheckState(byte[] recvServerData, int iter, string binarySysData)
        {

            //内核文件
            if (binarySysData[7] == '1')
            {
                _updateFileState.ComCoreVeriFlag = (recvServerData[(iter + 3)] == 0x55) ? true : false;

                //异或 取得文件传输验证结果（false即验证失败）
                _updateFileState.VeriResult = _updateFileState.VeriResult & _updateFileState.ComCoreVeriFlag;
            }
            //引导文件
            if (binarySysData[4] == '1')
            {
                _updateFileState.ComBootVeriFlag = (recvServerData[(iter + 6)] == 0x55) ? true : false;

                //异或 取得文件传输验证结果（false即验证失败）
                _updateFileState.VeriResult = _updateFileState.VeriResult & _updateFileState.ComBootVeriFlag;
            }


        }

        /// <summary>
        /// 设置VOBCMMI校验状态
        /// </summary>
        /// <param name="recvServerData">数据</param>
        /// <param name="iter">帧类型码所在位索引下标</param>
        /// <param name="binarySysData">校验文件类型码 二进制值</param>
        private static void SetVOBCMMICheckState(byte[] recvServerData, int iter, string binarySysData)
        {

            //内核文件
            if (binarySysData[7] == '1')
            {
                _updateFileState.MmiCoreVeriFlag = (recvServerData[(iter + 3)] == 0x55) ? true : false;

                //异或 取得文件传输验证结果（false即验证失败）
                _updateFileState.VeriResult = _updateFileState.VeriResult & _updateFileState.MmiCoreVeriFlag;
            }
            //数据文件
            /*if (binarySysData[6] == '1')
            {
                _updateFileState.MmiDataVeriFlag = (recvServerData[(iter + 4)] == 0x55) ? true : false;

                //异或 取得文件传输验证结果（false即验证失败）
                _updateFileState.VeriResult = _updateFileState.VeriResult & _updateFileState.MmiDataVeriFlag;
            }*/
            //配置文件
            if (binarySysData[5] == '1')
            {
                _updateFileState.MmiNvramVeriFlag = (recvServerData[(iter + 5)] == 0x55) ? true : false;

                //异或 取得文件传输验证结果（false即验证失败）
                _updateFileState.VeriResult = _updateFileState.VeriResult & _updateFileState.MmiNvramVeriFlag;
            }
            //引导文件
            if (binarySysData[4] == '1')
            {
                _updateFileState.MmiBootVeriFlag = (recvServerData[(iter + 6)] == 0x55) ? true : false;

                //异或 取得文件传输验证结果（false即验证失败）
                _updateFileState.VeriResult = _updateFileState.VeriResult & _updateFileState.MmiBootVeriFlag;
            }

        }

        /// <summary>
        /// 设置VOBCCCOV校验状态
        /// </summary>
        /// <param name="recvServerData">数据</param>
        /// <param name="iter">帧类型码所在位索引下标</param>
        /// <param name="binarySysData">校验文件类型码 二进制值</param>
        private static void SetVOBCCCOVCheckState(byte[] recvServerData, int iter, string binarySysData)
        {

            //内核文件
            if (binarySysData[7] == '1')
            {
                _updateFileState.CcovCoreVeriFlag = (recvServerData[(iter + 3)] == 0x55) ? true : false;

                //异或 取得文件传输验证结果（false即验证失败）
                _updateFileState.VeriResult = _updateFileState.VeriResult & _updateFileState.CcovCoreVeriFlag;
            }
            //数据文件
            if (binarySysData[6] == '1')
            {
                _updateFileState.CcovDataVeriFlag = (recvServerData[(iter + 4)] == 0x55) ? true : false;

                //异或 取得文件传输验证结果（false即验证失败）
                _updateFileState.VeriResult = _updateFileState.VeriResult & _updateFileState.CcovDataVeriFlag;
            }
            //配置文件
            /*if (binarySysData[5] == '1')
            {
                _updateFileState.CcovNvramVeriFlag = (recvServerData[(iter + 5)] == 0x55) ? true : false;

                //异或 取得文件传输验证结果（false即验证失败）
                _updateFileState.VeriResult = _updateFileState.VeriResult & _updateFileState.CcovNvramVeriFlag;
            }*/
            //引导文件
            if (binarySysData[4] == '1')
            {
                _updateFileState.CcovBootVeriFlag = (recvServerData[(iter + 6)] == 0x55) ? true : false;

                //异或 取得文件传输验证结果（false即验证失败）
                _updateFileState.VeriResult = _updateFileState.VeriResult & _updateFileState.CcovBootVeriFlag;
            }
            //CCOV配置文件
            if (binarySysData[3] == '1')
            {
                _updateFileState.CcovConfigVeriFlag = (recvServerData[(iter + 7)] == 0x55) ? true : false;

                //异或 取得文件传输验证结果（false即验证失败）
                _updateFileState.VeriResult = _updateFileState.VeriResult & _updateFileState.CcovConfigVeriFlag;
            }

        } 

        #endregion

        #region 设置文件更新完成状态

        /// <summary>
        /// 设置VOBCATP更新完成状态
        /// </summary>
        /// <param name="recvServerData">数据</param>
        /// <param name="vobcFType">子子系统更新文件类型</param>
        /// <param name="updateResultIter">更新完成 结果码 索引位</param>
        private static void SetVOBCATPcompleteState(byte[] recvServerData,vobcFileType vobcFType, int updateResultIter)
        {
            switch (vobcFType)
            {
                //内核文件
                case vobcFileType.CORE:
                    _updateFileState.AtpCoreCompleteFlag &= (recvServerData[updateResultIter] == 0x55) ? true : false;
                    break;
                //数据文件
                case vobcFileType.DATA:
                    _updateFileState.AtpDataCompleteFlag &= (recvServerData[updateResultIter] == 0x55) ? true : false;
                    break;
                //NvRam文件
                case vobcFileType.NVRAM:
                    _updateFileState.AtpNvramCompleteFlag &= (recvServerData[updateResultIter] == 0x55) ? true : false;
                    break;
                //引导文件
                case vobcFileType.BootLoader:
                    _updateFileState.AtpBootCompleteFlag &= (recvServerData[updateResultIter] == 0x55) ? true : false;
                    break;
                //无效值
                case vobcFileType.INVALID:
                    //TODO
                    break;
                //默认
                default:
                    //TODO
                    break;
            }     

        }

        /// <summary>
        /// 设置VOBCATO更新完成状态
        /// </summary>
        /// <param name="recvServerData">数据</param>
        /// <param name="vobcFType">子子系统更新文件类型</param>
        /// <param name="updateResultIter">更新完成 结果码 索引位</param>
        private static void SetVOBCATOcompleteState(byte[] recvServerData, vobcFileType vobcFType, int updateResultIter)
        {
            switch (vobcFType)
            {
                //内核文件
                case vobcFileType.CORE:
                    _updateFileState.AtoCoreCompleteFlag &= (recvServerData[updateResultIter] == 0x55) ? true : false;
                    break;
                //数据文件
                case vobcFileType.DATA:
                    _updateFileState.AtoDataCompleteFlag &= (recvServerData[updateResultIter] == 0x55) ? true : false;
                    break;
                //NvRam文件
                case vobcFileType.NVRAM:
                    _updateFileState.AtoNvramCompleteFlag &= (recvServerData[updateResultIter] == 0x55) ? true : false;
                    break;
                //引导文件
                case vobcFileType.BootLoader:
                    _updateFileState.AtoBootCompleteFlag &= (recvServerData[updateResultIter] == 0x55) ? true : false;
                    break;
                //无效值
                case vobcFileType.INVALID:
                    //TODO
                    break;
                //默认
                default:
                    //TODO
                    break;
            }

        }

        /// <summary>
        /// 设置VOBCCOM更新完成状态
        /// </summary>
        /// <param name="recvServerData">数据</param>
        /// <param name="vobcFType">子子系统更新文件类型</param>
        /// <param name="updateResultIter">更新完成 结果码 索引位</param>
        private static void SetVOBCCOMcompleteState(byte[] recvServerData, vobcFileType vobcFType, int updateResultIter)
        {
            switch (vobcFType)
            {
                //内核文件
                case vobcFileType.CORE:
                    _updateFileState.ComCoreCompleteFlag &= (recvServerData[updateResultIter] == 0x55) ? true : false;
                    break;
                //引导文件
                case vobcFileType.BootLoader:
                    _updateFileState.ComBootCompleteFlag &= (recvServerData[updateResultIter] == 0x55) ? true : false;
                    break;
                //无效值
                case vobcFileType.INVALID:
                    //TODO
                    break;
                //默认
                default:
                    //TODO
                    break;
            }

        }

        /// <summary>
        /// 设置VOBCMMI更新完成状态
        /// </summary>
        /// <param name="recvServerData">数据</param>
        /// <param name="vobcFType">子子系统更新文件类型</param>
        /// <param name="updateResultIter">更新完成 结果码 索引位</param>
        private static void SetVOBCMMIcompleteState(byte[] recvServerData, vobcFileType vobcFType, int updateResultIter)
        {
            switch (vobcFType)
            {
                //内核文件
                case vobcFileType.CORE:
                    _updateFileState.MmiCoreCompleteFlag &= (recvServerData[updateResultIter] == 0x55) ? true : false;
                    break;
                //配置文件（名称写错了，实际对应路径MMIConfig.xml）
                case vobcFileType.NVRAM:
                    _updateFileState.MmiNvramCompleteFlag &= (recvServerData[updateResultIter] == 0x55) ? true : false;
                    break;
                //引导文件
                case vobcFileType.BootLoader:
                    _updateFileState.MmiBootCompleteFlag &= (recvServerData[updateResultIter] == 0x55) ? true : false;
                    break;
                //无效值
                case vobcFileType.INVALID:
                    //TODO
                    break;
                //默认
                default:
                    //TODO
                    break;
            }

        }

        /// <summary>
        /// 设置VOBCCCOV更新完成状态
        /// </summary>
        /// <param name="recvServerData">数据</param>
        /// <param name="vobcFType">子子系统更新文件类型</param>
        /// <param name="updateResultIter">更新完成 结果码 索引位</param>
        private static void SetVOBCCCOVcompleteState(byte[] recvServerData, vobcFileType vobcFType, int updateResultIter)
        {
            switch (vobcFType)
            {
                //内核文件
                case vobcFileType.CORE:
                    _updateFileState.CcovCoreCompleteFlag &= (recvServerData[updateResultIter] == 0x55) ? true : false;
                    break;
                //数据文件
                case vobcFileType.DATA:
                    _updateFileState.CcovDataCompleteFlag &= (recvServerData[updateResultIter] == 0x55) ? true : false;
                    break;
                //NvRam文件，实际是真正的CCOV配置文件
                case vobcFileType.NVRAM:
                    _updateFileState.CcovNvramCompleteFlag &= (recvServerData[updateResultIter] == 0x55) ? true : false;
                    break;
                //CCOV配置文件
                case vobcFileType.CCOVConfig:
                    _updateFileState.CcovConfigCompleteFlag &= (recvServerData[updateResultIter] == 0x55) ? true : false;
                    break;
                //引导文件
                case vobcFileType.BootLoader:
                    _updateFileState.CcovBootCompleteFlag &= (recvServerData[updateResultIter] == 0x55) ? true : false;
                    break;
                //无效值
                case vobcFileType.INVALID:
                    //TODO
                    break;
                //默认
                default:
                    //TODO
                    break;
            }

        }

        #endregion

        private static void HeartBeatTimerInit()
        {
            timerHB.Stop();
            timerHB.Close();
            timerHB.Interval = 15000;
            timerHB.AutoReset = false;
            timerHB.Start();
            timerHB.Elapsed += new System.Timers.ElapsedEventHandler(timerHB_Elapsed);
        }

        private static void timerHB_Elapsed(object sender, EventArgs e)
        {
            timeout = true;
        }

        #endregion

    }
}
