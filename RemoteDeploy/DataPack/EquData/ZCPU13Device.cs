using AutoBurnInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TCT.ShareLib.LogManager;

namespace RemoteDeploy.EquData
{
    /// <summary>
    /// ZCPU13设备类
    /// </summary>
    public class ZCPU13Device:ZCDevice
    {
        private string m_ZcPu13CorePath = System.Windows.Forms.Application.StartupPath + "\\Config\\ZCConfig\\PU13\\vxWorks";
        private string m_ZcPu13DataPath = System.Windows.Forms.Application.StartupPath + "\\Config\\ZCConfig\\ZC.FS";
        private string m_ZcPu13ConfigPath = System.Windows.Forms.Application.StartupPath + "\\Config\\ZCConfig\\PU.CFG";
        public ZCPU13Device(IProduct product)
            : base(product)
        {
            LogManager.InfoLog.LogProcInfo("ZCPU13Device", "ZCPU13Device", "创建ZC产品" + product.ProductID + "PU13设备");
        }
        public override void  DeployExec()
        {
            
 	        ZCProduct zc = base.BelongProduct as ZCProduct;
            if (zc.AddState.m_pu13Sent == true)
            {
                return;
            }
            if (zc.SkipFlag == true)
            {
                return;
            }
            if(zc.DeployConfigCheck.IsCoreCheck)
            {
                if (!zc.Bfis.Exists((BurnFileInfo item) => item.FileType == FileType.PU13Core))
                { 
                    byte[] fileCoreData = File.ReadAllBytes(m_ZcPu13CorePath);
                    zc.Bfis.Add(new BurnFileInfo(FileType.PU13Core, fileCoreData));
                    string md5StrCore = BitConverter.ToString(DataPack.DataPack.GetMD5FromFile(m_ZcPu13CorePath));
                    zc.Uis.Add(new UpdateInfo(BurnDevice.Host1, FileType.PU13Core, md5StrCore));
                    zc.Uis.Add(new UpdateInfo(BurnDevice.Host3, FileType.PU13Core, md5StrCore));
                }
            }
            if (zc.DeployConfigCheck.IsDataCheck)
            {
                if (!zc.Bfis.Exists((BurnFileInfo item) => item.FileType == FileType.ZcFs))
                {
                    byte[] fileData = File.ReadAllBytes(m_ZcPu13DataPath);
                    zc.Bfis.Add(new BurnFileInfo(FileType.ZcFs, fileData));
                    string md5StrData = BitConverter.ToString(DataPack.DataPack.GetMD5FromFile(m_ZcPu13DataPath));
                    zc.Uis.Add(new UpdateInfo(BurnDevice.Host1, FileType.ZcFs, md5StrData));
                    zc.Uis.Add(new UpdateInfo(BurnDevice.Host3, FileType.ZcFs, md5StrData));
                }
            }
            if (zc.DeployConfigCheck.IsIniCheck)
            {
                if (!zc.Bfis.Exists((BurnFileInfo item) => item.FileType == FileType.PuCfg))
                {
                    byte[] fileConfigData = File.ReadAllBytes(m_ZcPu13ConfigPath);
                    zc.Bfis.Add(new BurnFileInfo(FileType.PuCfg, fileConfigData));
                    string md5StrConfig = BitConverter.ToString(DataPack.DataPack.GetMD5FromFile(m_ZcPu13ConfigPath));
                    zc.Uis.Add(new UpdateInfo(BurnDevice.Host1, FileType.PuCfg, md5StrConfig));
                    zc.Uis.Add(new UpdateInfo(BurnDevice.Host3, FileType.PuCfg, md5StrConfig));
                }
            }
            zc.AddState.m_pu13Sent = true;
        }
    }
}
