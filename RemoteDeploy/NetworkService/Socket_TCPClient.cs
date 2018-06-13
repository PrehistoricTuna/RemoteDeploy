using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;
using TCT.ShareLib.LogManager;
using System.Windows;

namespace RemoteDeploy.NetworkService
{
    /// <summary>
    /// Copyright (C) 2018 TCT
    /// Socket_TCPClient帮助类
    /// </summary>
    public class Socket_TCPClient
    {

        #region 变量

        /// <summary>
        /// 接收数据字节数据定义(默认1024字节)
        /// </summary>
        private byte[] bytReceive = new byte[1024];

        /// <summary>
        /// 要通信的服务端IP
        /// </summary>
        private string serverIP;

        /// <summary>
        /// 要通信的服务端端口
        /// </summary>
        private int serverPort;

        /// <summary>
        /// socket客户端对象
        /// </summary>
        public Socket clientSocket =null;

        /// <summary>
        /// socket是否可用
        /// </summary>
        public bool IsSocketEnable = true;

        ///// <summary>
        ///// 当前是否处于socket重连状态
        ///// </summary>
        //public bool IsReConnectionSocketState = false;

        Thread communicateThread = null;

        public string ServerIP { get { return serverIP; } }
        public int ServerPort { get { return serverPort; } }

        /// <summary>
        /// 回调委托
        /// </summary>
        /// <returns></returns>
        public delegate void BackData(byte[] receData);

        /// <summary>
        /// 数据回调
        /// </summary>
        public event BackData EBackData;

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serverIP">接收数据绑定的IP</param>
        /// <param name="serverPort">接收数据绑定的端口</param>
        public Socket_TCPClient(string _serverIP, int _serverPort)
        {
            try
            {
                serverIP = _serverIP;
                serverPort = _serverPort;
                Connection();
            }
            catch (Exception ex)
            {
                LogManager.InfoLog.LogCommunicationError("Socket_TCPClient", "Socket_TCPClient", ex.Message);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        private void Connection()
        {
            try
            {
                //IP和监听端口号
                IPEndPoint serverIpep = new IPEndPoint(
                            IPAddress.Parse(ServerIP), ServerPort);

                //声明实例
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //连接服务端
                clientSocket.Connect(serverIpep);
                //MessageBox.Show("111111111111111111111");
                //通过进行获取TCP连接  
                communicateThread = new Thread(Me_ReceiveMessage);
                communicateThread.IsBackground = true;
                communicateThread.Start(clientSocket);

                ////已建立成功
                //IsReConnectionSocketState = false;

            }
            catch 
            {
                ////异常  继续重新创建连接
                //IsReConnectionSocketState = true;
                //释放socket资源
                //clientSocket.Close();
                //clientSocket.Dispose();

                //释放内存资源
                //GC.Collect();
                //GC.WaitForPendingFinalizers();
                Socket_TCPClient_Dispose();
            }
        }


        /// <summary>  
        /// 接收消息  
        /// </summary>  
        /// <param name="_objClientSocket">当前socket对象</param>  
        private void Me_ReceiveMessage(object _objClientSocket)
        {
            try
            {
                while (IsSocketEnable)
                {
                    //类型转换
                    Socket clientSocket = _objClientSocket as Socket;

                    //可用资源大于0 读取数据
                    if (clientSocket.Available > 0)
                    {
                        //数据接收，Receive为阻塞型
                        int receiveNumber = clientSocket.Receive(bytReceive, 0, bytReceive.Length, 0);

                        if (receiveNumber > 0)
                        {
                            //去除无效的数据和空数据部分
                            byte[] validData = Me_ConvertReceiveVaildData(receiveNumber);

                            string msg = string.Empty;
                            foreach (byte item in validData)
                            {
                                msg += " " + Convert.ToString(item, 16).PadLeft(2, '0');
                            }

                            msg += "[" + serverIP + ":" + serverPort + "]";

                            LogManager.InfoLog.LogCommunicationInfo("Socket_TCPClient", "Me_ReceiveMessage", msg);

                            //回调函数被注册 返回回调信息
                            if (EBackData != null)
                            {
                                EBackData(validData);
                            }
                        }
                    }
                    else
                    {
                        SpinWait.SpinUntil(() => false, 1);
                    }
                }

            }
            catch (Exception ex)
            {
                LogManager.InfoLog.LogCommunicationError("Socket_TCPClient", "Me_ReceiveMessage", ex.Message+ex.StackTrace+ex.Source);
            }
        }

        /// <summary>
        /// 发送消息函数
        /// </summary>
        /// <param name="sendData">待发送数据</param>
        /// <returns>发送结构反馈 true=成功 false=失败</returns>
        public bool Me_SendMessage(byte[] sendData)
        {
            //定义返回值变量
            bool reResult = false;

            try
            {

                //执行发送数据操作
                int SendBack = clientSocket.Send(sendData);
                
                string msg = string.Empty;
                foreach (byte item in sendData)
                {
                    msg += " " + Convert.ToString(item, 16).PadLeft(2, '0');
                }

                msg += "[" + serverIP + ":" + serverPort + "]";

                LogManager.InfoLog.LogCommunicationInfo("Socket_TCPClient", "Me_SendMessage", msg);

                //反馈处理结果（发送发回值如果与待发送数据长度一致 代表发送成功）
                reResult = (SendBack == sendData.Length) ? true : false;

            }
            catch (Exception ex)
            {
                LogManager.InfoLog.LogCommunicationError("Socket_TCPClient", "Me_SendMessage", ex.Message);

                /*//非重连状态 开始重新连接
                if (IsReConnectionSocketState)
                {
                    //释放socket资源
                    clientSocket.Close();
                    clientSocket.Dispose();

                    //释放内存资源
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    //发送失败  认定连接失效 重建连接
                    //Connection();

                }*/
            }

            return reResult;

        }


        /// <summary>
        /// 转换出有效的数据
        /// </summary>
        /// <param name="receiveNumber">需要从集合中截取的数据长度</param>
        /// <returns>截取的预期数据</returns>
        private byte[] Me_ConvertReceiveVaildData(int receiveNumber)
        {
            //依据用户数据长度 定义数组大小
            byte[] validData = new byte[receiveNumber];

            //获取有效数据
            for (int i = 0; i < receiveNumber; i++)
            {
                validData[i] = bytReceive[i];
            }

            return validData;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Socket_TCPClient_Dispose()
        {
            try
            {
                IsSocketEnable = false;
                clientSocket.Close();
                clientSocket.Dispose();

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception ex)
            {
                LogManager.InfoLog.LogCommunicationError("Socket_TCPClient", "Socket_TCPClient_Dispose", ex.Message);
            }
        }

    }
}
