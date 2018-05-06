using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteDeploy.EquData
{
    public enum EmProductLine
    {
        NEWAIR,
        CPK,
        FAO,
        HLHT,
        NONE
    }

    public enum EmContainerType
    {
        VOBC,
        ZC,
        NONE
    }

    public class DeployConfiState
    {
        public bool IsBootLoaderCheck = false;
        public bool IsCoreCheck = false;
        public bool IsNvRamCheck = false;
        public bool IsDataCheck = false;
        public bool IsIniCheck = false;
    }

}
