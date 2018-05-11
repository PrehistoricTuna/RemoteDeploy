using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace RemoteDeploy.NetworkService
{
    /// <summary>
    /// tcp连接类
    /// </summary>
    class Class1
    {
            static TcpClient tcp_pc = new TcpClient();
            static bool IslinkError = true;

            private Class1()
            {
                StaticLink();
                Thread thread = new Thread(() =>
                {
                    while (true)
                    {
                        if (IslinkError)
                        {
                            tcp_pc = null;
                            tcp_pc = new TcpClient();
                            Console.WriteLine("======================");
                            Console.WriteLine("3秒钟后重新链接服务器");
                            Thread.Sleep(3000);
                            Console.WriteLine("重新链接服务器");
                            Console.WriteLine("======================");
                            StaticLink();
                        }
                        Thread.Sleep(1);
                    }
                });
                thread.Start();
                Console.Read();
            }

            private void StaticLink()
            {
                IslinkError = false;
                try
                {
                    AsyncCallback asynccallback = new AsyncCallback(StaticSendMsg);
                    IAsyncResult result = tcp_pc.BeginConnect("10.0.0.217", 40000, asynccallback, null);
                    tcp_pc.EndConnect(result);
                    Console.WriteLine("----------------------------------------------");
                    Console.WriteLine("客户端连接服务器成功");
                    Console.WriteLine("可以向服务端发送消息");
                    Console.WriteLine("----------------------------------------------");
                    Console.WriteLine("");
                }
                catch (Exception)
                {
                    Console.WriteLine("----------------------------------------------");
                    Console.WriteLine("客户端连接服务器失败");
                    Console.WriteLine("----------------------------------------------");
                    Console.WriteLine("");
                    IslinkError = true;
                    return;
                }
                string ml;
                do
                {
                    ml = Console.ReadLine();
                    if (ml != "n")
                    {
                        try
                        {
                            tcp_pc.Client.Send(Encoding.UTF8.GetBytes(ml));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("与服务端断开链接1");
                            //关闭释放资源  
                            tcp_pc.Client.Close();
                            IslinkError = true;
                            break;
                        }
                    }

                } while (ml != "n");
                tcp_pc.Client.Close();
            }

            private void StaticSendMsg(IAsyncResult result)
            {
                byte[] bytes = new byte[1024];
                int a = 0;
                do
                {
                    try
                    {
                        a = tcp_pc.Client.Receive(bytes);
                        if (a > 0)
                        {
                            string b = System.Text.Encoding.UTF8.GetString(bytes, 0, a);
                            Console.WriteLine("客户端：" + b);
                        }
                    }
                    catch (Exception)
                    {
                        IslinkError = true;
                        tcp_pc.Client.Close();
                        Console.WriteLine("与服务端断开链接");
                        return;
                    }
                } while (a > 0);
            }
        }  
    }
