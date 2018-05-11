using System;

namespace TCT.ShareLib.LogManager
{

    /// <summary>
    /// 配置日志内容类
    /// </summary>
    public class LogConfigurationContent
    {
        private string m_className;
        private string m_content;
        private string m_fileName;
        private string m_item;
        private string m_methodName;
        private DateTime m_time = DateTime.Now;
        private string m_type;

        /// <summary>
        /// 配置日志内容构造器
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="className">类名</param>
        /// <param name="methodName">方法名</param>
        /// <param name="fileName">文件名</param>
        /// <param name="item">对象</param>
        /// <param name="content">内容</param>
        public LogConfigurationContent(EmLogType type, string className, string methodName, string fileName, string item, string content)
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
            this.m_fileName = fileName;
            this.m_item = item;
            this.m_content = content;
        }
        /// <summary>
        /// 重写ToString方法
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} [CONF] {1}[{2}::{3}][{4}][{5}]:{6}", new object[] { this.m_time.ToString("yyyy/MM/dd HH:mm:ss"), this.m_type, this.m_className, this.m_methodName, this.m_fileName, this.m_item, this.m_content });
        }
    }
}

