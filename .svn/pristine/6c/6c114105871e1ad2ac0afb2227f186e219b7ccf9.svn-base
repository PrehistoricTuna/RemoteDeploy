using AutoBurnInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TCT.ShareLib.LogManager;

namespace RemoteDeploy.EquData
{
    public class ZCPU24Device:ZCDevice
    {
        private string m_ZcPu24CorePath = System.Windows.Forms.Application.StartupPath + "\\Config\\ZCConfig\\PU24\\vxWorks";
        private string m_ZcPu24DataPath = System.Windows.Forms.Application.StartupPath + "\\Config\\ZCConfig\\ZC.FS";
        private string m_ZcPu24ConfigPath = System.Windows.Forms.Application.StartupPath + "\\Config\\ZCConfig\\PU.CFG";
        public ZCPU24Device(IProduct product)
            : base(product)
        {
            LogManager.InfoLog.LogProcInfo("ZCPU24Device", "ZCPU24Device", "创建ZC产品" + product.ProductID + "PU24设备");
        }
        public override void  DeployExec()
        {
            
 	        ZCProduct zc = base.BelongProduct as ZCProduct;
            if (zc.AddState.m_pu24Sent == true)
            {
                return;
            }
            if (zc.SkipFlag == true)
            {
                return;
            }
            if(zc.DeployConfigCheck.IsCoreCheck)
            {
                if (!zc.Bfis.Exists((BurnFileInfo item) => item.FileType == FileType.PU24Core))
                { 
                    byte[] fileCoreData = File.ReadAllBytes(m_ZcPu24CorePath);
                    zc.Bfis.Add(new BurnFileInfo(FileType.PU24Core, fileCoreData));
                    string md5StrCore = BitConverter.ToString(DataPack.DataPack.GetMD5FromFile(m_ZcPu24CorePath));
                    zc.Uis.Add(new UpdateInfo(BurnDevice.Host2, FileType.PU24Core, md5StrCore));
                    zc.Uis.Add(new UpdateInfo(BurnDevice.Host4, FileType.PU24Core, md5StrCore));
                }
            }
            if (zc.DeployConfigCheck.IsDataCheck)
            {
                //if (!zc.Bfis.Exists((BurnFileInfo item) => item.FileType == FileType.ZcFs))
                //{
                    byte[] fileData = File.ReadAllBytes(m_ZcPu24DataPath);
                    zc.Bfis.Add(new BurnFileInfo(FileType.ZcFs, fileData));
                    string md5StrData = BitConverter.ToString(DataPack.DataPack.GetMD5FromFile(m_ZcPu24DataPath));
                    zc.Uis.Add(new UpdateInfo(BurnDevice.Host2, FileType.ZcFs, md5StrData));
                    zc.Uis.Add(new UpdateInfo(BurnDevice.Host4, FileType.ZcFs, md5StrData));
                //}
            }
            if (zc.DeployConfigCheck.IsIniCheck)
            {
                //if (!zc.Bfis.Exists((BurnFileInfo item) => item.FileType == FileType.PuCfg))
                //{
                    byte[] fileConfigData = File.ReadAllBytes(m_ZcPu24ConfigPath);
                    zc.Bfis.Add(new BurnFileInfo(FileType.PuCfg, fileConfigData));
                    string md5StrConfig = BitConverter.ToString(DataPack.DataPack.GetMD5FromFile(m_ZcPu24ConfigPath));
                    zc.Uis.Add(new UpdateInfo(BurnDevice.Host2, FileType.PuCfg, md5StrConfig));
                    zc.Uis.Add(new UpdateInfo(BurnDevice.Host4, FileType.PuCfg, md5StrConfig));
                //}
            }
            zc.AddState.m_pu24Sent = true;
        }
    }
}
