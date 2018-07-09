using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using TCT.ShareLib.LogManager;

namespace RemoteDeploy.EquData
{
    public class HLHTLineConsole : IProjectConsole
    {
        /// <summary>
        /// 互联互通产品
        /// </summary>
        public HLHTLineConsole()
        {
            ProductLine = EmProductLine.HLHT;
            LogManager.InfoLog.LogProcInfo("HLHTLineConsole", "HLHTLineConsole", "创建互联互通线路数据集合实体");
        }
    }
}
