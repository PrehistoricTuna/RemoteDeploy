using AutoBurnInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TCT.ShareLib.LogManager;

namespace RemoteDeploy.EquData
{
    public class CcDevice : ZCDevice
    {
        private string m_CcCorePath = System.Windows.Forms.Application.StartupPath + "\\Config\\ZCConfig\\CC\\vxWorks";
        private string m_CcDataPath = System.Windows.Forms.Application.StartupPath + "\\Config\\ZCConfig\\CCOZ.FS";
        private string m_CcConfig1Path = System.Windows.Forms.Application.StartupPath + "\\Config\\ZCConfig\\CC1.CFG";
        private string m_CcConfig2Path = System.Windows.Forms.Application.StartupPath + "\\Config\\ZCConfig\\CC2.CFG";
        public CcDevice(IProduct product)
            : base(product)
        {
            LogManager.InfoLog.LogProcInfo("CcDevice", "CcDevice", "创建ZC产品" + product.ProductID + "Cc设备");
        }
        public override void DeployExec()
        {

            ZCProduct zc = base.BelongProduct as ZCProduct;
            if (zc.AddState.m_ccSent == true)
            {
                return;
            }
            if (zc.SkipFlag == true)
            {
                return;
            }
            if (zc.DeployConfigCheck.IsCoreCheck)
            {
                if (!zc.Bfis.Exists((BurnFileInfo item) => item.FileType == FileType.CcCore))
                {
                    byte[] fileCoreData = File.ReadAllBytes(m_CcCorePath);
                    zc.Bfis.Add(new BurnFileInfo(FileType.CcCore, fileCoreData));
                    string md5StrCore = BitConverter.ToString(DataPack.DataPack.GetMD5FromFile(m_CcCorePath));
                    zc.Uis.Add(new UpdateInfo(BurnDevice.Ccov1, FileType.CcCore, md5StrCore));
                    zc.Uis.Add(new UpdateInfo(BurnDevice.Ccov2, FileType.CcCore, md5StrCore));
                }
            }
            if (zc.DeployConfigCheck.IsDataCheck)
            {
                if (!zc.Bfis.Exists((BurnFileInfo item) => item.FileType == FileType.CcFs))
                {
                    byte[] fileData = File.ReadAllBytes(m_CcDataPath);
                    zc.Bfis.Add(new BurnFileInfo(FileType.CcFs, fileData));
                    string md5StrData = BitConverter.ToString(DataPack.DataPack.GetMD5FromFile(m_CcDataPath));
                    zc.Uis.Add(new UpdateInfo(BurnDevice.Ccov1, FileType.CcFs, md5StrData));
                    zc.Uis.Add(new UpdateInfo(BurnDevice.Ccov2, FileType.CcFs, md5StrData));
                }
            }
            if (zc.DeployConfigCheck.IsIniCheck)
            {
                if (!zc.Bfis.Exists((BurnFileInfo item) => item.FileType == FileType.Cc1Cfg))
                {
                    byte[] fileConfig1Data = File.ReadAllBytes(m_CcConfig1Path);
                    zc.Bfis.Add(new BurnFileInfo(FileType.Cc1Cfg, fileConfig1Data));
                    string md5StrConfig1 = BitConverter.ToString(DataPack.DataPack.GetMD5FromFile(m_CcConfig1Path));
                    zc.Uis.Add(new UpdateInfo(BurnDevice.Ccov1, FileType.Cc1Cfg, md5StrConfig1));
                }
                if (!zc.Bfis.Exists((BurnFileInfo item) => item.FileType == FileType.Cc2Cfg))
                {
                    byte[] fileConfig2Data = File.ReadAllBytes(m_CcConfig2Path);
                    zc.Bfis.Add(new BurnFileInfo(FileType.Cc2Cfg, fileConfig2Data));
                    string md5StrConfig2 = BitConverter.ToString(DataPack.DataPack.GetMD5FromFile(m_CcConfig2Path));
                    zc.Uis.Add(new UpdateInfo(BurnDevice.Ccov2, FileType.Cc2Cfg, md5StrConfig2));
                }
            }
            zc.AddState.m_ccSent = true;
        }
    }
}
