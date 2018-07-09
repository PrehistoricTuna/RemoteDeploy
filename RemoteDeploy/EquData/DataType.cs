using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteDeploy.EquData
{
    /// <summary>
    /// 产品线枚举
    /// </summary>
    public enum EmProductLine
    {
        NEWAIR,
        CPK,
        FAO,
        HLHT,
        NONE
    }

    /// <summary>
    /// 容器类型枚举
    /// </summary>
    public enum EmContainerType
    {
        VOBC,
        ZC,
        NONE
    }

    /// <summary>
    /// 部署配置状态类
    /// </summary>
    public class DeployConfiState
    {
        public bool IsBootLoaderCheck = false;
        public bool IsCoreCheck = false;
        public bool IsNvRamCheck = false;
        public bool IsDataCheck = false;
        public bool IsIniCheck = false;
    }

}
