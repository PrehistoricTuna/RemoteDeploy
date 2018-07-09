using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteDeploy.Models.VOBC
{
    //VOBC信息类
    public class VOBCStateInfoClass
    {
        #region 属性

        #region ATP参数赋值

        //ATP应用版本号
        public string AtpSoftVersion { get; set; }

        //ATP数据版本号
        public string AtpDataVersion { get; set; }

        //ATP板卡状态
        public string AtpStatus{ get; set; }

        #endregion

        #region ATO参数赋值

        //ATO应用版本号
        public string AtoSoftVersion { get; set; }

        //ATO数据版本号
        public string AtoDataVersion { get; set; }

        //ATO板卡状态
        public string AtoStatus { get; set; }

        #endregion

        #region CCOV参数赋值

        //CCOV应用版本号
        public string CCOVSoftVersion { get; set; }

        //CCOV数据版本号
        public string CCOVDataVersion { get; set; }

        //CCOV板卡状态
        public string CCOVStatus { get; set; }

        //TFTP服务开启状态
        public string AtpTftpStatus{ get; set; }

        #endregion

        #region MMI参数赋值

        //MMI应用版本号
        public string MmiSoftVersion{ get; set; }

        //MMI工作状态
        public string MmiStatus{ get; set; }

        #endregion

        #region COM参数赋值

        //COM应用版本号
        public string Com1SoftVersion{ get; set; }

        //COM应用版本号
        public string Com2SoftVersion { get; set; }

        //COM工作状态
        public string ComStatus{ get; set; }

        #endregion

        #region 其他列车参数赋值

        //列车运行模式
        //public string OperationMode{ get; set; }

        //列车是否静止 
        public bool IsSteady { get; set; }

        //列车IP地址        
        //public string TrainIP{ get; set; }

        //列车无线网络状态
        //public string WirelessStatus{ get; set; }

        //列车无线关联信噪比
        public int WirelessSNR{ get; set; }

        //TC1端？
        //public string Tc1Status{ get; set; }

        //TC2端？
        //public string Tc2Status { get; set; }

        //列车位置
        public string TrainPosition{ get; set; }

        //最终预检结果
        public bool PreResult { get; set; }

        #endregion

        #endregion
    }
        
}
