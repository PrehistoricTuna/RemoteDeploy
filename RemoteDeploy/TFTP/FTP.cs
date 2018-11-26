using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using TCT.ShareLib.LogManager;

namespace RemoteDeploy.TFTP
{
    public static class FTPHelper
    {

        #region 变量属性

        /// <summary>
        /// Ftp 指定用户名
        /// </summary>
        public static string FtpUserID = "1";

        /// <summary>
        /// Ftp 指定用户密码
        /// </summary>
        public static string FtpPassword = "1";

        /// <summary>
        /// 文件锁
        /// </summary>
        //private static object m_cThisLock = new object();

        
        //记录文件路径和文件流信息的keyvalue键值表 用作防止文件被互斥访问  key=文件本地路径  value=文件流
        private static List<KeyValuePair<string, FileInfo>> keyValueOfFileStreamList = new List<KeyValuePair<string, FileInfo>>();

        #endregion

        #region 上传文件到FTP服务器

        /// <summary>
        /// 上传文件到FTP服务器(断点续传)
        /// </summary>
        /// <param name="localFullPath">本地文件全路径名称：C:\Users\JianKunKing\Desktop\IronPython脚本测试工具</param>
        /// <param name="remoteFilepath">远程文件所在文件夹路径</param>
        /// <returns></returns>       
        public static bool FtpUploadBroken(string ftpServerIP, string localFullPath, string remoteFilepath)
        {


            remoteFilepath = (null == remoteFilepath) ? "" : remoteFilepath;

            //判定传送文件的目标目录是否存在
            //if (!DirectoryExist(remoteFilepath))
            //{
            //    MakeDir(remoteFilepath);
            //}
            //变量定义

            //用作存储格式化后的文件名称变量
            string newFileName = string.Empty;

            //更新结果
            bool success = true;

            //用作读取数据文件流的变量
            FileInfo fileInf = null;

            try
            {
                //在存储列表中查询  如果查询的到则直接使用 如果查询不到则读取并存入列表
                if (null == keyValueOfFileStreamList.Find(tar => tar.Key == localFullPath).Key)
                {
                    fileInf = new FileInfo(localFullPath);
                    KeyValuePair<string, FileInfo> tmpFValue = new KeyValuePair<string, FileInfo>(localFullPath, fileInf) { };
                    keyValueOfFileStreamList.Add(tmpFValue);
                }
                else
                {
                    fileInf = keyValueOfFileStreamList.Find(tar => tar.Key == localFullPath).Value;
                }

                long allbye = (long)fileInf.Length;
                if (fileInf.Name.IndexOf("#") == -1)
                {
                    newFileName = RemoveSpaces(fileInf.Name);
                }
                else
                {
                    newFileName = fileInf.Name.Replace("#", "＃");
                    newFileName = RemoveSpaces(newFileName);
                }
                long startfilesize = GetFileSize(newFileName, remoteFilepath, ftpServerIP);
                if (startfilesize >= allbye)
                {
                    return false;
                }
                long startbye = startfilesize;
                //更新进度  
                //if (updateProgress != null)
                //{
                //    updateProgress((int)allbye, (int)startfilesize);//更新进度条   
                //}

                string uri;
                if (remoteFilepath.Length == 0)
                {
                    uri = "ftp://" + ftpServerIP + "/" + newFileName;
                }
                else
                {
                    uri = "ftp://" + ftpServerIP + "/" + remoteFilepath + "/" + newFileName;
                }
                FtpWebRequest reqFTP = null;
                // 根据uri创建FtpWebRequest对象

                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));


                reqFTP.Proxy = null;
                // ftp用户名和密码 
                reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
                // 默认为true，连接不会被关闭 
                // 在一个命令之后被执行 
                reqFTP.KeepAlive = true;
                // 指定执行什么命令 
                reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
                // 指定为FTP主动式工作 Modified @ 10.17
                //reqFTP.UsePassive = true;
                // 指定数据传输类型 
                reqFTP.UseBinary = true;
                // 上传文件时通知服务器文件的大小 
                reqFTP.ContentLength = fileInf.Length;
                int buffLength = 1024;// 缓冲大小设置为1kb 
                byte[] buff = new byte[buffLength];
                // 打开一个文件流 (System.IO.FileStream) 去读上传的文件
                FileStream fs = null;
                Stream strm = null;
                try
                {
                    //Modified @ 9.13
                    //fs = fileInf.OpenRead();
                    fs = new System.IO.FileStream(localFullPath, FileMode.Open, FileAccess.Read, FileShare.Read);

                    // 把上传的文件写入流 
                    strm = reqFTP.GetRequestStream();
                    // 每次读文件流的2kb   
                    fs.Seek(startfilesize, 0);
                    int contentLen = fs.Read(buff, 0, buffLength);
                    // 流内容没有结束 
                    while (contentLen != 0)
                    {
                        // 把内容从file stream 写入 upload stream 
                        strm.Write(buff, 0, contentLen);
                        contentLen = fs.Read(buff, 0, buffLength);
                        startbye += contentLen;
                        //更新进度  
                        //if (updateProgress != null)
                        //{
                        //    updateProgress((int)allbye, (int)startbye);//更新进度条   
                        //}
                    }
                    // 关闭两个流 
                    strm.Close();
                    fs.Close();
                    fs.Dispose();
                    LogManager.InfoLog.LogCommunicationInfo("FTP", "FtpUploadBroken", localFullPath + "发送完成！");
                }
                catch (Exception ex)
                {
                    LogManager.InfoLog.LogCommunicationInfo("FTP", "FtpUploadBroken", ex.Message);
                    success = false;
                    Thread.Sleep(1000);
                }
                finally
                {
                    if (fs != null)
                    {
                        fs.Close();
                    }
                    if (strm != null)
                    {
                        strm.Close();
                    }
                }
            }
            catch(Exception ex)
            {
                success = false;
                LogManager.InfoLog.LogCommunication(EmLogType.Error, "FTP.cs", "FtpUploadBroken", "使用FTP发送文件时发生错误\r\n"+ex.Message);
            }

            return success;
        }

        /// <summary>
        /// 上传文件到FTP服务器(断点续传)
        /// </summary>
        /// <param name="localFullPath">本地文件全路径名称：C:\Users\JianKunKing\Desktop\IronPython脚本测试工具</param>
        /// <param name="remoteFilepath">远程文件所在文件夹路径</param>
        /// <returns></returns>       
        public static bool FtpUploadBrokenOLD(string ftpServerIP, string localFullPath, string remoteFilepath)
        {

            if (remoteFilepath == null)
            {
                remoteFilepath = "";
            }

            //判定传送文件的目标目录是否存在
            //if (!DirectoryExist(remoteFilepath))
            //{
            //    MakeDir(remoteFilepath);
            //}
            string newFileName = string.Empty;
            bool success = true;
            FileInfo fileInf = new FileInfo(localFullPath);
            long allbye = (long)fileInf.Length;
            if (fileInf.Name.IndexOf("#") == -1)
            {
                newFileName = RemoveSpaces(fileInf.Name);
            }
            else
            {
                newFileName = fileInf.Name.Replace("#", "＃");
                newFileName = RemoveSpaces(newFileName);
            }
            long startfilesize = GetFileSize(newFileName, remoteFilepath, ftpServerIP);
            if (startfilesize >= allbye)
            {
                return false;
            }
            long startbye = startfilesize;
            //更新进度  
            //if (updateProgress != null)
            //{
            //    updateProgress((int)allbye, (int)startfilesize);//更新进度条   
            //}

            string uri;
            if (remoteFilepath.Length == 0)
            {
                uri = "ftp://" + ftpServerIP + "/" + newFileName;
            }
            else
            {
                uri = "ftp://" + ftpServerIP + "/" + remoteFilepath + "/" + newFileName;
            }
            FtpWebRequest reqFTP = null;
            // 根据uri创建FtpWebRequest对象
            try
            {
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));

            }
            catch
            {
                success = false;
            }
            reqFTP.Proxy = null;
            // ftp用户名和密码 
            reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
            // 默认为true，连接不会被关闭 
            // 在一个命令之后被执行 
            reqFTP.KeepAlive = true;
            // 指定执行什么命令 
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
            // 指定数据传输类型 
            reqFTP.UseBinary = true;
            // 上传文件时通知服务器文件的大小 
            reqFTP.ContentLength = fileInf.Length;
            int buffLength = 2048;// 缓冲大小设置为2kb 
            byte[] buff = new byte[buffLength];
            // 打开一个文件流 (System.IO.FileStream) 去读上传的文件 
            bool b = true;
            FileStream fs = null;
            Stream strm = null;
            //while (b)
            //{
            try
            {
                fs = fileInf.OpenRead();

                // 把上传的文件写入流 
                strm = reqFTP.GetRequestStream();
                // 每次读文件流的2kb   
                fs.Seek(startfilesize, 0);
                int contentLen = fs.Read(buff, 0, buffLength);
                // 流内容没有结束 
                while (contentLen != 0)
                {
                    // 把内容从file stream 写入 upload stream 
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                    startbye += contentLen;
                    //更新进度  
                    //if (updateProgress != null)
                    //{
                    //    updateProgress((int)allbye, (int)startbye);//更新进度条   
                    //}
                }
                // 关闭两个流 
                strm.Close();
                fs.Close();
                fs.Dispose();
                b = false;
            }
            catch (Exception ex)
            {
                LogManager.InfoLog.LogCommunicationInfo("FTP", "FtpUploadBroken", ex.Message);
                success = false;
                Thread.Sleep(1000);
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
                if (strm != null)
                {
                    strm.Close();
                }
            }

            //}


            return success;
        }


        /// <summary>
        /// 去除空格
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string RemoveSpaces(string str)
        {
            return str;

            string a = "";
            CharEnumerator CEnumerator = str.GetEnumerator();
            while (CEnumerator.MoveNext())
            {
                byte[] array = new byte[1];
                array = System.Text.Encoding.ASCII.GetBytes(CEnumerator.Current.ToString());
                int asciicode = (short)(array[0]);
                if (asciicode != 32)
                {
                    a += CEnumerator.Current.ToString();
                }
            }
            string sdate = System.DateTime.Now.Year.ToString() + System.DateTime.Now.Month.ToString() + System.DateTime.Now.Day.ToString() + System.DateTime.Now.Hour.ToString()
                + System.DateTime.Now.Minute.ToString() + System.DateTime.Now.Second.ToString() + System.DateTime.Now.Millisecond.ToString();
            return a.Split('.')[a.Split('.').Length - 2] + "." + a.Split('.')[a.Split('.').Length - 1];
        }
        /// <summary>
        /// 获取已上传文件大小
        /// </summary>
        /// <param name="filename">文件名称</param>
        /// <param name="path">服务器文件路径</param>
        /// <param name="ftpServerIP">服务器IP地址</param>
        /// <returns></returns>
        public static long GetFileSize(string filename, string remoteFilepath, string ftpServerIP)
        {
            long filesize = 0;
            try
            {
                FtpWebRequest reqFTP;
                FileInfo fi = new FileInfo(filename);
                string uri;
                if (remoteFilepath.Length == 0)
                {
                    uri = "ftp://" + ftpServerIP + "/" + fi.Name;
                }
                else
                {
                    uri = "ftp://" + ftpServerIP + "/" + remoteFilepath + "/" + fi.Name;
                }
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(uri);
                reqFTP.KeepAlive = false;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);//用户，密码
                reqFTP.Method = WebRequestMethods.Ftp.GetFileSize;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                filesize = response.ContentLength;
                return filesize;
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region 获取当前目录下明细

        /// <summary>
        /// 获取当前目录下明细(包含文件和文件夹)
        /// </summary>
        /// <param name="ftpServerIP">FTP服务端IP地址</param>
        /// <returns></returns>
        public static string[] GetFilesDetailList(string ftpServerIP)
        {

            StringBuilder result = new StringBuilder();

            try
            {

                string ftpURI = "ftp://" + ftpServerIP + "/";

                FtpWebRequest ftp = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpURI));
                ftp.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
                ftp.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                WebResponse response = ftp.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default);
                string line = reader.ReadLine();

                while (line != null)
                {
                    result.Append(line);
                    result.Append("\n");
                    line = reader.ReadLine();
                }
                result.Remove(result.ToString().LastIndexOf("\n"), 1);
                reader.Close();
                response.Close();

            }
            catch (Exception ex)
            {
                LogManager.InfoLog.LogCommunicationError("MainWindow", "ProductReport", "FTP获取远程地址为" + ftpServerIP + "的文件目录时失败：" + ex.Message);
            }

            return result.ToString().Split('\n');

        }
        /// <summary>
        /// 获取当前目录下文件列表(仅文件)
        /// </summary>
        /// <param name="mask">搜索的文件类型</param>
        /// <param name="ftpServerIP">服务器IP地址信息</param>
        /// <returns></returns>
        public static string[] GetFileList(string path, string mask, string ftpServerIP)
        {

            StringBuilder result = new StringBuilder();
            FtpWebRequest reqFTP;
            try
            {
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/" + path));
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
                reqFTP.Method = WebRequestMethods.Ftp.ListDirectory;
                WebResponse response = reqFTP.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default);

                string line = reader.ReadLine().Trim();
                while (line != null)
                {
                    if (mask.Trim() != string.Empty && mask.Trim() != "*.*")
                    {

                        string mask_ = mask.Substring(0, mask.IndexOf("*"));
                        if (line.Substring(0, mask_.Length) == mask_)
                        {
                            result.Append(line);
                            result.Append("\n");
                        }
                    }
                    else
                    {
                        result.Append(line);
                        result.Append("\n");
                    }
                    line = reader.ReadLine();
                }
                result.Remove(result.ToString().LastIndexOf('\n'), 1);
                reader.Close();
                response.Close();

            }
            catch (Exception ex)
            {
                LogManager.InfoLog.LogCommunicationError("MainWindow", "ProductReport", "FTP获取远程地址为" + ftpServerIP + "的文件目录时失败：" + ex.Message);
            }

            return result.ToString().Split('\n');
        }
        /// <summary>
        /// 获取当前目录下文件列表(仅文件)
        /// </summary>
        /// <param name="mask">搜索的文件类型</param>
        /// <param name="ftpServerIP">服务器IP地址信息</param>
        /// <returns></returns>
        public static string[] GetFileList(string mask, string ftpServerIP)
        {

            StringBuilder result = new StringBuilder();
            FtpWebRequest reqFTP;
            try
            {
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/"));
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
                reqFTP.Method = WebRequestMethods.Ftp.ListDirectory;
                WebResponse response = reqFTP.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default);

                string line = reader.ReadLine();
                while (line != null)
                {
                    if (mask.Trim() != string.Empty && mask.Trim() != "*.*")
                    {

                        string mask_ = mask.Substring(0, mask.IndexOf("*"));
                        if (line.Substring(0, mask_.Length) == mask_)
                        {
                            result.Append(line);
                            result.Append("\n");
                        }
                    }
                    else
                    {
                        result.Append(line);
                        result.Append("\n");
                    }
                    line = reader.ReadLine();
                }
                result.Remove(result.ToString().LastIndexOf('\n'), 1);
                reader.Close();
                response.Close();
                return result.ToString().Split('\n');
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// 获取当前目录下所有的文件夹列表(仅文件夹)
        /// </summary>
        /// <param name="ftpServerIP">服务器IP地址信息</param>
        /// <returns></returns>
        public static string[] GetDirectoryList(string ftpServerIP)
        {
            string[] drectory = GetFilesDetailList(ftpServerIP);
            string m = string.Empty;
            foreach (string str in drectory)
            {
                int dirPos = str.IndexOf("<DIR>");
                if (dirPos > 0)
                {
                    /*判断 Windows 风格*/
                    m += str.Substring(dirPos + 5).Trim() + "\n";
                }
                else if (str.Trim().Substring(0, 1).ToUpper() == "D")
                {
                    /*判断 Unix 风格*/
                    string dir = str.Substring(54).Trim();
                    if (dir != "." && dir != "..")
                    {
                        m += dir + "\n";
                    }
                }
            }

            char[] n = new char[] { '\n' };
            return m.Split(n);
        }
        #endregion

        #region 删除文件及文件夹

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="ftpServerIP">FTP服务端地址</param> 
        /// <param name="fileName"></param>
        public static bool Delete(string fileName, string ftpServerIP)
        {
            try
            {
                string uri = "ftp://" + ftpServerIP + "/" + fileName;
                FtpWebRequest reqFTP;
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));

                reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
                reqFTP.KeepAlive = false;
                reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;

                string result = String.Empty;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                long size = response.ContentLength;
                Stream datastream = response.GetResponseStream();
                StreamReader sr = new StreamReader(datastream);
                result = sr.ReadToEnd();
                sr.Close();
                datastream.Close();
                response.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw ex;
            }
        }

        /// <summary>
        /// 删除文件夹
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="ftpServerIP">FTP服务端地址</param> 
        public static void RemoveDirectory(string folderName, string ftpServerIP)
        {
            try
            {
                string uri = "ftp://" + ftpServerIP + "/" + folderName;
                FtpWebRequest reqFTP;
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));

                reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
                reqFTP.KeepAlive = false;
                reqFTP.Method = WebRequestMethods.Ftp.RemoveDirectory;

                string result = String.Empty;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                long size = response.ContentLength;
                Stream datastream = response.GetResponseStream();
                StreamReader sr = new StreamReader(datastream);
                result = sr.ReadToEnd();
                sr.Close();
                datastream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 其他操作

        /// <summary>
        /// 获取指定文件大小
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="ftpServerIP">FTP服务端地址</param> 
        /// <returns></returns>
        public static long GetFileSize(string filename, string ftpServerIP)
        {
            //文件长度
            long fileSize = 0;

            try
            {
                //创建FTPweb访问类
                FtpWebRequest reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/" + filename));

                //属性赋值
                reqFTP.Method = WebRequestMethods.Ftp.GetFileSize;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);

                //封装传输协议
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();

                //获取文件流
                Stream ftpStream = response.GetResponseStream();
                fileSize = response.ContentLength;

                //文件流即协议类对象释放
                ftpStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                LogManager.InfoLog.LogCommunicationError("MainWindow", "ProductReport", "FTP发送文件[" + filename + "]时出现异常！" + ex.Message);
            }
            return fileSize;
        }

        /// <summary>
        /// 判断当前目录下指定的子目录是否存在
        /// <param name="ftpServerIP">FTP服务端地址</param> 
        /// </summary>
        /// <param name="RemoteDirectoryName">指定的目录名</param>
        public static bool DirectoryExist(string RemoteDirectoryName, string ftpServerIP)
        {
            try
            {
                string[] dirList = GetDirectoryList(ftpServerIP);

                foreach (string str in dirList)
                {
                    if (str.Trim() == RemoteDirectoryName.Trim())
                    {
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }

        }

        /// <summary>
        /// 判断当前目录下指定的文件是否存在
        /// </summary>
        /// <param name="RemoteFileName">远程文件名</param>
        public static bool FileExist(string RemoteFileName, string ftpServerIP)
        {
            string[] fileList = GetFileList("*.*", ftpServerIP);
            foreach (string str in fileList)
            {
                if (str.Trim() == RemoteFileName.Trim())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="dirName">待创建的文件路径名称</param>
        /// <param name="ftpServerIP">ftp服务端IP地址</param>
        public static void MakeDir(string dirName, string ftpServerIP)
        {
            FtpWebRequest reqFTP;
            try
            {
                // dirName = name of the directory to create.
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/" + dirName));
                reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();

                ftpStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                LogManager.InfoLog.LogCommunicationError("MainWindow", "ProductReport", "FTP创建远程目录[" + dirName + "]时出现错误：" + ex.Message);
            }
        }

        /// <summary>
        /// 改名
        /// </summary>
        /// <param name="currentFilename"></param>
        /// <param name="newFilename"></param>
        /// <param name="ftpServerIP">ftp服务端IP地址</param>
        public static void ReName(string currentFilename, string newFilename, string ftpServerIP)
        {
            FtpWebRequest reqFTP;
            try
            {
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + ftpServerIP + "/" + currentFilename));
                reqFTP.Method = WebRequestMethods.Ftp.Rename;
                reqFTP.RenameTo = newFilename;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();

                ftpStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                LogManager.InfoLog.LogCommunicationError("MainWindow", "ProductReport", "FTP修改远程文件名称时发生错误：" + ex.Message);
            }
        }

        /// <summary>
        /// 移动文件
        /// </summary>
        /// <param name="currentFilename"></param>
        /// <param name="ftpServerIP">ftp服务端IP地址</param>
        public static void MovieFile(string currentFilename, string newDirectory, string ftpServerIP)
        {
            ReName(currentFilename, newDirectory, ftpServerIP);
        }

        #endregion

    }
}
