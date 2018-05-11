using System;

namespace TCT.ShareLib.LogManager
{
    /// <summary>
    /// 告警日志内容类
    /// </summary>
    public class LogAlarmContent
    {
        private string m_content;
        private string m_level;
        private string m_mainType;
        private string m_subType;
        private DateTime m_time = DateTime.Now;

        /// <summary>
        /// 告警日志内容的构造方法
        /// </summary>
        /// <param name="level">告警等级</param>
        /// <param name="maintype">主类型</param>
        /// <param name="subtype">子类型</param>
        /// <param name="content">告警内容</param>
        public LogAlarmContent(string level, string maintype, string subtype, string content)
        {
            this.m_level = level;
            this.m_mainType = maintype;
            this.m_subType = subtype;
            this.m_content = content;
        }

        /// <summary>
        /// 重写ToString方法
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3} {4} {5}", new object[] { this.m_time.ToString("yyyy/MM/dd HH:mm:ss"), this.m_level, this.m_mainType, this.m_subType, this.m_content });
        }
    }
}

