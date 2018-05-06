using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using TCT.ShareLib.LogManager;

namespace RemoteDeploy.TFTP
{
    /// <summary>
    /// Copyright (C) 2018 TCT
    /// TFTP_Client帮助类
    /// </summary>
    public static class TFTP_Client
    {

        /// <summary>
        /// 上传文件到指令服务器(win7系统需要打开 TFTP功能)
        /// </summary>
        /// <param name="serverIP">服务器IP地址</param>
        /// <param name="filePath">文件全路径（本地文件全路径，包含文件名称及后缀）</param>
        public static void UpLoad(string serverIP, string filePath)
        {
            //拼接上传指令
            string arg = string.Format("-i {0} put {1}", serverIP, filePath);

            //使用‘tftp.exe’上传文件
            ExecPro("TFTP.EXE", arg);

        }

        /// <summary>
        /// 执行外部进程
        /// </summary>
        /// <param name="proName">进程名</param>
        /// <param name="arg">参数</param>
        private static void ExecPro(string proName, string arg)
        {
            //判定‘tftp.exe’是否存在
            if (File.Exists(string.Format(@"{0}\{1}", Environment.SystemDirectory, proName)))//@"D:\", proName)))//
            {
                //执行命令
                Process pro = new Process();
                pro.StartInfo.FileName = proName;
                pro.StartInfo.Arguments = arg;
                pro.StartInfo.UseShellExecute = false;
                pro.StartInfo.CreateNoWindow = true;
                pro.StartInfo.RedirectStandardOutput = true;
                pro.StartInfo.RedirectStandardError = true;
                pro.Start();

                //打印结果
                LogManager.InfoLog.LogCommunicationInfo("TFTP_Client", "ExecPro", pro.StandardOutput.ReadToEnd());
                LogManager.InfoLog.LogCommunicationInfo("TFTP_Client", "ExecPro", pro.StandardError.ReadToEnd());
                Console.Write(pro.StandardOutput.ReadToEnd());
                Console.Write(pro.StandardError.ReadToEnd());
            }
            else
            {
                Console.Write(string.Format("系统目录下没有{0}!!", proName));
            }
        }
    }
}
