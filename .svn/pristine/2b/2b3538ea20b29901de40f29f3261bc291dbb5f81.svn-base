using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteDeploy.Common
{
    /// <summary>
    /// 公共处理函数
    /// </summary>
    public static class CommonMethod
    {

        /// <summary>
        /// 通过输入参数 vobc子系统类型 获取vobc子系统名称
        /// </summary>
        /// <param name="sType">vobc子系统类型</param>
        /// <returns>vobc子系统名称</returns>
        public static string GetVobcSystemNameByType(vobcSystemType sType) 
        {
            string sName = string.Empty;

            switch (sType)
            {
                case vobcSystemType.ATP_1:
                    sName = "ATP_1";
                    break;
                case vobcSystemType.ATP_2:
                    sName = "ATP_2";
                    break;
                case vobcSystemType.ATP_3:
                    sName = "ATP_3";
                    break;
                case vobcSystemType.ATO_1:
                    sName = "ATO_1";
                    break;
                case vobcSystemType.ATO_2:
                    sName = "ATO_2";
                    break;
                case vobcSystemType.COM_1:
                    sName = "COM_1";
                    break;
                case vobcSystemType.COM_2:
                    sName = "COM_2";
                    break;
                case vobcSystemType.MMI:
                    sName = "MMI";
                    break;
                case vobcSystemType.CCOV:
                    sName = "CCOV";
                    break;
                case vobcSystemType.INVALID:
                    sName = "";
                    break;
                default:
                    break;
            }

            return sName;

        }

        /// <summary>
        /// 通过输入参数 vobc子系统类型 获取vobc子系统集合
        /// 用途：上位机上传文件时，例如ATP，只会上传一份ATP文件，但实际界面
        /// 显示时，需要显示3个ATP的状态
        /// </summary>
        /// <param name="sType">vobc子系统类型</param>
        /// <returns>vobc子系统类型集合</returns>
        public static List<vobcSystemType> GetVobcSystemListByType(vobcSystemType sType)
        {
            List<vobcSystemType> typeList = new List<vobcSystemType>();

            switch (sType)
            {
                case vobcSystemType.ATP_1:
                    typeList.Add(vobcSystemType.ATP_1);
                    typeList.Add(vobcSystemType.ATP_2);
                    typeList.Add(vobcSystemType.ATP_3);
                    break;
                case vobcSystemType.ATP_2:
                    break;
                case vobcSystemType.ATP_3:
                    break;
                case vobcSystemType.ATO_1:
                    typeList.Add(vobcSystemType.ATO_1);
                    typeList.Add(vobcSystemType.ATO_2);
                    break;
                case vobcSystemType.ATO_2:
                    break;
                case vobcSystemType.COM_1:
                    typeList.Add(vobcSystemType.COM_1);
                    typeList.Add(vobcSystemType.COM_2);
                    break;
                case vobcSystemType.COM_2:
                    break;
                case vobcSystemType.MMI:
                    typeList.Add(vobcSystemType.MMI);
                    break;
                case vobcSystemType.CCOV:
                    typeList.Add(vobcSystemType.CCOV);
                    break;
                case vobcSystemType.ALL:
                    typeList.Add(vobcSystemType.ATP_1);
                    typeList.Add(vobcSystemType.ATP_2);
                    typeList.Add(vobcSystemType.ATP_3);
                    typeList.Add(vobcSystemType.ATO_1);
                    typeList.Add(vobcSystemType.ATO_2);
                    typeList.Add(vobcSystemType.COM_1);
                    typeList.Add(vobcSystemType.COM_2);
                    typeList.Add(vobcSystemType.MMI);
                    typeList.Add(vobcSystemType.CCOV);
                    break;
                case vobcSystemType.INVALID:
                    break;
                default:
                    break;
            }

            return typeList;

        }

        /// <summary>
        /// 通过输入参数 获取vobc子系统烧录状态显示信息
        /// </summary>
        /// <param name="sType">vobc子系统烧录状态</param>
        /// <returns>对应名称</returns>
        public static string GetVobcDeployNameByType(vobcSystemDeployState sType)
        {
            string sName = string.Empty;

            switch (sType)
            {
                case vobcSystemDeployState.Normal:
                    sName = "正常就绪";
                    break;
                case vobcSystemDeployState.FileUploading:
                    sName = "文件上传中";
                    break;
                case vobcSystemDeployState.FileCheck:
                    sName = "文件校验中";
                    break;
                case vobcSystemDeployState.DevRestart:
                    sName = "设备待重启";
                    break;
                case vobcSystemDeployState.FileUpdating:
                    sName = "文件更新中";
                    break;
                default:
                    break;
            }

            return sName;

        }

        /// <summary>
        /// 依据vobc子系统类型获取对应该类型的nameof值
        /// </summary>
        /// <param name="type">vobc子系统类型</param>
        /// <returns>转化后的值</returns>
        public static  string GetStringByType(vobcSystemType type)
        {
            //返回值变量定义
            string rtnString = string.Empty;

            //根据输入的子系统类型 转译成子系统名称
            switch (type)
            {
                case vobcSystemType.ATO_1:
                    rtnString = "ATO_1";
                    break;
                case vobcSystemType.ATO_2:
                    rtnString = "ATO_2";
                    break;
                case vobcSystemType.ATP_1:
                    rtnString = "ATP_1";
                    break;
                case vobcSystemType.ATP_2:
                    rtnString = "ATP_2";
                    break;
                case vobcSystemType.ATP_3:
                    rtnString = "ATP_3";
                    break;
                case vobcSystemType.CCOV:
                    rtnString = "CCOV";
                    break;
                case vobcSystemType.COM_1:
                    rtnString = "COM_1";
                    break;
                case vobcSystemType.COM_2:
                    rtnString = "COM_2";
                    break;
                case vobcSystemType.MMI:
                    rtnString = "MMI";
                    break;
                default:
                    rtnString = "";
                    break;
            }

            return rtnString;

        }

        /// <summary>
        /// 根据vobc烧录文件类型获取对应该类型的nameof值
        /// </summary>
        /// <param name="type">vobc烧录文件类型</param>
        /// <returns></returns>
        public static string GetFileByType(vobcFileType type)
        {
            //返回值变量定义
            string rtnString = "";

            //根据输入的子系统文件类型 转译成子系统文件名称
            switch (type)
            {
                case vobcFileType.BootLoader:
                    rtnString = "BootLoader";
                    break;
                case vobcFileType.CCOVConfig:
                    rtnString = "CCOVConfig";
                    break;
                case vobcFileType.CORE:
                    rtnString = "Core";
                    break;
                case vobcFileType.DATA:
                    rtnString = "Data";
                    break;
                case vobcFileType.NVRAM:
                    rtnString = "NvRam";
                    break;
                default:
                    break;
            }

            return rtnString;

        }

    }
}
