using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteDeploy.Common
{
    #region 枚举

    /// <summary>
    /// VOBC指令类型
    /// </summary>
    public enum vobcCommandType
    {
        sendFile = 0x00, //文件发送命令
        startUpdateFile = 0x01,//更新文件命令
        checkFile = 0x02,//文件校验命令
        cutLink = 0x03,//断开链接
        systemReset = 0x04,//系统复位
        vobcInfoRequest = 0x07,//VOBC信息获取
        heart = 0x08,//心跳
        buildLink = 0x09,//建立链接
        systemRestart = 0x0A,//系统重启
        fileTransRequest = 0x0B,//文件传输请求
        stopUpdateFile = 0x0C,//停止更新文件命令
        CcovGetMD5 = 0x0D,//CCOV向上位机索取子子系统MD5码
    }

    /// <summary>
    /// VOBC子系统烧录显示状态
    /// </summary>
    public enum vobcSystemDeployState
    {
        Normal,
        FileUploading ,
        FileCheck ,
        DevRestart ,
        FileUpdating 
    }

    /// <summary>
    /// VOBC子系统标识
    /// </summary>
    public enum vobcSystemType
    {
        ATP_1=0x01,//ATP1
        ATP_2 = 0x02,//ATP2
        ATP_3 = 0x03,//ATP3
        ATO_1 = 0x04,//ATO1
        ATO_2 = 0x05,//ATO2
        COM_1 = 0x06,//COM1
        COM_2 = 0x07,//COM2
        MMI = 0x08,//MMI
        CCOV = 0x09,//COOV
        ALL = 0x0A,//用于刷新特定状态时全部子系统通用
        INVALID=0x00//无效
    }

    /// <summary>
    /// VOBC部署文件类型
    /// </summary>
    public enum vobcFileType
    {
        CORE=0x01,//内核文件（应用程序）
        DATA = 0x02,//数据文件（数据）
        NVRAM = 0x04,//配置文件（NVRAM）
        CCOVConfig = 0x10,//CCOV配置文件
        BootLoader=0x08,//引导文件（BootLoader/BootRom）
        INVALID=0x00//无效
    }

    #endregion
}
