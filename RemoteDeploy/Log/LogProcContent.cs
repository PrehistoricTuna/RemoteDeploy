
using System;

namespace TCT.ShareLib.LogManager
{
    public class LogProcContent
    {
        private string m_className;
        private string m_content;
        private string m_methodName;
        private string m_subType;
        private DateTime m_time = DateTime.Now;
        private string m_type;

        public LogProcContent(EmLogType type, string subtype, string className, string methodName, string content)
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
            this.m_subType = subtype;
            this.m_className = className;
            this.m_methodName = methodName;
            this.m_content = content;
        }

        public override string ToString()
        {
            return string.Format("{0} [PROC] {1}[{2}::{3}]:{4}", new object[] { this.m_time.ToString("yyyy/MM/dd HH:mm:ss"), this.m_type, this.m_className, this.m_methodName, this.m_content });
        }
    }
}

