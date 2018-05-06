using System;

namespace TCT.ShareLib.LogManager
{

    public class LogAlarmContent
    {
        private string m_content;
        private string m_level;
        private string m_mainType;
        private string m_subType;
        private DateTime m_time = DateTime.Now;

        public LogAlarmContent(string level, string maintype, string subtype, string content)
        {
            this.m_level = level;
            this.m_mainType = maintype;
            this.m_subType = subtype;
            this.m_content = content;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3} {4} {5}", new object[] { this.m_time.ToString("yyyy/MM/dd HH:mm:ss"), this.m_level, this.m_mainType, this.m_subType, this.m_content });
        }
    }
}

