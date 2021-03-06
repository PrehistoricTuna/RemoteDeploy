﻿using AutoBurnInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TCT.ShareLib.LogManager;

namespace RemoteDeploy.EquData
{
    /// <summary>
    /// Comm设备类
    /// </summary>
    public class CommDevice : ZCDevice
    {
        private string m_CommCorePath = System.Windows.Forms.Application.StartupPath + "\\Config\\ZCConfig\\COMM\\vxWorks";

        /// <summary>
        /// Comm设备的构造函数
        /// </summary>
        /// <param name="product">该设备所属的产品</param>
        public CommDevice(IProduct product)
            : base(product)
        {
            LogManager.InfoLog.LogProcInfo("CommDevice", "CommDevice", "创建ZC产品" + product.ProductID + "COMM设备");
        }

        /// <summary>
        /// 重写DeployExec函数
        /// </summary>
        public override void  DeployExec()
        {
            
 	        ZCProduct zc = base.BelongProduct as ZCProduct;
            if (zc.AddState.m_comSent == true)
            {
                return;
            }
            if (zc.SkipFlag == true)
            {
                return;
            }
            if(zc.DeployConfigCheck.IsCoreCheck)
            {
                if (!zc.Bfis.Exists((BurnFileInfo item) => item.FileType == FileType.CommCore))
                { 
                    byte[] fileCoreData = File.ReadAllBytes(m_CommCorePath);
                    zc.Bfis.Add(new BurnFileInfo(FileType.CommCore, fileCoreData));
                    string md5StrCore = BitConverter.ToString(DataPack.DataPack.GetMD5FromFile(m_CommCorePath));
                    zc.Uis.Add(new UpdateInfo(BurnDevice.Ftsm1, FileType.CommCore, md5StrCore));
                    zc.Uis.Add(new UpdateInfo(BurnDevice.Ftsm2, FileType.CommCore, md5StrCore));
                }
            }
            zc.AddState.m_comSent = true;
        }
    }
}
