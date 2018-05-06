using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace RemoteDeploy.NetworkService
{
    public class Udp
    {
        public static void Send(string msg)
        {
            Socket sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            EndPoint point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6000);
            sendSocket.SendTo(Encoding.UTF8.GetBytes(msg), point);
            sendSocket.Close();
        }


        #region 封装by sds

        #region 变量

        /// <summary>
        /// 用于UDP接收的网络服务类
        /// </summary>
        private static UdpClient udpcRecv = null;

        #endregion


        /// <summary>
        /// 依据输入参数发送数据
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="serverIP"></param>
        /// <param name="serverPort"></param>
        /// <returns></returns>
        public static bool SendTo(byte[] msg, string serverIP, int serverPort)
        {
            //返回值
            bool resultValue = true;

            //创建SocketUDP对象
            Socket sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                //组合网络地址
                EndPoint point = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);

                //执行发送数据函数并获得返回值
                int sendResult = sendSocket.SendTo(msg, point);

                //判定发送是否成功
                resultValue = (sendResult == msg.Length) ? true : false;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                resultValue = false;
            }
            finally
            {
                //释放SocketUDP对象
                sendSocket.Close();
            }

            return resultValue;

        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="localIP"></param>
        /// <param name="localPort"></param>
        /// <returns></returns>
        public static byte[] Recv(string localIP, int localPort)
        {
            try
            {

                //IP和监听端口号
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any
                    /*IPAddress.Parse(localIP)*/, localPort);

                udpcRecv = (null == udpcRecv) ? new UdpClient(endPoint) : udpcRecv;

                //接收数据
                byte[] bytRecv = udpcRecv.Receive(ref endPoint);

                return bytRecv;

            }
            catch (Exception ex)
            {
                throw ex;
            }

        } 

        #endregion

        public static string Recv()
        {
            Socket recvSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            recvSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6001));
            EndPoint point = new IPEndPoint(IPAddress.Any, 0);
            byte[] buffer = new byte[1024];
            int length = recvSocket.ReceiveFrom(buffer, ref point);
            string message = Encoding.UTF8.GetString(buffer, 0, length);
            recvSocket.Close();
            return message;
        }
    }
}
