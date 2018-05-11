
using System;



namespace TCT.ShareLib.LogManager
{

    /// <summary>
    /// 程序性能日志的内容类
    /// </summary>
    public class LogPerformanceContent
    {
        private string m_content;
        private double m_execTime;
        private string m_taskName;
        private DateTime m_time = DateTime.Now;
        private string m_type;

        /// <summary>
        /// 程序运行性能的内容类构造方法
        /// </summary>
        /// <param name="type">告警、错误、信息</param>
        /// <param name="taskName">任务名</param>
        /// <param name="execTime">执行时间</param>
        /// <param name="content">内容</param>
        public LogPerformanceContent(EmLogType type, string taskName, double execTime, string content)
        {
            switch (type)
            {
                case EmLogType.Info:
                    this.m_type = "[INFO]";
                    break;

                case EmLogType.Error:
                    this.m_type = "[ERROR]";
                    break;

                case EmLogType.Warning:
                    this.m_type = "[WARNING]";
                    break;

                default:
                    this.m_type = "";
                    break;
            }
            this.m_taskName = taskName;
            this.m_execTime = execTime;
            this.m_content = content;
        }

        /// <summary>
        /// 重写ToString方法
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} [PF] {1}[{2}][{3}]:{4}", new object[] { this.m_time.ToString("yyyy/MM/dd HH:mm:ss"), this.m_type, this.m_taskName, this.m_execTime, this.m_content });
        }
    }
}

