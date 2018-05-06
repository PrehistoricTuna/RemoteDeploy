using System;


namespace TCT.ShareLib.LogManager
{


    public class LogCommunicationContent
    {
        private string m_className;
        private string m_content;
        private string m_methodName;
        private string m_receive;
        private string m_send;
        private DateTime m_time = DateTime.Now;
        private string m_type;

        public LogCommunicationContent(EmLogType type, string className, string methodName, string send, string receive, string content)
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
            this.m_className = className;
            this.m_methodName = methodName;
            this.m_send = send;
            this.m_receive = receive;
            this.m_content = content;
        }

        public override string ToString()
        {
            return string.Format("{0} [COMM] {1}[{2}::{3}][{4}->{5}]:{6}", new object[] { this.m_time.ToString("yyyy/MM/dd HH:mm:ss"), this.m_type, this.m_className, this.m_methodName, this.m_send, this.m_receive, this.m_content });
        }
    }
}

