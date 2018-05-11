using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using RemoteDeploy.Models.VOBC;
using TCT.ShareLib.LogManager;

namespace RemoteDeploy.DataPack
{
    //工具软件向VOBC发送数据打包类
    public static class DataPack
    {
        private static byte _atpUpdateFile;
        private static byte _atoUpdateFile;
        private static byte _ccovUpdateFile;
        private static byte _mmiUpdateFile;
        private static byte _comUpdateFile; 

        #region 打包建链请求信息


        /// <summary>
        /// 打包建链请求
        /// </summary>
        /// <returns>帧字节数组</returns>
        public static byte[] PackBuildLinkRequest()
        {
            byte[] pData = new byte[8];
            byte iter = 0;
            pData[iter++] = 0x00;
            pData[iter++] = 0x06;//帧长度
            pData[iter++] = 0x09;//帧类型码
            pData[iter++] = 0x55;//请求建立连接

            UInt32 crc32 =CRC.CRC32(pData, (ushort)(pData.Length - 4), 0);

            pData[iter++] = BitConverter.GetBytes(crc32)[3];
            pData[iter++] = BitConverter.GetBytes(crc32)[2];
            pData[iter++] = BitConverter.GetBytes(crc32)[1];
            pData[iter] = BitConverter.GetBytes(crc32)[0];

            return pData;
            //添加FFFE协议头尾
            //return FFFE.FFFEPack(pData);
            
          
        }

        #endregion

        /*#region 打包第二阶段建链请求信息

        public static byte[] PackBuildLinkSecondRequest()
        {
            byte[] pData = new byte[9];
            byte iter = 0;
            pData[iter++] = 0x00;
            pData[iter++] = 0x07;//帧长度
            pData[iter++] = 0x09;//帧类型码
            pData[iter++] = 0x02;//表明所处阶段
            pData[iter++] = 0x55;//请求建立连接

            UInt32 crc32 = CRC.CRC32(pData, (ushort)(pData.Length - 4), 0);

            pData[iter++] = BitConverter.GetBytes(crc32)[3];
            pData[iter++] = BitConverter.GetBytes(crc32)[2];
            pData[iter++] = BitConverter.GetBytes(crc32)[1];
            pData[iter] = BitConverter.GetBytes(crc32)[0];

            return pData;
        }

        #endregion*/

        #region 打包列车状态请求信息
        /// <summary>
        /// 打包列车状态请求信息
        /// </summary>
        /// <returns>帧字节数组</returns>
        public static byte[] PackVOBCInfoRequest()
        {
            byte[] pData = new byte[8];
            byte iter = 0;
            pData[iter++] = 0x00;
            pData[iter++] = 0x06;//帧长度
            pData[iter++] = 0x07;//帧类型码
            pData[iter++] = 0x55;//获取列车状态请求

            UInt32 crc32 = CRC.CRC32(pData, (ushort)(pData.Length - 4), 0);

            pData[iter++] = BitConverter.GetBytes(crc32)[3];
            pData[iter++] = BitConverter.GetBytes(crc32)[2];
            pData[iter++] = BitConverter.GetBytes(crc32)[1];
            pData[iter] = BitConverter.GetBytes(crc32)[0];

            return pData;
            //添加FFFE协议头尾
            //return FFFE.FFFEPack(pData);
            
          
        }

        #endregion

        #region 打包文件上传请求信息
        /// <summary>
        /// 打包文件上传请求信息
        /// </summary>
        /// <returns>帧字节数组</returns>
        public static byte[] PackFileTransferRequest()
        {
            byte[] pData = new byte[8];
            byte iter = 0;
            pData[iter++] = 0x00;
            pData[iter++] = 0x06;//帧长度
            pData[iter++] = 0x0B;//帧类型码
            pData[iter++] = 0x55;//获取列车状态请求

            UInt32 crc32 = CRC.CRC32(pData, (ushort)(pData.Length - 4), 0);

            pData[iter++] = BitConverter.GetBytes(crc32)[3];
            pData[iter++] = BitConverter.GetBytes(crc32)[2];
            pData[iter++] = BitConverter.GetBytes(crc32)[1];
            pData[iter] = BitConverter.GetBytes(crc32)[0];

            return pData;
            //添加FFFE协议头尾
            //return FFFE.FFFEPack(pData);
            
        }
        #endregion

        #region 打包文件校验请求信息

        /// <summary>
        /// 打包文件校验请求信息
        /// </summary>
        /// <param name="checkFileList">待验证VOBC信息实体类集合</param>
        /// <returns>帧字节数组</returns>
        public static byte[] PackFileVerificationRequest(VobcCheckFile checkFile)
        {
            byte[] pData = new byte[111];
            int iter = 0;
            pData[iter++] = 0x00;
            pData[iter++] = 0x6D;//帧长度
            pData[iter++] = 0x02;//帧类型码

            //填入各子子系统级校验文件类型码
            pData[iter++] =Convert.ToByte(checkFile.vobcSystemType);//子子系统类型码
            pData[iter++] = Convert.ToByte(checkFile.vobcFileTypeList.Sum(tar => Convert.ToInt32(tar)));//校验文件类型码(多文件类型求和=文件类型码)
            Common.vobcSystemType systemType = checkFile.vobcSystemType;
            switch (systemType)
            {
                case Common.vobcSystemType.ATP_1:
                    _atpUpdateFile = Convert.ToByte(checkFile.vobcFileTypeList.Sum(tar => Convert.ToInt32(tar)));
                    break;
                case Common.vobcSystemType.ATO_1:
                    _atoUpdateFile = Convert.ToByte(checkFile.vobcFileTypeList.Sum(tar => Convert.ToInt32(tar)));
                    break;
                case Common.vobcSystemType.COM_1:
                    _comUpdateFile = Convert.ToByte(checkFile.vobcFileTypeList.Sum(tar => Convert.ToInt32(tar)));
                    break;
                case Common.vobcSystemType.MMI:
                    _mmiUpdateFile = Convert.ToByte(checkFile.vobcFileTypeList.Sum(tar => Convert.ToInt32(tar)));
                    break;
                case Common.vobcSystemType.CCOV:
                    _ccovUpdateFile = Convert.ToByte(checkFile.vobcFileTypeList.Sum(tar => Convert.ToInt32(tar)));
                    break;
                default:
                    break;
            }
            //MD5有效标志位 默认都重置为无效（0xAA）
            pData[21] = 0xAA;//内核文件MD5有效标志
            pData[38] = 0xAA;//数据文件MD5有效标志
            pData[55] = 0xAA;//配置文件MD5有效标志
            pData[72] = 0xAA;//引导文件MD5有效标志
            pData[89] = 0xAA;//CCOV配置文件MD5有效标志
            pData[106] = 0xAA;//预留MD5有效标志

            //针对需要传输的文件，计算MD5值并将MD5写入数据帧
            for (int index = 0; index < checkFile.vobcFileTypeList.Count; index++)
            {
                if (checkFile.vobcFileTypeList[index] != Common.vobcFileType.INVALID)
                {

                    //计算MD5值
                    byte[] md5Value = GetMD5FromFile(checkFile.vobcFilePathList.ElementAt(index).Key);

                    //替换数组的开始索引下标
                    int replaceBeginIndex = -1;

                    //依据传输的文件类型 获取替换数组的索引下标
                    switch (checkFile.vobcFileTypeList[index])
                    {
                        case RemoteDeploy.Common.vobcFileType.CORE:
                            replaceBeginIndex = 5;
                            break;
                        case RemoteDeploy.Common.vobcFileType.DATA:
                            replaceBeginIndex = 22;
                            break;
                        case RemoteDeploy.Common.vobcFileType.NVRAM:
                            replaceBeginIndex = 39;
                            break;
                        case RemoteDeploy.Common.vobcFileType.BootLoader:
                            replaceBeginIndex = 56;
                            break;
                        case RemoteDeploy.Common.vobcFileType.CCOVConfig:
                            replaceBeginIndex = 73;
                            break;
                        default:
                            replaceBeginIndex = -1;
                            break;
                    }
                    string log = string.Empty;
                    log += "文件" + checkFile.vobcFileTypeList[index] + "MD5为：[";
                    foreach (byte item in md5Value)
                    {
                        log += Convert.ToString(item, 16) + " ";
                    }
                    log += "]";
                    LogManager.InfoLog.LogCommunicationInfo("DataPack",
                        "PackFileVerificationRequest", log);

                    //追加MD5值
                    ReplaceByteArray(ref pData, replaceBeginIndex, md5Value);

                    //追加MD5有效标志
                    pData[(replaceBeginIndex + md5Value.Length)] = 0x55;

                }

            }

            //跳至CRC起始位
            iter = 107;

            UInt32 crc32 = CRC.CRC32(pData, (ushort)(pData.Length - 4), 0);
            pData[iter++] = BitConverter.GetBytes(crc32)[3];
            pData[iter++] = BitConverter.GetBytes(crc32)[2];
            pData[iter++] = BitConverter.GetBytes(crc32)[1];
            pData[iter] = BitConverter.GetBytes(crc32)[0];

            return pData;
            //添加FFFE协议头尾
            //return FFFE.FFFEPack(pData);
            
        }

        #endregion

        #region 打包文件更新请求信息
        /// <summary>
        /// 打包文件更新请求信息
        /// </summary>
        /// <returns>帧字节数组</returns>
        public static byte[] PackFileUpdateRequest()
        {
            byte[] pData = new byte[16];
            byte iter = 0;
            pData[iter++] = 0x00;
            pData[iter++] = 0x0E;//帧长度
            pData[iter++] = 0x01;//帧类型码
            //填入各更新文件类型码
            pData[iter++] = _atpUpdateFile;
            pData[iter++] = _atpUpdateFile;
            pData[iter++] = _atpUpdateFile;
            pData[iter++] = _atoUpdateFile;
            pData[iter++] = _atoUpdateFile;
            pData[iter++] = _comUpdateFile;
            pData[iter++] = _comUpdateFile;
            pData[iter++] = _mmiUpdateFile;
            pData[iter++] = _ccovUpdateFile;

            //pData[iter++] = 0x01;//请求标志置为有效

            UInt32 crc32 = CRC.CRC32(pData, (ushort)(pData.Length - 4), 0);

            pData[iter++] = BitConverter.GetBytes(crc32)[3];
            pData[iter++] = BitConverter.GetBytes(crc32)[2];
            pData[iter++] = BitConverter.GetBytes(crc32)[1];
            pData[iter] = BitConverter.GetBytes(crc32)[0];

            return pData;
            //添加FFFE协议头尾
            //return FFFE.FFFEPack(pData);
            
        }

        #endregion

        #region 打包停止更新请求信息
        /// <summary>
        /// 打包停止更新请求信息
        /// </summary>
        /// <returns>帧字节数组</returns>
        public static byte[] PackStopUpdateRequest()
        {
            byte[] pData = new byte[8];
            byte iter = 0;
            pData[iter++] = 0x00;
            pData[iter++] = 0x06;//帧长度
            pData[iter++] = 0x0C;//帧类型码
            pData[iter++] = 0x55;//停止更新请求            

            UInt32 crc32 = CRC.CRC32(pData, (ushort)(pData.Length - 4), 0);

            pData[iter++] = BitConverter.GetBytes(crc32)[3];
            pData[iter++] = BitConverter.GetBytes(crc32)[2];
            pData[iter++] = BitConverter.GetBytes(crc32)[1];
            pData[iter] = BitConverter.GetBytes(crc32)[0];

            return pData;
            //添加FFFE协议头尾
            //return FFFE.FFFEPack(pData);
            
        }

        #endregion

        #region 打包远程重启请求信息
        /// <summary>
        /// 打包远程重启请求信息
        /// </summary>
        /// <returns>帧字节数组</returns>
        public static byte[] PackRemoteRebootRequest()
        {
            byte[] pData = new byte[8];
            byte iter = 0;
            pData[iter++] = 0x00;
            pData[iter++] = 0x06;//帧长度
            pData[iter++] = 0x0a;//帧类型码
            pData[iter++] = 0x55;//远程重启请求

            UInt32 crc32 = CRC.CRC32(pData, (ushort)(pData.Length - 4), 0);

            pData[iter++] = BitConverter.GetBytes(crc32)[3];
            pData[iter++] = BitConverter.GetBytes(crc32)[2];
            pData[iter++] = BitConverter.GetBytes(crc32)[1];
            pData[iter] = BitConverter.GetBytes(crc32)[0];

            return pData;
            //添加FFFE协议头尾
            //return FFFE.FFFEPack(pData);
            
        }

        #endregion

        #region 打包断开连接请求信息
        /// <summary>
        /// 打包断开连接请求信息
        /// </summary>
        /// <returns>帧字节数组</returns>
        public static byte[] PackDisconnectRequest()
        {
            byte[] pData = new byte[8];
            byte iter = 0;
            pData[iter++] = 0x00;
            pData[iter++] = 0x06;//帧长度
            pData[iter++] = 0x03;//帧类型码
            pData[iter++] = 0x55;//断开连接请求

            UInt32 crc32 = CRC.CRC32(pData, (ushort)(pData.Length - 4), 0);

            pData[iter++] = BitConverter.GetBytes(crc32)[3];
            pData[iter++] = BitConverter.GetBytes(crc32)[2];
            pData[iter++] = BitConverter.GetBytes(crc32)[1];
            pData[iter] = BitConverter.GetBytes(crc32)[0];

            return pData;
            //添加FFFE协议头尾
            //return FFFE.FFFEPack(pData);
            
        }

        #endregion
        
        #region 打包心跳请求信息
        /// <summary>
        /// 打包心跳请求信息
        /// </summary>
        /// <returns>帧字节数组</returns>
        public static byte[] PackHeartbeatRequest()
        {
            byte[] pData = new byte[7];
            byte iter = 0;
            pData[iter++] = 0x00;
            pData[iter++] = 0x05;//帧长度
            pData[iter++] = 0x08;//帧类型码

            UInt32 crc32 = CRC.CRC32(pData, (ushort)(pData.Length - 4), 0);

            pData[iter++] = BitConverter.GetBytes(crc32)[3];
            pData[iter++] = BitConverter.GetBytes(crc32)[2];
            pData[iter++] = BitConverter.GetBytes(crc32)[1];
            pData[iter] = BitConverter.GetBytes(crc32)[0];

            return pData;
            //添加FFFE协议头尾
            //return FFFE.FFFEPack(pData);
            
        }

        #endregion

        #region 打包复位请求信息
        /// <summary>
        /// 打包复位请求信息
        /// </summary>
        /// <returns>帧字节数组</returns>
        public static byte[] PackResetRequest()
        {
            byte[] pData = new byte[9];
            byte iter = 0;
            pData[iter++] = 0x00;
            pData[iter++] = 0x07;//帧长度
            pData[iter++] = 0x04;//帧类型码
            pData[iter++] = 0x55;//系统复位请求
            pData[iter++] = 0x55;//应用启动请求

            UInt32 crc32 = CRC.CRC32(pData, (ushort)(pData.Length - 4), 0);

            pData[iter++] = BitConverter.GetBytes(crc32)[3];
            pData[iter++] = BitConverter.GetBytes(crc32)[2];
            pData[iter++] = BitConverter.GetBytes(crc32)[1];
            pData[iter] = BitConverter.GetBytes(crc32)[0];

            return pData;
            //添加FFFE协议头尾
            //return FFFE.FFFEPack(pData);
            
        }

        #endregion

        #region 获取MD5值

        /// <summary>
        /// 计算文件MD5值
        /// </summary>
        /// <param name="fileName">文件绝对路径</param>
        /// <returns>计算好的MD5值</returns>
        public static byte[] GetMD5FromFile(string fileName)
        {
            try
            {
                //文件流 用于读取文件
                using (FileStream file = new FileStream(fileName, FileMode.Open))
                {

                    //MD5对象 用于计算MD5
                    System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();

                    //执行MD5码计算
                    byte[] retVal = md5.ComputeHash(file);

                    //StringBuilder sb = new StringBuilder();
                    //for (int i = 0; i < retVal.Length; i++)
                    //{
                    //    sb.Append(retVal[i].ToString("x2"));
                    //}

                    return retVal;
                }

            }
            catch (Exception ex)
            {
                throw new Exception("计算文件MD5时出现异常！" + ex.Message);
            }
        }

        #endregion

        #region 替换byte数组中的某些值

        /// <summary>
        /// 替换byte数组中的某些值
        /// </summary>
        /// <param name="rawArray">待替换原始数组</param>
        /// <param name="replaceIndex">替换的起始下标</param>
        /// <param name="replaceArray">需要替换的内容</param>
        private static void ReplaceByteArray(ref byte[] rawArray, int replaceIndex, byte[] replaceArray)
        {
            try
            {
                for (int i = replaceIndex; i < (replaceIndex + replaceArray.Length); i++)
                {
                    rawArray[i] = replaceArray[i - replaceIndex];
                }
            }
            catch (Exception ex)
            {
                throw new Exception("组包VOBC文件校验信息时出现异常！" + ex.Message);
            }
        }

        #endregion

    }
}
