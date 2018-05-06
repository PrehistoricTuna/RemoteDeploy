using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteDeploy
{
    /// <summary>
    /// 自定义异常类3.
    /// </summary>
    public class MyException : ApplicationException
    {
        public MyException(string message) : base(message) { }
        public override string Message
        {
            get
            {
                return base.Message;
            }
        }
    }
}
