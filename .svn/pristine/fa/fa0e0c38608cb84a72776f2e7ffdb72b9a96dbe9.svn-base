
using System;

namespace TCT.ShareLib.LogManager
{
    /// <summary>
    /// 运行日志内容类
    /// </summary>
    public class LogProcContent
    {
        private string m_className;
        private string m_content;
        private string m_methodName;
        private string m_subType;
        private DateTime m_time = DateTime.Now;
        private string m_type;

        /// <summary>
        /// 运行日志内容类构造器
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="subtype">子类型</param>
        /// <param name="className">所在类名</param>
        /// <param name="methodName">所在方法名</param>
        /// <param name="content">日志内容</param>
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
        /// <summary>
        /// 重写ToString方法
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} [PROC] {1}[{2}::{3}]:{4}", new object[] { this.m_time.ToString("yyyy/MM/dd HH:mm:ss"), this.m_type, this.m_className, this.m_methodName, this.m_content });
        }
    }
}

