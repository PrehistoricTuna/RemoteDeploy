
using System;



namespace TCT.ShareLib.LogManager
{


    public class LogPerformanceContent
    {
        private string m_content;
        private double m_execTime;
        private string m_taskName;
        private DateTime m_time = DateTime.Now;
        private string m_type;

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

        public override string ToString()
        {
            return string.Format("{0} [PF] {1}[{2}][{3}]:{4}", new object[] { this.m_time.ToString("yyyy/MM/dd HH:mm:ss"), this.m_type, this.m_taskName, this.m_execTime, this.m_content });
        }
    }
}

