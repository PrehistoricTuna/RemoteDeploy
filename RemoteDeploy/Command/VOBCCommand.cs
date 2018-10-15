using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteDeploy.NetworkService;
using System.IO;
using RemoteDeploy.Common;
using RemoteDeploy.Models.VOBC;
using RemoteDeploy.EquData;
using TCT.ShareLib.LogManager;
using RemoteDeploy.DataPack;
using RemoteDeploy.TFTP;
using System.Threading;

namespace RemoteDeploy.Command
{

    public class VOBCCommand : ICommand
    {

        #region 变量

        /// <summary>
        /// VOBC服务端IP地址
        /// </summary>
        public string vobcServerIP { get; private set; }

        /// <summary>
        /// VOBC服务端端口信息
        /// </summary>
        public int vobcServerPort { get; private set; }

        /// <summary>
        /// VOBC车载ID
        /// </summary>
        public string vobcDevID { get; private set; }

        /// <summary>
        /// 指令类型
        /// </summary>
        public vobcCommandType vobcCommandT { get; private set; }
        private Dictionary<string, string> vobcPathList = new Dictionary<string, string>();
        public Dictionary<string, string> VobcPathList
        {
            get { return vobcPathList; }
            set { vobcPathList = value; }
        }
        private VobcCheckFile m_checkFile = new VobcCheckFile();

        public VobcCheckFile CheckFile
        {
            get { return m_checkFile; }
            set { m_checkFile = value; }
        }

        #endregion

        /// <summary>
        /// VOBC命令函数
        /// </summary>
        /// <param name="serverIP">产品IP</param>
        /// <param name="serverPort">产品端口</param>
        /// <param name="vobcID">VOBCID</param>
        /// <param name="commandT">VOBC指令类型</param>
        public VOBCCommand(string serverIP, int serverPort,
            string vobcID, vobcCommandType commandT)
        {
            vobcServerIP = serverIP;
            vobcServerPort = serverPort;
            vobcDevID = vobcID;
            vobcCommandT = commandT;
        }
        /// <summary>
        /// VOBC命令函数
        /// </summary>
        /// <param name="serverIP">产品IP</param>
        /// <param name="serverPort">产品端口</param>
        /// <param name="vobcID">VOBCID</param>
        /// <param name="commandT">VOBC指令类型</param>
        /// <param name="pathList">路径集</param>
        public VOBCCommand(string serverIP, int serverPort,
    string vobcID, vobcCommandType commandT, Dictionary<string, string> pathList)
        {
            vobcServerIP = serverIP;
            vobcServerPort = serverPort;
            vobcDevID = vobcID;
            vobcCommandT = commandT;
            vobcPathList = pathList;
        }
        /// <summary>
        /// VOBC指令函数
        /// </summary>
        /// <param name="serverIP">产品IP</param>
        /// <param name="serverPort">产品端口</param>
        /// <param name="vobcID">VOBCID</param>
        /// <param name="commandT">VOBC指令类型</param>
        /// <param name="checkFile">VOBC文件校验信息实体</param>
        public VOBCCommand(string serverIP, int serverPort,
string vobcID, vobcCommandType commandT, VobcCheckFile checkFile)
        {
            vobcServerIP = serverIP;
            vobcServerPort = serverPort;
            vobcDevID = vobcID;
            vobcCommandT = commandT;
            m_checkFile = checkFile;
        }

        /// <summary>
        /// 执行主函数
        /// </summary>
        /// <returns>返回执行结果</returns>
        public override bool Exec()
        {

            //执行结果
            bool execResult = false;

            #region TCP代码 正式环境使用

            VOBCProduct pro = CDeviceDataFactory.Instance.GetProductByIpPort(vobcServerIP, vobcServerPort);
            if (pro != null)
            {
                //根据指令类型 传输不同指令
                //建链
                if (vobcCommandT == vobcCommandType.buildLink)
                {            
                    //Modified @ 7.7
                    if ((pro.CTcpClient == null) || (pro.CTcpClient.IsSocketEnable != true) || (pro.CTcpClient.clientSocket == null))
                    {
                        //实例化TCP客户端类
                        pro.CTcpClient = new Socket_TCPClient(vobcServerIP, vobcServerPort);
                        //TCP客户端代码 正式环境使用
                        pro.CTcpClient.EBackData += new Socket_TCPClient.BackData(TcpVobc_EBackData);
                    }
                    else
                    {
                        //Modified @ 7.7
                        //释放原对象
                        //pro.CTcpClient.Socket_TCPClient_Dispose();
                        //实例化TCP客户端类
                        //pro.CTcpClient = new Socket_TCPClient(vobcServerIP, vobcServerPort);
                        //TCP客户端代码 正式环境使用
                        //pro.CTcpClient.EBackData += new Socket_TCPClient.BackData(TcpVobc_EBackData);
                    }
                    Thread.Sleep(500);
                    execResult = pro.CTcpClient.Me_SendMessage(DataPack.DataPack.PackBuildLinkRequest(pro));
                    LogManager.InfoLog.LogCommunicationInfo("VOBCCommand", "Exec", vobcDevID + "发送建链帧");
                }
                //重建链
                else if (vobcCommandT == vobcCommandType.rebuildLink)
                {                    
                    //实例化TCP客户端类
                    pro.CTcpClient = new Socket_TCPClient(vobcServerIP, vobcServerPort);
                    //TCP客户端代码 正式环境使用
                    pro.CTcpClient.EBackData += new Socket_TCPClient.BackData(TcpVobc_EBackData);                    
                    Thread.Sleep(500);
                    execResult = pro.CTcpClient.Me_SendMessage(DataPack.DataPack.PackBuildLinkRequest(pro));
                    LogManager.InfoLog.LogCommunicationInfo("VOBCCommand", "Exec", vobcDevID + "发送建链帧");
                }
                //VOBC状态获取
                else if (vobcCommandT == vobcCommandType.vobcInfoRequest)
                {
                    execResult = pro.CTcpClient.Me_SendMessage(DataPack.DataPack.PackVOBCInfoRequest());
                    LogManager.InfoLog.LogCommunicationInfo("VOBCCommand", "Exec", vobcDevID + "VOBC状态获取发送");

                }
                //文件传输请求
                else if (vobcCommandT == vobcCommandType.fileTransRequest)
                {
                    execResult = pro.CTcpClient.Me_SendMessage(DataPack.DataPack.PackFileTransferRequest());
                    LogManager.InfoLog.LogCommunicationInfo("VOBCCommand", "Exec", vobcDevID + "发送传输文件请求！");
                }
                //执行文件发送
                else if (vobcCommandT == vobcCommandType.sendFile)
                {       
                    foreach (KeyValuePair<string, string> path in m_checkFile.vobcFilePathList)
                    {
                        LogManager.InfoLog.LogCommunicationInfo("VOBCCommand", "Exec", "待更新的文件：[" + path + "]");
                        if (File.Exists(path.Key))
                        {
                            try
                            {
                                LogManager.InfoLog.LogCommunicationInfo("VOBCCommand", "Exec", "开始执行文件[" + path + "]的FTP发送！目标地址：" + vobcServerIP);
                                
                                //TFTP.TFTP_Client.UpLoad(vobcServerIP, path);
                                ////设置烧录子子系统在界面中的显示状态--文件上传中 放到SendFile里执行刷新界面，节约command执行时间 Modified @ 9.13
                                //CDeviceDataFactory.Instance.VobcContainer.SetProductDeviceState(vobcServerIP, vobcServerPort,
                                //CommonMethod.GetVobcSystemListByType(m_checkFile.vobcSystemType),
                                //Convert.ToString(CommonMethod.GetVobcDeployNameByType(vobcSystemDeployState.FileUploading)));

                                ////通知界面刷新
                                //CDeviceDataFactory.Instance.VobcContainer.dataModify.Modify();

                                //Modified @ 9.13
                                //string th = Convert.ToString(pro.ProductID);
                                Thread th = new Thread(new ThreadStart(delegate { FTPHelper.FtpUploadBroken(vobcServerIP, path.Key, path.Value); }));
                                th.Start();
                                ////FTP上传文件
                                //bool ret = FTPHelper.FtpUploadBroken(vobcServerIP, path.Key, path.Value);


                                //if (ret == false)
                                //{
                                //    LogManager.InfoLog.LogCommunicationInfo("VOBCCommand", "Exec", "Ftp send file ERROR[" + path + "]的FTP发送！目标地址：" + vobcServerIP);
                                //}
                                //else
                                //{
                                //    LogManager.InfoLog.LogCommunicationInfo("VOBCCommand", "Exec", "文件[" + path + "]的FTP发送完成！");
                                //}
                            }
                            catch (Exception)
                            {
                                LogManager.InfoLog.LogCommunicationInfo("VOBCCommand", "Exec", "Error ");
                            }
                        }
                        else
                        {
                            //TODO
                            LogManager.InfoLog.LogCommunicationInfo("VOBCCommand", "Exec", "本地文件未找到[" + path + "]！");
                        }
                        Thread.Sleep(10);
                    }

                }
                //文件校验请求帧
                else if (vobcCommandT == vobcCommandType.checkFile)
                {
                    ////设置烧录子子系统在界面中的显示状态--文件校验中 放到CheckFile里执行刷新界面，节约command执行时间 Modified @ 9.13
                    //CDeviceDataFactory.Instance.VobcContainer.SetProductDeviceState(vobcServerIP, vobcServerPort,
                    //CommonMethod.GetVobcSystemListByType(m_checkFile.vobcSystemType),
                    //Convert.ToString(CommonMethod.GetVobcDeployNameByType(vobcSystemDeployState.FileCheck)));
                    ////通知界面刷新
                    //CDeviceDataFactory.Instance.VobcContainer.dataModify.Modify();

                    execResult = pro.CTcpClient.Me_SendMessage(DataPack.DataPack.PackFileVerificationRequest(m_checkFile, pro));
                    LogManager.InfoLog.LogCommunicationInfo("VOBCCommand", "Exec", vobcDevID + "文件校验请求发送！");
                }
                //文件更新请求帧
                else if (vobcCommandT == vobcCommandType.startUpdateFile)
                {
                    //设置烧录子子系统在界面中的显示状态--文件更新中
                    //CDeviceDataFactory.Instance.VobcContainer.SetProductDeviceState(vobcServerIP,
                    //CommonMethod.GetVobcSystemListByType(m_checkFile.vobcSystemType),
                    //Convert.ToString(CommonMethod.GetVobcDeployNameByType(vobcSystemDeployState.FileUpdating)));
                    //通知界面刷新
                    //CDeviceDataFactory.Instance.VobcContainer.dataModify.Modify();

                    execResult = pro.CTcpClient.Me_SendMessage(DataPack.DataPack.PackFileUpdateRequest(pro));
                    LogManager.InfoLog.LogCommunicationInfo("VOBCCommand", "Exec", vobcDevID + "文件更新请求发送！");
                }
                //远程重启帧
                else if (vobcCommandT == vobcCommandType.systemRestart)
                {
                    //TODO:暂时不处理，下位机硬件暂不支持
                }
                //停止更新请求帧
                else if (vobcCommandT == vobcCommandType.stopUpdateFile)
                {
                    execResult = pro.CTcpClient.Me_SendMessage(DataPack.DataPack.PackStopUpdateRequest());
                    LogManager.InfoLog.LogCommunicationInfo("VOBCCommand", "Exec", vobcDevID + "文件停止更新请求发送！");
                }
                //远程复位帧
                else if (vobcCommandT == vobcCommandType.systemReset)
                {
                    execResult = pro.CTcpClient.Me_SendMessage(DataPack.DataPack.PackResetRequest());
                    LogManager.InfoLog.LogCommunicationInfo("VOBCCommand", "Exec", vobcDevID + "远程复位请求发送！");
                }
                //断链请求帧
                else if (vobcCommandT == vobcCommandType.cutLink)
                {
                    try
                    {
                        if (pro != null)
                        {
                            execResult = pro.CTcpClient.Me_SendMessage(DataPack.DataPack.PackDisconnectRequest());
                            pro.CTcpClient.Socket_TCPClient_Dispose();
                        }
                        LogManager.InfoLog.LogCommunicationInfo("VOBCCommand", "Exec", vobcDevID + "断链请求发送！");
                    }
                    catch
                    {

                    }
                }
                //CCOV向上位机获取MD5指令
                else if (vobcCommandT == vobcCommandType.CcovGetMD5)
                {
                    execResult = pro.CTcpClient.Me_SendMessage(DataPack.DataPack.PackFileVerificationRequest(m_checkFile, pro));
                    LogManager.InfoLog.LogCommunicationInfo("VOBCCommand", "Exec", vobcDevID + "CCOV获取MD5，向下发送！");
                }
                else
                {
                    //TODO:暂时不处理
                }
            }
            else
            {
                execResult = false;
            }

            #endregion

            //返回执行结果
            return execResult;

        }
        /// <summary>
        /// TCPVOBC数据回调
        /// </summary>
        /// <param name="receData">回调数据</param>
        void TcpVobc_EBackData(byte[] receData)
        {
            VOBCProduct pro = CDeviceDataFactory.Instance.GetProductByIpPort(vobcServerIP, vobcServerPort);
            DataAnalysis.VOBCDataAnalysis(receData, pro.CTcpClient);

            //if (pro != null)
            //{
            //    //第一次建链即触发回调，在收到更新结果回复时在这里直接处理
            //    if (receData[2] == 0x06)
            //    {
            //        pro.WaitForUpdateResult();
            //    }
            //}
            //else
            //{
            //    LogManager.InfoLog.LogCommunicationError("VOBCCommand", "TcpVobc_EBackData", pro.ProductID + "未找到接收到的VOBC对象！");
            //}
        }


    }
}
