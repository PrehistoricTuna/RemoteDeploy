﻿using System;

namespace TCT.ShareLib.LogManager
{


    public class LogConfigurationContent
    {
        private string m_className;
        private string m_content;
        private string m_fileName;
        private string m_item;
        private string m_methodName;
        private DateTime m_time = DateTime.Now;
        private string m_type;

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

        public override string ToString()
        {
            return string.Format("{0} [CONF] {1}[{2}::{3}][{4}][{5}]:{6}", new object[] { this.m_time.ToString("yyyy/MM/dd HH:mm:ss"), this.m_type, this.m_className, this.m_methodName, this.m_fileName, this.m_item, this.m_content });
        }
    }
}
