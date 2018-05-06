﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RemoteDeploy.ControlDispatcher;
using RemoteDeploy.Command;
using RemoteDeploy.SendRecv.Send;
using RemoteDeploy.SendRecv.Recv;
using System.ServiceModel;
using RemoteDeploy.EquData;
using RemoteDeploy.Observer;
using RemoteDeploy.Common;
using System.Windows.Forms.VisualStyles;
using RemoteDeploy.View;
using System.Threading;
using RemoteDeploy.NetworkService;
using AutoBurnInterface;
using System.IO;
using TCT.ShareLib.LogManager;
using RemoteDeploy.DataPack;


namespace RemoteDeploy
{

    public partial class MainWindow : Form
    {

        #region 变量定义

        //产品容器类实例
        private IProContainer container = null;

        //当前正在使用的DataGridView控件
        private DataGridView curruntDataGridView = null;

        //当前已选中的实体信息集合
        private List<IDevice> selectedDevice = new List<IDevice>();

        //观察者界面实例
        private FormObserver observer = null;

        //VOBC子子系统部署文件检查状态类实例
        private DeployConfiState deployConfigCheck = new DeployConfiState();

        private bool selectConfirmed = false;

        private Dictionary<BackgroundWorker, IProduct> wokerList = new Dictionary<BackgroundWorker, IProduct>();
        #endregion

        #region 构造函数

        public MainWindow()
        {
            InitializeComponent();

            //初始化操作
            APPInit();
        }

        #endregion


        #region 事件

        /// <summary>
        /// 界面观察者实现，用于刷新Column_VOBCState
        /// </summary>
        public void container_EBackData()
        {
            try
            {
                this.Invoke(new Action(delegate()
                {
                    if (container == CDeviceDataFactory.Instance.VobcContainer)
                    {
                        //刷新VOBC状态数据
                        for (int index = 0; index < dataGrid_VOBC.Rows.Count; index++)
                        {
                            IProduct product = CDeviceDataFactory.Instance.VobcContainer.Find((IProduct temp) => temp.ProductID == Convert.ToString(dataGrid_VOBC.Rows[index].Cells["Column_VOBCID"].Value));
                            dataGrid_VOBC.Rows[index].Cells["Column_VOBCState"].Value = product.ProductState;
                            dataGrid_VOBC.Rows[index].Cells["Column_VOBCSystem"].Value = product.DeployProcess().ToString() + "%";

                        }
                    }
                    else if (container == CDeviceDataFactory.Instance.ZcContainer)
                    {
                        //刷新ZC状态数据
                        for (int index = 0; index < dataGrid_ZC.Rows.Count; index++)
                        {
                            IProduct product = CDeviceDataFactory.Instance.ZcContainer.Find((IProduct temp) => temp.ProductID == Convert.ToString(dataGrid_ZC.Rows[index].Cells["Column_ZCArea"].Value));
                            dataGrid_ZC.Rows[index].Cells["Column_ZCState"].Value = product.ProductState;

                        }
                    }
                    //TODO:DSU,CI
                    ///刷新详细信息界面
                    RefreshDataDetail();
                }));
            }
            catch (Exception ex)
            {
                throw new MyException(ex.ToString());
            }
        }

        /// <summary>
        /// 后台线程控件的执行完成通知事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerDeploy_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            ResetState();

        }
        private void ResetState()
        {
            if (container.FindAll(tar => (tar.InProcess == true)).Count == 0)
            {
                //启用部署按钮
                tsbDeploy.Enabled = true;

                //禁用停止部署按钮
                tsbStop.Enabled = false;

                //启用确认选择按钮
                button_OK.Enabled = true;
                //Thread.Sleep(50000);
                //test_Click(null, e);
            }
        }

        /// <summary>
        /// 部署按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbDeploy_Click(object sender, EventArgs e)
        {
            if (selectConfirmed)
            {
                bool deviceSelect = false;

                for (int i = 0; i < dataGrid_VOBC.Rows.Count; i++)
                {
                    if ((bool)dataGrid_VOBC.Rows[i].Cells["Column_VOBCCC12"].EditedFormattedValue == true)
                    {
                        deviceSelect = true;
                        break;
                    }
                    else if ((bool)dataGrid_VOBC.Rows[i].Cells["Column_VOBCCOM"].EditedFormattedValue == true)
                    {
                        deviceSelect = true;
                        break;
                    }
                    else if ((bool)dataGrid_VOBC.Rows[i].Cells["Column_VOBCATP"].EditedFormattedValue == true)
                    {
                        deviceSelect = true;
                        break;
                    }
                    else if ((bool)dataGrid_VOBC.Rows[i].Cells["Column_VOBCATO"].EditedFormattedValue == true)
                    {
                        deviceSelect = true;
                        break;
                    }
                    else if ((bool)dataGrid_VOBC.Rows[i].Cells["Column_VOBCMMI"].EditedFormattedValue == true)
                    {
                        deviceSelect = true;
                        break;
                    }
                    else
                    {
                    }
                }
                ///判断子系统文件选择框选中状态
                if ((checkBox_core.Checked == false && checkBox_data.Checked == false && checkBox_ini.Checked == false && checkBox_nvram.Checked == false && checkBox_bootloader.Checked == false) || (deviceSelect == false))
                {
                    System.Windows.Forms.MessageBox.Show("请选择烧录文件!");
                    return;
                }

                //生成二次确认弹窗
                Confirm f3 = new Confirm
                {
                    StartPosition = FormStartPosition.CenterParent
                };

                //弹窗并获取返回值用于传值
                DialogResult dr = f3.ShowDialog();

                if (dr == DialogResult.Cancel)
                {
                    return;
                }
                else if (dr == DialogResult.OK)
                {
                    //禁用当前按钮
                    tsbDeploy.Enabled = false;

                    //启用停止按钮
                    tsbStop.Enabled = true;

                    //禁用确认选择按钮
                    button_OK.Enabled = false;

                    //后台线程执行部署操作
                    //backgroundWorkerDeploy.RunWorkerAsync();
                    wokerList.Clear();
                    foreach (IProduct pro in container)
                    {
                        if (pro.CSelectedDevice.Count != 0)
                        {
                            pro.InProcess = false;
                            BackgroundWorker worker = new BackgroundWorker();
                            worker.DoWork += Worker_DoWork;
                            worker.RunWorkerCompleted += backgroundWorkerDeploy_RunWorkerCompleted;
                            wokerList.Add(worker, pro);
                        }
                    }
                    foreach (KeyValuePair<BackgroundWorker, IProduct> pair in wokerList)
                    {
                        pair.Key.RunWorkerAsync(pair.Value);
                    }
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("请确认选择!");
                return;
            }


            
        }

        /// <summary>
        /// 状态查看按钮 单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbStateUpdate_Click(object sender, EventArgs e)
        {
            //当前为VOBC部署模式
            if (curruntDataGridView == dataGrid_VOBC)
            {
                //遍历VOBC实体数据
                foreach (DataGridViewRow oneProduct in dataGrid_VOBC.SelectedRows)
                {
                    //获取产品实例
                    IProduct product = container[oneProduct.Index] as IProduct;

                    DateTime beginTime = DateTime.Now;
                    while (true)
                    {

                        if (product.ProductState == "正常")
                        {
                            CommandQueue.instance.m_CommandQueue.Enqueue(new VOBCCommand(product.Ip, Convert.ToInt32(product.Port), product.ProductID, vobcCommandType.vobcInfoRequest));
                            break;
                        }
                        //
                        else
                        {
                            break;
                        }
                        /*else
                        {
                            Thread.Sleep(1000);

                            //CommandQueue.instance.m_CommandQueue.Enqueue(new VOBCCommand(product.Ip, Convert.ToInt32(product.Port), product.ProductID, vobcCommandType.buildLink));

                            TimeSpan span = DateTime.Now - beginTime;
                            if (span.TotalMilliseconds > 5000)
                            {
                                break;
                            }

                        }*/
                    }
                }
            }
            else if (curruntDataGridView == dataGrid_ZC)
            {
                //foreach (DataGridViewRow oneProduct in dataGrid_ZC.SelectedRows)
                //{
                //    IProduct device = container[oneProduct.Index] as IProduct;
                //    CommandQueue.instance.m_CommandQueue.Enqueue(new DeployCommand(device.ProductID, device.Name));
                //}

                try
                {

                    foreach (DataGridViewRow zcProduct in dataGrid_ZC.SelectedRows)
                    {
                        ZCProduct product = container[zcProduct.Index] as ZCProduct;
                        if (product.AutoBurnPush != null)
                        {
                            List<BurnDevice> bds = new List<BurnDevice>();
                            bds.Add(BurnDevice.Host1);
                            bds.Add(BurnDevice.Host2);
                            bds.Add(BurnDevice.Host3);
                            bds.Add(BurnDevice.Host4);
                            bds.Add(BurnDevice.Ccov1);
                            bds.Add(BurnDevice.Ccov2);
                            bds.Add(BurnDevice.Ftsm1);
                            bds.Add(BurnDevice.Ftsm2);
                            Dictionary<BurnDevice, StatusInfo> bs = product.AutoBurnPush.IRequest.GetStatusInfo(bds);
                            foreach (KeyValuePair<BurnDevice, StatusInfo> item in bs)
                            {
                                string deviceName = product.GetDeviceNameByBurnType(item.Key);
                                if (deviceName != "")
                                {
                                    DeviceState state = new DeviceState(item.Value.CommStatus, item.Value.Version,item.Value.FsVersion);
                                    CDeviceDataFactory.Instance.ZcContainer.SetProductDeviceState(product.Ip, deviceName, state);
                                }
                            }
                            CDeviceDataFactory.Instance.ZcContainer.dataModify.Modify();

                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

        /// <summary>
        /// 停止按钮 单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbStop_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow oneProduct in dataGrid_VOBC.SelectedRows)
            {
                IProduct product = container[oneProduct.Index] as IProduct;

                //发送停止信息帧
                CommandQueue.instance.m_CommandQueue.Enqueue(new VOBCCommand(product.Ip, Convert.ToInt32(product.Port), product.ProductID, vobcCommandType.stopUpdateFile));

                //发送断链请求帧
                CommandQueue.instance.m_CommandQueue.Enqueue(new VOBCCommand(product.Ip, Convert.ToInt32(product.Port), product.ProductID, vobcCommandType.cutLink));
            }

            //TODO:判定后台线程工作状态
            //if (backgroundWorkerDeploy.IsBusy == true)
            //{
            //    //取消工作
            //    backgroundWorkerDeploy.CancelAsync();
            //}
            //APPInit();
            //TODO:尚未确定方案,点击停止后应该发送停止更新请求还是终止FTP传输进程???如果是终止FTP部署则关联上面DoWork进程,如发送停止更新请求(已经在部署之后,CCOV更新或其他过程中),则不需要终止进程,只发送请求停止数据包即可(是否允许停止在数据分析中负责回显).
            tsbStop.Enabled = false;
            tsbDeploy.Enabled = true;
            button_OK.Enabled = true;
        }

        //timer计时器事件  执行心跳帧发送
        private void timer1_Tick(object sender, EventArgs e)
        {
            
            foreach(VOBCProduct product in CDeviceDataFactory.Instance.VobcContainer)
            {
                if(product.CTcpClient != null)
                {
                    if (product.CTcpClient.clientSocket != null)
                    {
                        LogManager.InfoLog.LogCommunicationInfo("MainWindow", "timer1_Tick", "发送VOBC" + product.ProductID + "心跳帧");
                        product.CTcpClient.Me_SendMessage(DataPack.DataPack.PackHeartbeatRequest());
                    }
                }
            }

        }

        /// <summary>
        /// 窗体关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (VOBCProduct product in CDeviceDataFactory.Instance.VobcContainer)
            {
                if (product.CTcpClient != null)
                {
                    product.CTcpClient.Socket_TCPClient_Dispose();
                }
            }
        }

        /// <summary>
        /// 建立连接按钮单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void linkEstab_Click(object sender, EventArgs e)
        {
            //遍历用户选择的需要建立链接的产品对象信息
            foreach (DataGridViewRow oneProduct in dataGrid_VOBC.SelectedRows)
            {
                //获取产品实例对象
                IProduct product = container[oneProduct.Index] as IProduct;
                if (product.CTcpClient != null)
                {
                    product.CTcpClient.Socket_TCPClient_Dispose();
                    InitDataGridViewColumns();
                    InitSelectedDevice();
                }
                //发送建链信息帧
                CommandQueue.instance.m_CommandQueue.Enqueue(new VOBCCommand(product.Ip, Convert.ToInt32(product.Port), product.ProductID, vobcCommandType.buildLink));                

                if (timer2.Enabled == false)
                {
                    timer2.Enabled = true;
                }
                /*timer2Count++;
                if (timer2Count <= 2)
                {
                    timer2_Tick(null, e);
                }
                else
                {
                    timer2.Stop();
                    timer2.Dispose();
                }*/
                /*//等待15秒判断是否进行二次重连
                
                if (product.ProductState != "正常")
                {
                    CommandQueue.instance.m_CommandQueue.Enqueue(new VOBCCommand(product.Ip, Convert.ToInt32(product.Port), product.ProductID, vobcCommandType.buildLink));
                }
                else
                {
                    return;
                }

                //
                
                if (product.ProductState != "正常")
                {
                    //刷新界面日志信息
                    //new WindowReport().ReportWindow("建链失败，请重启车载设备！");
                }
                else
                {
                    return;
                }*/
            }
        }

        /// <summary>
        /// 登录按钮单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_login_Click(object sender, EventArgs e)
        {
            //弹出登录窗口并显示在屏幕中央区域
            Login f2 = new Login();

            f2.StartPosition = FormStartPosition.CenterParent;

            f2.Show();

        }

        /// <summary>
        /// 选择详细查看的设备
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            //清空已选设备列表
            selectedDevice.Clear();

            //遍历数据
            for (int i = 0; i < curruntDataGridView.Rows.Count; i++)
            {
                container[i].CSelectedDeviceType.Clear();
                container[i].CSelectedDevice.Clear();
                for (int j = 0; j < curruntDataGridView.Columns.Count; j++)
                {
                    if (curruntDataGridView.Columns[j].CellType.Name == "DataGridViewCheckBoxCell")
                    {
                        if ((bool)curruntDataGridView.Rows[i].Cells[j].EditedFormattedValue == true)
                        {
                            List<IDevice> deviceList = container[i].CBelongsDevice.FindAll((IDevice temp) => temp.DeviceType == curruntDataGridView.Columns[j].HeaderText);
                            container[i].CSelectedDeviceType.Add(curruntDataGridView.Columns[j].HeaderText);
                            selectedDevice.AddRange(deviceList);

                            container[i].CSelectedDevice.AddRange(deviceList);
                        }
                    }
                }
            }

            //判断是否存在确认选择的的设备
            if (selectedDevice.Count > 0)
            {
                selectConfirmed = true;
            }
            else
            {
                selectConfirmed = false;
            }

            //刷新界面
            RefreshDataDetail();
            tsbDeploy.Enabled = true;

        }

        /// <summary>
        /// 选项卡选中变化事件，状态迁移，重新初始化选中设备，刷新界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl_Container_SelectedIndexChanged(object sender, EventArgs e)
        {
            string name = tabControl_Container.SelectedTab.Name;
            if (name == "tabPage_VOBC")
            {
                container = CDeviceDataFactory.Instance.VobcContainer;
                curruntDataGridView = dataGrid_VOBC;
            }
            else if (name == "tabPage_ZC")
            {
                container = CDeviceDataFactory.Instance.ZcContainer;
                curruntDataGridView = dataGrid_ZC;
                foreach (ZCProduct zc in CDeviceDataFactory.Instance.ZcContainer)
                {
                    zc.AutoBurnPush = new AutoBurnPush(zc.Ip, CDeviceDataFactory.Instance.ZcContainer.SetZCProc);
                    try
                    {
                        string online = zc.AutoBurnPush.IRequest.OnLine();
                        if (online == "")
                        {
                            CDeviceDataFactory.Instance.ZcContainer.SetProductState(zc.Ip, "正常");
                        }
                        else
                        {
                            rtbReportView.AppendText("ZC设备" + zc.ProductID + "初始化失败！");
                            LogManager.InfoLog.LogProcError("MainWindow", "tabControl_Container_SelectedIndexChanged", online);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogManager.InfoLog.LogProcError("MainWindow", "tabControl_Container_SelectedIndexChanged", ex.Message);
                    }
                }
                //dataGridView_VOBCDeviceDetails.Visible = false;
            }


            InitSelectedDevice();

        }

        /// <summary>
        /// 引导文件复选框触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_bootloader_CheckedChanged(object sender, EventArgs e)
        {
            deployConfigCheck.IsBootLoaderCheck = checkBox_bootloader.Checked;
        }

        /// <summary>
        /// 配置文件复选框触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_nvram_CheckedChanged(object sender, EventArgs e)
        {
            deployConfigCheck.IsNvRamCheck = checkBox_nvram.Checked;
        }

        /// <summary>
        /// 内核文件复选框触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_core_CheckedChanged(object sender, EventArgs e)
        {
            deployConfigCheck.IsCoreCheck = checkBox_core.Checked;
        }

        /// <summary>
        /// 数据文件复选框触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_data_CheckedChanged(object sender, EventArgs e)
        {
            deployConfigCheck.IsDataCheck = checkBox_data.Checked;
        }

        /// <summary>
        /// ini配置文件复选框触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_ini_CheckedChanged(object sender, EventArgs e)
        {
            deployConfigCheck.IsIniCheck = checkBox_ini.Checked;
        }

        #endregion

        #region 函数实现

        /// <summary>
        /// 初始化操作
        /// </summary>
        private void APPInit()
        {
            
            //禁用部署按钮
            tsbDeploy.Enabled = false;

            //禁用停止按钮
            tsbStop.Enabled = false;

            //命令集合初始化
            CommandQueue.instance.m_CommandQueue.Enqueue(new InitCommand());

            ///初始化发送接收线程
            Send send = new Send();
            Recv recv = new Recv();
            send.Init();
            recv.Init();

            ///初始化线路数据
            CDeviceDataFactory.Instance.LoadXml(System.Windows.Forms.Application.StartupPath + "\\Config\\TopoConfig.xml");
            curruntDataGridView = dataGrid_VOBC;

            DataGridViewButtonColumn check = new DataGridViewButtonColumn();
            check.HeaderText = "查看";
            check.Text = "查看";
            check.Name = "details";
            check.UseColumnTextForButtonValue = true;
            dataGrid_VOBC.Columns.Add(check);
            ///初始化控件列信息
            InitDataGridViewColumns();

            //容器默认初始化为VOBC
            container = CDeviceDataFactory.Instance.VobcContainer;

            //初始化详细信息设备列表并刷新界面
            InitSelectedDevice();

            ///界面观察者订阅所有产品的数据变化主题和进度报告主题
            this.observer = new FormObserver(this);

            //遍历产品容器集合
            foreach (IProContainer proContainer in CDeviceDataFactory.Instance.ProjectConsole.Projducts)
            {
                //观察者界面添加进程和颜色事件
                observer.AddProcess(proContainer.dataModify);
                observer.AddColorEvent(proContainer.dataModify);

                //观察者界面添加报告接口
                foreach (IProduct product in proContainer)
                {
                    observer.AddReport(product.Report);
                }
            }
        }

        /// <summary>
        /// 初始化DataGridView控件列信息
        /// </summary>
        private void InitDataGridViewColumns()
        {
            //加载VOBC显示数据
            dataGrid_VOBC.Rows.Clear();
            foreach (IProduct product in CDeviceDataFactory.Instance.VobcContainer)
            {
                int index = dataGrid_VOBC.Rows.Add();
                dataGrid_VOBC.Rows[index].Cells["Column_VOBCID"].Value = product.ProductID;
                dataGrid_VOBC.Rows[index].Cells["Column_VOBCSystem"].Value = product.DeployProcess().ToString()+"%";
                dataGrid_VOBC.Rows[index].Cells["Column_VOBCState"].Value = product.ProductState;
            }

            //加载ZC显示数据
            dataGrid_ZC.Rows.Clear();
            foreach (IProduct product in CDeviceDataFactory.Instance.ZcContainer)
            {
                int index = dataGrid_ZC.Rows.Add();
                dataGrid_ZC.Rows[index].Cells["Column_ZCArea"].Value = product.ProductID;
                dataGrid_ZC.Rows[index].Cells["Column_ZCSystem"].Value = product.Name;
                dataGrid_ZC.Rows[index].Cells["Column_ZCState"].Value = product.ProductState;
            }

        }

        /// <summary>
        /// 初始化详细信息设备列表并刷新界面
        /// </summary>
        private void InitSelectedDevice()
        {
            //先清空
            selectedDevice.Clear();

            //遍历数据并添加
            foreach (IProduct product in container)
            {
                selectedDevice.AddRange(product.CBelongsDevice);
            }

            //初始化数据
            InitDataGridViewColumns();

            foreach (IDevice device in selectedDevice)
            {
                device.PreCheckResult = false;
            }
            //界面刷新
            RefreshDataDetail();

        }

        /// <summary>
        /// DataGridView控件修改颜色
        /// </summary>
        public void DataGridView_Change_Color()
        {
            this.Invoke(new Action(delegate ()
            {
                ResetState();
            }));
            foreach (DataGridViewRow row in curruntDataGridView.Rows)
            {
                IProduct product = CDeviceDataFactory.Instance.VobcContainer.Find((IProduct temp) => temp.ProductID == Convert.ToString(row.Cells["Column_VOBCID"].Value));
                if (product.CSelectedDevice.Count != 0)
                {
                    if (product.InProcess == true)
                    { 
                        row.DefaultCellStyle.BackColor = System.Drawing.Color.Orange;
                    }
                    else
                    {
                        if (product.SkipFlag)
                        {
                            row.DefaultCellStyle.BackColor = System.Drawing.Color.Red;
                        }
                        else
                        {
                            row.DefaultCellStyle.BackColor = System.Drawing.Color.Green;
                        }
                    }
                }
            }
        }

       

        /// <summary>
        /// 界面打印区刷新
        /// </summary>
        /// <param name="reprort"></param>
        public void ProductReport(string reprort)
        {
            try
            {
                this.Invoke(new Action(delegate()
                {
                    rtbReportView.AppendText(reprort + "\n");
                }));
            }
            catch (Exception ex)
            {
                throw new MyException(ex.ToString());
            }
        }

        /// <summary>
        /// 刷新当前详细信息表格
        /// </summary>
        private void RefreshDataDetail()
        {

            //清空数据
            dataGridView_VOBCDeviceDetails.Rows.Clear();

            //VOBC使用车载ID  非VOBC 如ZC  CI  DSU等使用集中区

            dataGridView_VOBCDeviceDetails.Columns["Column_Area"].HeaderText = (curruntDataGridView == dataGrid_VOBC) ?
                "车载ID" : "集中区";

            //遍历当前已选中的实体
            foreach (IDevice device in selectedDevice)
            {

                //获取当前正在添加的数据行索引
                int index = dataGridView_VOBCDeviceDetails.Rows.Add();

                //依据数据行索引 并从数据实体中获取数据  显示到界面中
                dataGridView_VOBCDeviceDetails.Rows[index].Cells["Column_Area"].Value = device.BelongProduct.ProductID;
                dataGridView_VOBCDeviceDetails.Rows[index].Cells["Column_System"].Value = device.DeviceType;
                dataGridView_VOBCDeviceDetails.Rows[index].Cells["Column_Device"].Value = device.DeviceName;
                dataGridView_VOBCDeviceDetails.Rows[index].Cells["Column_State"].Value = device.State;
                dataGridView_VOBCDeviceDetails.Rows[index].Cells["Column_SoftVersion"].Value = device.SoftVersion;
                dataGridView_VOBCDeviceDetails.Rows[index].Cells["Column_DataVersion"].Value = device.DataVersion;
                dataGridView_VOBCDeviceDetails.Rows[index].Cells["Column_IP"].Value = device.BelongProduct.Ip;
                dataGridView_VOBCDeviceDetails.Rows[index].Cells["Column_PreResults"].Value = device.PreCheckResult ? "预检成功" : "预检失败";
                dataGridView_VOBCDeviceDetails.Rows[index].Cells["Column_ErrorReason"].Value = device.PreCheckFailReason;
                dataGridView_VOBCDeviceDetails.Rows[index].Cells["Column_Process"].Value = device.ProcessState;

                //依据状态 显示结果列的状态图标
                switch (device.State)
                {
                    case "更新成功":
                        dataGridView_VOBCDeviceDetails.Rows[index].Cells["Column_Results"].Value = RemoteDeploy.Properties.Resources.Green;
                        dataGridView_VOBCDeviceDetails.Rows[index].DefaultCellStyle.ForeColor = System.Drawing.Color.Green;
                        break;
                    case "更新失败":
                        dataGridView_VOBCDeviceDetails.Rows[index].Cells["Column_Results"].Value = RemoteDeploy.Properties.Resources.Red;
                        dataGridView_VOBCDeviceDetails.Rows[index].DefaultCellStyle.ForeColor = System.Drawing.Color.Red;
                        break;
                    case "文件上传中":
                    case "设备待重启":
                    case "更新执行中":
                    case "文件校验中":
                    case "下发完成":
                        dataGridView_VOBCDeviceDetails.Rows[index].Cells["Column_Results"].Value = RemoteDeploy.Properties.Resources.DarkOrange;
                        dataGridView_VOBCDeviceDetails.Rows[index].DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
                        break;
                    default:
                        dataGridView_VOBCDeviceDetails.Rows[index].Cells["Column_Results"].Value = RemoteDeploy.Properties.Resources.Gray;
                        dataGridView_VOBCDeviceDetails.Rows[index].DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
                        break;
                }

            }

            //清除控件选中状态
            dataGridView_VOBCDeviceDetails.ClearSelection();

        }

        /// <summary>
        /// 记录日志查询功能
        /// </summary>
        private void 历史查询ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(System.Windows.Forms.Application.StartupPath + "\\log");
        }

        /// <summary>
        /// 登录窗口
        /// </summary>
        private void 登录ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Login login = new Login();
            login.StartPosition = FormStartPosition.CenterParent;
            login.ChangeState += new ChangeButtonState(login_ChangeState);
            login.ShowDialog();
        }

        /// <summary>
        /// 登录成功
        /// </summary>
        void login_ChangeState(bool topmost)
        {
            button_OK.Enabled = true;
        }

        /// <summary>
        /// 注销
        /// </summary>
        private void 注销ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button_OK.Enabled = false;
        }

        /// <summary>
        /// 退出
        /// </summary>
        private void 权限ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion

        private void MainWindow_Load(object sender, EventArgs e)
        {
            label1.Location = new System.Drawing.Point(panel_AutoLoadingDetails.Location.X + (panel_AutoLoadingDetails.Width / 2) - label1.Width / 2, -1);
        }

        private void dataGridView_VOBCDeviceDetails_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.StartPosition = FormStartPosition.CenterParent;
            about.ShowDialog();
        }

        private void panel_AutoLoadingDetails_SizeChanged(object sender, EventArgs e)
        {
            //panel_AutoLoadingDetails

            label1.Location = new System.Drawing.Point(panel_AutoLoadingDetails.Location.X + (panel_AutoLoadingDetails.Width / 2) - label1.Width/2, -1);
        }

        private void dataGrid_VOBC_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {

            try
            {
                if (((bool)dataGrid_VOBC.Rows[e.RowIndex].Cells["Column_VOBCCC12"].EditedFormattedValue == true) &&
                    ((bool)dataGrid_VOBC.Rows[e.RowIndex].Cells["Column_VOBCCOM"].EditedFormattedValue == true) && 
                    ((bool)dataGrid_VOBC.Rows[e.RowIndex].Cells["Column_VOBCATP"].EditedFormattedValue == true) &&
                    ((bool)dataGrid_VOBC.Rows[e.RowIndex].Cells["Column_VOBCATO"].EditedFormattedValue == true) &&
                    ((bool)dataGrid_VOBC.Rows[e.RowIndex].Cells["Column_VOBCMMI"].EditedFormattedValue == true))
                {
                    dataGrid_VOBC.Rows[e.RowIndex].Cells["Column_VOBCCC12"].Value = false;
                    dataGrid_VOBC.Rows[e.RowIndex].Cells["Column_VOBCCOM"].Value = false;
                    dataGrid_VOBC.Rows[e.RowIndex].Cells["Column_VOBCATP"].Value = false;
                    dataGrid_VOBC.Rows[e.RowIndex].Cells["Column_VOBCATO"].Value = false;
                    dataGrid_VOBC.Rows[e.RowIndex].Cells["Column_VOBCMMI"].Value = false;
                }
                else
                {
                    dataGrid_VOBC.Rows[e.RowIndex].Cells["Column_VOBCCC12"].Value = true;
                    dataGrid_VOBC.Rows[e.RowIndex].Cells["Column_VOBCCOM"].Value = true;
                    dataGrid_VOBC.Rows[e.RowIndex].Cells["Column_VOBCATP"].Value = true;
                    dataGrid_VOBC.Rows[e.RowIndex].Cells["Column_VOBCATO"].Value = true;
                    dataGrid_VOBC.Rows[e.RowIndex].Cells["Column_VOBCMMI"].Value = true;
                }
            }
            catch
            {

            }

        }

        private void dataGrid_VOBC_DoubleClick(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in curruntDataGridView.SelectedRows)
            {
                //TODO:双击dataGrid_VOBC空白处，选中所有已选中行内的Cell
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            foreach (DataGridViewRow oneProduct in dataGrid_VOBC.SelectedRows)
            {
                //获取产品实例对象
                IProduct product = container[oneProduct.Index] as IProduct;
                if (product.CTcpClient == null)
                //if (product.ProductState != "正常")
                {
                    //发送建链信息帧
                    CommandQueue.instance.m_CommandQueue.Enqueue(new VOBCCommand(product.Ip, Convert.ToInt32(product.Port), product.ProductID, vobcCommandType.buildLink));
                }
            }
            timer2.Stop();
            timer2.Enabled = false;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            VOBCProduct product = e.Argument as VOBCProduct;
            product.SkipFlag = false;
            foreach(VOBCDevice device in product.CSelectedDevice)
            {
                device.RunDeploy(deployConfigCheck);
            }
        }

        private void dataGrid_VOBC_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
            if (e.RowIndex >= 0)
            {
                DataGridViewColumn column = dataGrid_VOBC.Columns[e.ColumnIndex];
                if (column is DataGridViewButtonColumn)
                {
                    selectedDevice.Clear();
                    VOBCProduct vobc = CDeviceDataFactory.Instance.VobcContainer[e.RowIndex] as VOBCProduct;
                    selectedDevice.AddRange(vobc.CSelectedDevice);
                    RefreshDataDetail();
                    tsbDeploy.Enabled = false;
                }
            }
        }

        private void timer3_Tick(object sender, EventArgs e)
        {

        }
    }

}
