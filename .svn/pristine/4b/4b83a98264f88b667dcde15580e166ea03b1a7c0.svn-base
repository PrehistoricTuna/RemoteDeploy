//////////////////////////////////////////////////////////////////////
//////////////北京交控科技股份有限公司（TCT）///////////////////////////



using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
namespace TCT.ShareLib.LogManager
{
    /// <summary>
    /// 日志管理类
    /// </summary>
    public class LogManager
    {
        /// <summary>
        /// 日志管理器单例，供外部调用
        /// </summary>
        public static readonly LogManager InfoLog = new LogManager();
        /// <summary>
        /// 文件锁
        /// </summary>
        private object m_cThisLock = new object();
        private string m_directory;
        /// <summary>
        /// 日志文件名词典
        /// </summary>
        private Dictionary<string, string> m_fileNames = new Dictionary<string, string>();

        private LogManager()
        {
            //this.CreateLogFile("alarmchn", false);///中文告警
            //this.CreateLogFile("alarmeng", false);///英文告警
            this.CreateLogFile("comm", false);///通信日志
            //this.CreateLogFile("conf", false);///配置日志
            //this.CreateLogFile("proc", false);///运行日志
            //this.CreateLogFile("pf", false);///性能日志
        }
        /// <summary>
        /// 创建日志文件
        /// </summary>
        /// <param name="fileType">日志类型</param>
        /// <param name="withTime">是否伴随记录时间</param>
        private void CreateLogFile(string fileType, bool withTime)
        {
            this.m_directory = Application.StartupPath + @"\log";
            if (!Directory.Exists(this.m_directory))
            {
                Directory.CreateDirectory(this.m_directory);
            }
            StringBuilder builder = new StringBuilder(this.m_directory);
            builder.Append(@"\");
            builder.Append(fileType);
            builder.Append(DateTime.Today.ToString("yyyyMMdd"));
            if (withTime)
            {
                builder.Append("_");
                builder.Append((int) DateTime.Now.TimeOfDay.TotalSeconds);
            }
            builder.Append(".log");
            string path = builder.ToString();
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }
            this.m_fileNames.Add(fileType, path);
        }
        /// <summary>
        /// 记录中文告警日志
        /// </summary>
        /// <param name="content">告警内容对象</param>
        internal void LogAlarmChn(LogAlarmContent content)
        {
            //this.LogRecord("alarmchn", content);
        }
        /// <summary>
        /// 中文告警内容
        /// </summary>
        /// <param name="level">告警级别</param>
        /// <param name="maintype">告警类别</param>
        /// <param name="subtype">告警子类别</param>
        /// <param name="content">告警内容</param>
        public void LogAlarmChn(string level, string maintype, string subtype,string content)
        {
            //LogAlarmContent content2 = new LogAlarmContent(level, maintype, subtype, content);
            //this.LogAlarmChn(content2);
        }
        /// <summary>
        /// 英文告警内容
        /// </summary>
        /// <param name="content">告警内容对象</param>
        internal void LogAlarmEng(LogAlarmContent content)
        {
            //this.LogRecord("alarmeng", content);
        }
        /// <summary>
        /// 英文告警内容
        /// </summary>
        /// <param name="level">告警级别</param>
        /// <param name="maintype">告警类别</param>
        /// <param name="subtype">告警子类别</param>
        /// <param name="content">告警内容</param>
        public void LogAlarmEng(string level, string maintype, string subtype,string content)
        {
            //LogAlarmContent content2 = new LogAlarmContent(level, maintype, subtype, content);
            //this.LogAlarmEng(content2);
        }
        /// <summary>
        /// 通信日志
        /// </summary>
        /// <param name="content"></param>
        internal void LogCommunication(LogCommunicationContent content)
        {
            this.LogRecord("comm", content);
        }
        /// <summary>
        /// 通信日志
        /// </summary>
        /// <param name="type">日志类别：信息、错误、警告</param>
        /// <param name="className">记录所在类名</param>
        /// <param name="methodName">记录所在方法名</param>
        /// <param name="content">通信日志内容</param>
        public void LogCommunication(EmLogType type, string className, string methodName, string content)
        {
            LogCommunicationContent content2 = new LogCommunicationContent(type, className, methodName, "", "", content);
            this.LogRecord("comm", content2);
        }
        /// <summary>
        /// 通信日志
        /// </summary>
        /// <param name="type">日志类别：信息、错误、警告</param>
        /// <param name="className">记录所在类名</param>
        /// <param name="methodName">记录所在方法名</param>
        /// <param name="send">通信发送方</param>
        /// <param name="receive">通信接收方</param>
        /// <param name="content">日志内容</param>
        public void LogCommunication(EmLogType type, string className, string methodName, string send, string receive, string content)
        {
            LogCommunicationContent content2 = new LogCommunicationContent(type, className, methodName, send, receive, content);
            this.LogRecord("comm", content2);
        }
        /// <summary>
        /// 记录通信错误日志
        /// </summary>
        /// <param name="className">记录所在类名</param>
        /// <param name="methodName">记录所在方法名</param>
        /// <param name="content">日志内容</param>
        public void LogCommunicationError(string className, string methodName, string content)
        {
            this.LogCommunication(EmLogType.Error, className, methodName, content);
        }
        /// <summary>
        /// 记录通信信息日志
        /// </summary>
        /// <param name="className">记录所在类名</param>
        /// <param name="methodName">记录所在方法名</param>
        /// <param name="content">日志内容</param>
        public void LogCommunicationInfo(string className, string methodName, string content)
        {
            this.LogCommunication(EmLogType.Info, className, methodName, content);
        }
        /// <summary>
        /// 记录通信警告日志
        /// </summary>
        /// <param name="className">记录所在类名</param>
        /// <param name="methodName">记录所在方法名</param>
        /// <param name="content">日志内容</param>
        public void LogCommunicationWarning(string className, string methodName, string content)
        {
            this.LogCommunication(EmLogType.Warning, className, methodName, content);
        }
        /// <summary>
        /// 记录配置日志
        /// </summary>
        /// <param name="content"></param>
        internal void LogConfiguration(LogConfigurationContent content)
        {
            //this.LogRecord("conf", content);
        }
        /// <summary>
        /// 记录配置日志
        /// </summary>
        /// <param name="type">配置日志类型：信息、错误、警告</param>
        /// <param name="className">记录所在类名</param>
        /// <param name="methodName">记录所在方法名</param>
        /// <param name="fileName">配置文件名称</param>
        /// <param name="item">配置项名称</param>
        /// <param name="content">日志内容</param>
        public void LogConfiguration(EmLogType type, string className, string methodName, string fileName, string item, string content)
        {
            //LogConfigurationContent content2 = new LogConfigurationContent(type, className, methodName, fileName, item, content);
            //this.LogConfiguration(content2);
        }
        /// <summary>
        /// 记录配置错误日志
        /// </summary>
        /// <param name="fileName">配置文件名称</param>
        /// <param name="item">配置项名称</param>
        /// <param name="content">日志内容</param>
        public void LogConfigurationError(string fileName, string item, string content)
        {
            //this.LogConfiguration(EmLogType.Error, "", "", fileName, item, content);
        }
        /// <summary>
        /// 记录配置信息日志
        /// </summary>
        /// <param name="fileName">配置文件名称</param>
        /// <param name="item">配置项名称</param>
        /// <param name="content">日志内容</param>
        public void LogConfigurationInfo(string fileName, string item, string content)
        {
            //this.LogConfiguration(EmLogType.Info, "", "", fileName, item, content);
        }
        /// <summary>
        /// 记录配置警告日志
        /// </summary>
        /// <param name="fileName">配置文件名称</param>
        /// <param name="item">配置项名称</param>
        /// <param name="content">日志内容</param>
        public void LogConfigurationWarning(string fileName, string item, string content)
        {
            //this.LogConfiguration(EmLogType.Warning, "", "", fileName, item, content);
        }
        /// <summary>
        /// 记录运行错误日志
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void LogError(string format, params object[] args)
        {
            //this.LogProcError("", "", string.Format(format, args));
        }
        /// <summary>
        /// 记录运行信息日志
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void LogInfo(string format, params object[] args)
        {
            //this.LogProcInfo("", "", string.Format(format, args));
        }
        /// <summary>
        /// 记录性能日志
        /// </summary>
        /// <param name="content"></param>
        internal void LogPerformance(LogPerformanceContent content)
        {
            //this.LogRecord("pf", content);
        }
        /// <summary>
        /// 记录性能日志
        /// </summary>
        /// <param name="type">错误、警告、信息</param>
        /// <param name="taskName">任务名称</param>
        /// <param name="execTime">运行时间</param>
        /// <param name="content">日志内容</param>
        public void LogPerformance(EmLogType type, string taskName, double execTime, string content)
        {
            //LogPerformanceContent content2 = new LogPerformanceContent(type, taskName, execTime, content);
            //this.LogPerformance(content2);
        }
        /// <summary>
        /// 性能错误日志
        /// </summary>
        /// <param name="taskName">任务名称</param>
        /// <param name="execTime">运行时间</param>
        /// <param name="content">日志内容</param>
        public void LogPerformanceError(string taskName, double execTime, string content)
        {
            //this.LogPerformance(EmLogType.Error, taskName, execTime, content);
        }
        /// <summary>
        /// 性能信息日志
        /// </summary>
        /// <param name="taskName">任务名称</param>
        /// <param name="execTime">运行时间</param>
        /// <param name="content">日志内容</param>
        public void LogPerformanceInfo(string taskName, double execTime, string content)
        {
            //this.LogPerformance(EmLogType.Info, taskName, execTime, content);
        }
        /// <summary>
        /// 性能告警日志
        /// </summary>
        /// <param name="taskName">任务名称</param>
        /// <param name="execTime">运行时间</param>
        /// <param name="content">日志内容</param>
        public void LogPerformanceWarning(string taskName, double execTime, string content)
        {
            //this.LogPerformance(EmLogType.Warning, taskName, execTime, content);
        }
        /// <summary>
        /// 记录运行日志
        /// </summary>
        /// <param name="content"></param>
        internal void LogProc(LogProcContent content)
        {
            //this.LogRecord("proc", content);
        }
        /// <summary>
        /// 记录运行日志
        /// </summary>
        /// <param name="type">告警、错误、信息</param>
        /// <param name="subtype">自定义子类型</param>
        /// <param name="className">所在类名</param>
        /// <param name="methodName">所在方法名</param>
        /// <param name="content">日志内容</param>
        public void LogProc(EmLogType type, string subtype, string className, string methodName, string content)
        {
            //LogProcContent content2 = new LogProcContent(type, subtype, className, methodName, content);
            //this.LogProc(content2);
        }
        /// <summary>
        /// 记录运行错误日志
        /// </summary>
        /// <param name="className">所在类名</param>
        /// <param name="methodName">所在方法名</param>
        /// <param name="content">日志内容</param>
        public void LogProcError(string className, string methodName, string content)
        {
            //this.LogProc(EmLogType.Error, "", className, methodName, content);
        }
        /// <summary>
        /// 记录运行信息日志
        /// </summary>
        /// <param name="className">所在类名</param>
        /// <param name="methodName">所在方法名</param>
        /// <param name="content">日志内容</param>
        public void LogProcInfo(string className, string methodName, string content)
        {
            //this.LogProc(EmLogType.Info, "", className, methodName, content);
        }
        /// <summary>
        /// 记录运行告警日志
        /// </summary>
        /// <param name="className">所在类名</param>
        /// <param name="methodName">所在方法名</param>
        /// <param name="content">日志内容</param>
        public void LogProcWarning(string className, string methodName, string content)
        {
            //this.LogProc(EmLogType.Warning, "", className, methodName, content);
        }
        /// <summary>
        /// 日志记录方法
        /// </summary>
        /// <param name="type"></param>
        /// <param name="content"></param>
        private void LogRecord(string type, object content)
        {
            lock (this.m_cThisLock)
            {
                string fileName = this.m_fileNames[type];
                FileInfo info = new FileInfo(fileName);
                if (info.Length > 0x9fe980L)
                {
                    this.m_fileNames.Remove(type);
                    this.CreateLogFile(type, true);
                }
                using (StreamWriter writer = new StreamWriter(fileName, true))
                {
                    writer.WriteLine(content.ToString());
                    writer.Close();
                }
            }
        }
    }
}

