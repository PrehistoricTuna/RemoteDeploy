using System;
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

        //用于将用户名显示到界面
        public static string username;

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

        //是否执行了确认选择
        private bool selectConfirmed = false;

        //后台线程列表
        private Dictionary<BackgroundWorker, IProduct> wokerList = new Dictionary<BackgroundWorker, IProduct>();

        //心跳线程
        private Thread thread_heart = null;
        
        //enum类型的usertype
        private Login.UserType usertypestat;

        //后台工作线程计数
        private int workerCount;

        //用于识别当前dataGrid是否可编辑
        private bool windowEditable = true;

        //用于建链超时计时器检测需建链的VOBC
        List<IProduct> productLinkList = new List<IProduct>();

        #endregion

        #region 构造函数
        /// <summary>
        /// 窗体主函数
        /// </summary>
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
                LogManager.InfoLog.LogCommunicationError("MainWindow", "ProductReport", ex.ToString());
            }
        }

        /// <summary>
        /// 后台线程控件的执行完成通知事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerDeploy_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //开始第二阶段后已下发完成，禁用停止按钮
            tsbStop.Enabled = false;

            workerCount -= 1;
            if (workerCount == 0)
            {
                ResetState();
            }
        }
        /// <summary>
        /// 重置状态
        /// </summary>
        private void ResetState()
        {
            //当所有产品都不在部署状态后再重置
            if (container.FindAll(tar => (tar.InProcess == true)).Count == 0)
            { 
                //启用各类按钮
                button_OK.Enabled = true;
                linkEstab.Enabled = true;
                tsbStop.Enabled = true;

                //启用CheckBox
                checkBox_core.Enabled = true;
                checkBox_data.Enabled = true;

                //启用登陆相关
                登录ToolStripMenuItem.Enabled = true;
                注销ToolStripMenuItem.Enabled = true;

                windowEditable = true;

                for (int i = 0; i < curruntDataGridView.Rows.Count; i++)
                {
                    for (int j = 3; j < curruntDataGridView.Columns.Count; j++)
                    {
                        curruntDataGridView.Rows[i].Cells[j].ReadOnly = false;
                    }
                }                

                if (usertypestat == Login.UserType.manager)
                {
                    checkBox_bootloader.Enabled = true;
                    checkBox_ini.Enabled = true;
                    checkBox_nvram.Enabled = true;
                }
            }
        }

        /// <summary>
        /// 部署按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbDeploy_Click(object sender, EventArgs e)
        {
            //已经执行了确认选择
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
                        //TODO
                    }
                }
                ///判断子系统文件选择框选中状态
                if ((checkBox_core.Checked == false && checkBox_data.Checked == false && checkBox_ini.Checked == false && checkBox_nvram.Checked == false && checkBox_bootloader.Checked == false) || (deviceSelect == false))
                {
                    System.Windows.Forms.MessageBox.Show("请选择烧录文件或设备!");
                    return;
                }
                else
                {
                    //生成二次确认弹窗
                    Confirm confirForm = new Confirm
                    {
                        StartPosition = FormStartPosition.CenterParent
                    };

                    //弹窗并获取返回值用于传值
                    DialogResult dr = confirForm.ShowDialog();

                    //判定界面回传的用户操作  用户取消
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

                        //禁用建立链接
                        linkEstab.Enabled = false;

                        //禁用状态查看
                        tsbStateUpdate.Enabled = false;

                        //禁用CheckBox控件
                        checkBox_bootloader.Enabled = false;
                        checkBox_core.Enabled = false;
                        checkBox_data.Enabled = false;
                        checkBox_nvram.Enabled = false;
                        checkBox_ini.Enabled = false;

                        //禁用登陆相关
                        登录ToolStripMenuItem.Enabled = false;
                        注销ToolStripMenuItem.Enabled = false;

                        //后台线程执行部署操作
                        //backgroundWorkerDeploy.RunWorkerAsync();

                        //清空多线程集合
                        wokerList.Clear();
                        workerCount = 0;

                        for (int i = 0; i < curruntDataGridView.Rows.Count; i++)
                        {
                            for (int j = 0; j < curruntDataGridView.Columns.Count; j++)
                            {
                                curruntDataGridView.Rows[i].Cells[j].ReadOnly = true;
                            }
                        }                        

                        foreach (IProduct pro in container)
                        {
                            if (pro.CSelectedDevice.Count != 0)
                            {
                                //Modified @ 7.10
                                //(pro as VOBCProduct).VobcStateInfo = null;
                                //CommandQueue.instance.m_CommandQueue.Enqueue(new VOBCCommand(pro.Ip, Convert.ToInt32(pro.Port), pro.ProductID, vobcCommandType.vobcInfoRequest));

                                pro.InProcess = false;
                                pro.StepOne = true;
                                
                                windowEditable = false;
                                BackgroundWorker worker = new BackgroundWorker();
                                workerCount++;
                                worker.DoWork += Worker_DoWork;
                                worker.RunWorkerCompleted += backgroundWorkerDeploy_RunWorkerCompleted;
                                wokerList.Add(worker, pro);
                                //清空进度信息
                                //foreach (IDevice device in selectedDevice)
                                //{
                                //    int index = dataGridView_VOBCDeviceDetails.Rows.Add();
                                //    dataGridView_VOBCDeviceDetails.Rows[index].Cells["Column_Process"].Value = "";
                                //}
                            }
                        }
                        foreach (KeyValuePair<BackgroundWorker, IProduct> pair in wokerList)
                        {
                            pair.Key.RunWorkerAsync(pair.Value);
                        }
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
                //foreach (DataGridViewRow oneProduct in dataGrid_VOBC.SelectedRows)
                //Modified @ 9.10
                foreach (DataGridViewRow oneProduct in dataGrid_VOBC.Rows)
                {
                    IProduct product = container[oneProduct.Index];
                    //VOBC产品 默认将产品类中存储的VOBC状态信息重置
                    (product as VOBCProduct).VobcStateInfo = null;
                    if (product.ProductState == "正常")
                    {
                        CommandQueue.instance.m_CommandQueue.Enqueue(new VOBCCommand(product.Ip, Convert.ToInt32(product.Port), product.ProductID, vobcCommandType.vobcInfoRequest));
                    }                      
                    else
                    {
                        //Do nothing.
                    }
                    //else
                    //{
                    //    Thread.Sleep(1000);

                    //    //CommandQueue.instance.m_CommandQueue.Enqueue(new VOBCCommand(product.Ip, Convert.ToInt32(product.Port), product.ProductID, vobcCommandType.buildLink));

                    //    TimeSpan span = DateTime.Now - beginTime;
                    //    if (span.TotalMilliseconds > 5000)
                    //    {
                    //        break;
                    //    }

                    //}
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
                                    CDeviceDataFactory.Instance.ZcContainer.SetProductDeviceState(product.Ip,Convert.ToInt32(product.Port), deviceName, state);
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
        /// 断链按钮 单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbStop_Click(object sender, EventArgs e)
        {
            timer2.Enabled = false;
            timer2.Dispose();
            foreach (DataGridViewRow oneProduct in dataGrid_VOBC.Rows)
            {
                VOBCProduct product = container[oneProduct.Index] as VOBCProduct;
                //终止时清空VobcStateInfo
                product.VobcStateInfo = null;

                //重置显示状态 Modified @ 7.7
                CDeviceDataFactory.Instance.VobcContainer.SetProductDeviceState(product.Ip, Convert.ToInt32(product.Port),
                        CommonMethod.GetVobcSystemListByType(vobcSystemType.ALL), " ");

                //状态在必须中
                if (product.InProcess == true)
                {
                    //发送停止信息帧
                    CommandQueue.instance.m_CommandQueue.Enqueue(new VOBCCommand(product.Ip, Convert.ToInt32(product.Port), product.ProductID, vobcCommandType.stopUpdateFile));

                    //发送断链请求帧
                    CommandQueue.instance.m_CommandQueue.Enqueue(new VOBCCommand(product.Ip, Convert.ToInt32(product.Port), product.ProductID, vobcCommandType.cutLink));                    

                    //启用登陆相关
                    登录ToolStripMenuItem.Enabled = true;
                    注销ToolStripMenuItem.Enabled = true;

                    product.SkipFlag = true;
                    product.InProcess = false;
                    product.StepOne = true;
                    workerCount = 0;
                    windowEditable = true;

                    for (int i = 0; i < curruntDataGridView.Rows.Count; i++)
                    {
                        for (int j = 3; j < curruntDataGridView.Columns.Count; j++)
                        {
                            curruntDataGridView.Rows[i].Cells[j].ReadOnly = false;
                        }
                    }

                    product.ResetDeviceProcState();
                    CDeviceDataFactory.Instance.VobcContainer.SetProductState(product.Ip, Convert.ToInt32(product.Port), "用户终止");
                    CDeviceDataFactory.Instance.VobcContainer.dataModify.Color();                    
                }
                else
                {
                    if (product != null)
                    {
                        //发送断链请求帧
                        CommandQueue.instance.m_CommandQueue.Enqueue(new VOBCCommand(product.Ip, Convert.ToInt32(product.Port), product.ProductID, vobcCommandType.cutLink));
                        CDeviceDataFactory.Instance.VobcContainer.SetProductState(product.Ip, Convert.ToInt32(product.Port), "中断");                        
                    }
                }
                product.TimerClose();
                //product.Report.ReportWindow("用户终止" + product.ProductID + "部署过程");
            }

            //TODO:判定后台线程工作状态
            //if (backgroundWorkerDeploy.IsBusy == true)
            //{
            //    //取消工作
            //    backgroundWorkerDeploy.CancelAsync();
            //}
            //APPInit();
            //TODO:尚未确定方案,点击停止后应该发送停止更新请求还是终止FTP传输进程???如果是终止FTP部署则关联上面DoWork进程,如发送停止更新请求(已经在部署之后,CCOV更新或其他过程中),则不需要终止进程,只发送请求停止数据包即可(是否允许停止在数据分析中负责回显).
            
            button_OK.Enabled = true;

            VOBCProduct productone = container[0] as VOBCProduct;
            productone.Report.ReportWindow("部署进程已被用户终止，请根据使用指导重启或重启两次车载设备后再重新开始部署！");
        }

        ////timer计时器事件 执行心跳帧发送
        //private void timer1_Tick(object sender, EventArgs e)
        //{

        //    foreach (VOBCProduct product in CDeviceDataFactory.Instance.VobcContainer)
        //    {
        //        if (product.CTcpClient != null)
        //        {
        //            if (product.CTcpClient.clientSocket != null)
        //            {
        //                LogManager.InfoLog.LogCommunicationInfo("MainWindow", "timer1_Tick", "发送VOBC" + product.ProductID + "心跳帧");
        //                product.CTcpClient.Me_SendMessage(DataPack.DataPack.PackHeartbeatRequest());
        //            }
        //        }
        //    }

        //}

        /// <summary>
        /// 窗体关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            thread_heart.Abort();
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
            //清空列表全部VOBC状态
            foreach (DataGridViewRow oneProduct in dataGrid_VOBC.Rows)
            {
                IProduct product = container[oneProduct.Index];
                (product as VOBCProduct).VobcStateInfo = null;
            }   
        
            //遍历用户选择的需要建立链接的产品对象信息
            foreach (DataGridViewRow oneProduct in dataGrid_VOBC.SelectedRows)
            {               
                //获取产品实例对象
                IProduct product = container[oneProduct.Index] as IProduct;
                
                LogManager.InfoLog.LogProcInfo("MainWindow", "linkEstab_Click", "VOBC产品" + product.ProductID + "进入部署一阶段");
                
                //发送建链信息帧
                CommandQueue.instance.m_CommandQueue.Enqueue(new VOBCCommand(product.Ip, Convert.ToInt32(product.Port), product.ProductID, vobcCommandType.buildLink));
                CDeviceDataFactory.Instance.VobcContainer.SetProductFailReason(product.Ip, Convert.ToInt32(product.Port), "");
                //Modified @ 9.10
                if (!productLinkList.Exists((IProduct temp) => temp.ProductID == product.ProductID))
                {
                    productLinkList.Add(product);
                }
                
            }
            if (timer2.Enabled == false)
            {
                timer2.Enabled = true;
            }
            tsbStateUpdate.Enabled = true;
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
                tsbDeploy.Enabled = true;
            }
            else
            {
                selectConfirmed = false;
            }

            //刷新界面
            RefreshDataDetail();           
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
                            CDeviceDataFactory.Instance.ZcContainer.SetProductState(zc.Ip, Convert.ToInt32(zc.Port), "正常");
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
        /// 执行心跳发送方法
        /// </summary>
        public void HeartBeatSend()
        {
            while (true)
            {
                foreach (VOBCProduct product in CDeviceDataFactory.Instance.VobcContainer)
                {
                    if (product.CTcpClient != null)
                    {
                        if (product.CTcpClient.clientSocket != null)
                        {
                            LogManager.InfoLog.LogCommunicationInfo("MainWindow", "HeartBeatSend", "发送VOBC" + product.ProductID + "心跳帧");
                            product.CTcpClient.Me_SendMessage(DataPack.DataPack.PackHeartbeatRequest());
                        }
                    }
                }
                Thread.Sleep(5000);
            }
            
        }
        /// <summary>
        /// 初始化操作
        /// </summary>
        private void APPInit()
        {
            //禁用确认按钮
            button_OK.Enabled = false;

            //禁用建链按钮
            linkEstab.Enabled = false;

            //状态查看按钮
            tsbStateUpdate.Enabled = false;

            //禁用部署按钮
            tsbDeploy.Enabled = false;

            //停止按钮
            tsbStop.Enabled = false;

            //禁用引导文件勾选
            checkBox_bootloader.Enabled = false;

            //禁用配置文件勾选
            checkBox_ini.Enabled = false;

            //禁用nvram文件勾选
            checkBox_nvram.Enabled = false;

            //命令集合初始化
            CommandQueue.instance.m_CommandQueue.Enqueue(new InitCommand());

            ///初始化发送接收线程
            Send send = new Send();
            //Recv recv = new Recv();
            send.Init();
            //recv.Init();

            ///初始化线路数据
            CDeviceDataFactory.Instance.LoadXml(System.Windows.Forms.Application.StartupPath + "\\Config\\TopoConfig.xml");
            curruntDataGridView = dataGrid_VOBC;

            //DataGridViewButtonColumn check = new DataGridViewButtonColumn();
            //check.HeaderText = "查看";
            //check.Text = "查看";
            //check.Name = "details";
            //check.UseColumnTextForButtonValue = true;
            //check.Width = 50;
            //dataGrid_VOBC.Columns.Add(check);

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

            //初始化并开始心跳线程，执行心跳发送方法
            thread_heart = new Thread(new ThreadStart(HeartBeatSend));
            thread_heart.Start();
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
                        //更新成功才刷新为绿色
                        else if (product.ProductState == "更新成功")
                        {
                            row.DefaultCellStyle.BackColor = System.Drawing.Color.Green;
                        }
                        //如果是初始未开始的则不刷新颜色
                        else 
                        {
                            row.DefaultCellStyle.BackColor = System.Drawing.SystemColors.Window;
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
                    SqliteHelper.ExecuteNonQuery("INSERT INTO LogHistory VALUES('" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "','" + reprort + "')", null);
                }));
            }
            catch (Exception ex)
            {
                LogManager.InfoLog.LogCommunicationError("MainWindow", "ProductReport", ex.ToString());
            }
        }

        /// <summary>
        /// 刷新当前详细信息表格
        /// </summary>
        private void RefreshDataDetail()
        {
            //int currentLine = this.dataGridView_VOBCDeviceDetails.FirstDisplayedScrollingRowIndex;
            //清空数据
            //dataGridView_VOBCDeviceDetails.Rows.Clear();

            //VOBC使用车载ID  非VOBC 如ZC  CI  DSU等使用集中区

            dataGridView_VOBCDeviceDetails.Columns["Column_Area"].HeaderText = (curruntDataGridView == dataGrid_VOBC) ?
                "车载ID" : "集中区";
            //添加行部分
            List<IDevice> adddevices = new List<IDevice>();
            List<DataGridViewRow> removeHe = new List<DataGridViewRow>();
            foreach (IDevice device in selectedDevice)
            {
                bool isadd = true;
                foreach (DataGridViewRow row in this.dataGridView_VOBCDeviceDetails.Rows)
                {
                    if (row.Cells[0].Value.ToString() == device.BelongProduct.ProductID
                         && row.Cells[2].Value.ToString() == device.DeviceName)
                    {
                        isadd = false;
                    }
                }
                if (isadd)
                {
                    adddevices.Add(device);
                }
            }
            //移除行部分
            foreach (DataGridViewRow row in this.dataGridView_VOBCDeviceDetails.Rows)
            {
                bool isremove = true;
                foreach (IDevice device in selectedDevice) 
                {
                    if (row.Cells[0].Value.ToString() == device.BelongProduct.ProductID
                         && row.Cells[2].Value.ToString() == device.DeviceName)
                    {
                        isremove = false;
                    }
                }
                if (isremove)
                {
                    removeHe.Add(row);
                }
            }
            foreach (DataGridViewRow re_row in removeHe)
            {
                if (re_row.Index!=-1)
                dataGridView_VOBCDeviceDetails.Rows.Remove(re_row);
            }
            //其他
            //遍历当前已选中的实体
            int i = 0;
            foreach (IDevice device in selectedDevice)
            {
                //待添加表格行
                if (adddevices.Contains(device))
                {
                    DataGridViewRow _row = new DataGridViewRow();
                    _row.CreateCells(this.dataGridView_VOBCDeviceDetails);
                    //获取当前正在添加的数据行索引
                    //dataGridView_VOBCDeviceDetails.Rows.Add();
                    #region 添加表格数据
                    //依据数据行索引 并从数据实体中获取数据  显示到界面中
                    _row.Cells[0].Value = device.BelongProduct.ProductID;
                    _row.Cells[1].Value = device.DeviceType;
                    _row.Cells[2].Value = device.DeviceName;
                    _row.Cells[3].Value = device.State;
                    _row.Cells[4].Value = device.BelongProduct.Ip;
                    _row.Cells[5].Value = device.SoftVersion;
                    _row.Cells[6].Value = device.DataVersion;

                    _row.Cells[7].Value = device.PreCheckResult ? "预检成功" : "预检失败";
                    _row.Cells[8].Value = device.PreCheckFailReason;
                    _row.Cells[10].Value = device.ProcessState;

                    //依据状态 显示结果列的状态图标
                    switch (device.State)
                    {
                        case "更新成功":
                            _row.Cells[11].Value = RemoteDeploy.Properties.Resources.Green;
                            _row.DefaultCellStyle.ForeColor = System.Drawing.Color.Green;
                            break;
                        case "更新失败":
                            _row.Cells[11].Value = RemoteDeploy.Properties.Resources.Red;
                            _row.DefaultCellStyle.ForeColor = System.Drawing.Color.Red;
                            break;
                        case "文件上传中":
                        case "设备待重启":
                        case "更新执行中":
                        case "文件校验中":
                        case "下发完成":
                            _row.Cells[11].Value = RemoteDeploy.Properties.Resources.DarkOrange;
                            _row.DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
                            break;
                        default:
                            _row.Cells[11].Value = RemoteDeploy.Properties.Resources.Gray;
                            _row.DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
                            break;
                    }
                    dataGridView_VOBCDeviceDetails.Rows.Insert(i,_row);
                    #endregion
                }
                else
                {
                    foreach (DataGridViewRow row in this.dataGridView_VOBCDeviceDetails.Rows)
                    {
                        if (row.Cells[0].Value.ToString() == device.BelongProduct.ProductID
                             && row.Cells[2].Value.ToString() == device.DeviceName)
                        {
                            #region 更新表格数据
                            //依据数据行索引 并从数据实体中获取数据  显示到界面中
                            row.Cells[0].Value = device.BelongProduct.ProductID;
                            row.Cells[1].Value = device.DeviceType;
                            row.Cells[2].Value = device.DeviceName;
                            row.Cells[3].Value = device.State;
                            row.Cells[4].Value = device.BelongProduct.Ip;
                            row.Cells[5].Value = device.SoftVersion;
                            row.Cells[6].Value = device.DataVersion;
                            row.Cells[7].Value = device.PreCheckResult ? "预检成功" : "预检失败";
                            row.Cells[8].Value = device.PreCheckFailReason;
                            row.Cells[10].Value = device.ProcessState;
                            //依据状态 显示结果列的状态图标
                            switch (device.State)
                            {
                                case "更新成功":
                                    row.Cells[11].Value = RemoteDeploy.Properties.Resources.Green;
                                    row.DefaultCellStyle.ForeColor = System.Drawing.Color.Green;
                                    break;
                                case "更新失败":
                                    row.Cells[11].Value = RemoteDeploy.Properties.Resources.Red;
                                    row.DefaultCellStyle.ForeColor = System.Drawing.Color.Red;
                                    break;
                                case "文件上传中":
                                case "设备待重启":
                                case "更新执行中":
                                case "文件校验中":
                                case "下发完成":
                                    row.Cells[11].Value = RemoteDeploy.Properties.Resources.DarkOrange;
                                    row.DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
                                    break;
                                default:
                                    row.Cells[11].Value = RemoteDeploy.Properties.Resources.Gray;
                                    row.DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
                                    break;
                            }
                            #endregion
                        }
                    }

                }
                i++;
            }
            
           
            
            //清除控件选中状态
            //dataGridView_VOBCDeviceDetails.ClearSelection();
            //if ((dataGridView_VOBCDeviceDetails.Rows.Count > currentLine) && (currentLine>=0))
            //{
            //    this.dataGridView_VOBCDeviceDetails.FirstDisplayedScrollingRowIndex = currentLine;
            //}
        }

        /// <summary>
        /// 记录日志查询功能
        /// </summary>
        private void 历史查询ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //System.Diagnostics.Process.Start(System.Windows.Forms.Application.StartupPath + "\\log");
            LogHistory loghistory = new LogHistory(this.usertypestat);
            loghistory.StartPosition = FormStartPosition.CenterParent;
            loghistory.ShowDialog();
        }

        /// <summary>
        /// 登录窗口
        /// </summary>
        private void 登录ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Login login = new Login();
            login.StartPosition = FormStartPosition.CenterParent;
            //login.ChangeState += new ChangeButtonState(login_ChangeState);
            if (login.ShowDialog() == DialogResult.OK)
            {
                //启用建立连接按钮
                linkEstab.Enabled = true;

                //启用停止部署按钮
                tsbStop.Enabled = true;

                //启用确认选择按钮
                button_OK.Enabled = true;

                //显示已登录人员
                this.Text = "远程部署工具 - " + username + " 已登录";

                //usertypestat更改状态
                usertypestat = login.getusertype();

                //超级管理员可以勾选引导文件,配置文件，NVRam文件
                if (usertypestat == Login.UserType.manager)            
                {
                    checkBox_bootloader.Enabled = true;
                    checkBox_bootloader.Checked = false;
                }
                else
                {
                    checkBox_bootloader.Enabled = false;
                    checkBox_bootloader.Checked = false;
                }
            }
        }

        /// <summary>
        /// 注销
        /// </summary>
        private void 注销ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            //禁用建立连接按钮
            linkEstab.Enabled = false;

            //禁用状态查看按钮
            tsbStateUpdate.Enabled = false;

            //禁用部署按钮
            tsbDeploy.Enabled = false;

            //禁用停止按钮
            tsbStop.Enabled = false;

            //禁用确认选择按钮
            button_OK.Enabled = false;

            //禁用引导文件勾选
            checkBox_bootloader.Enabled = false;

            //禁用配置文件勾选
            checkBox_ini.Enabled = false;

            //禁用NVRam文件勾选
            checkBox_nvram.Enabled = false;

            this.Text = "远程部署工具 - 未登录";

            //改变用户类型为none
            usertypestat = Login.UserType.none;

        }

        /// <summary>
        /// 退出
        /// </summary>
        private void 权限ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion
        /// <summary>
        /// 主窗体加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Load(object sender, EventArgs e)
        {
            label1.Location = new System.Drawing.Point(panel_AutoLoadingDetails.Location.X + (panel_AutoLoadingDetails.Width / 2) - label1.Width / 2, -1);
        
        }

        /// <summary>
        /// “关于”菜单栏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.StartPosition = FormStartPosition.CenterParent;
            about.ShowDialog();
        }
        /// <summary>
        /// "自动部署详细信息"调整大小
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel_AutoLoadingDetails_SizeChanged(object sender, EventArgs e)
        {
            //panel_AutoLoadingDetails

            label1.Location = new System.Drawing.Point(panel_AutoLoadingDetails.Location.X + (panel_AutoLoadingDetails.Width / 2) - label1.Width/2, -1);
        }
        /// <summary>
        /// 双击选中整行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGrid_VOBC_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {

            try
            {
                if (windowEditable)
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
                else
                {
                    //什么都不做
                }
            }
            catch
            {

            }

        }

        /// <summary>
        /// 判断是否建链超时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer2_Tick(object sender, EventArgs e)
        {
            //遍历正在进行建链的产品实例 判定是否建链超时
            foreach (IProduct product in productLinkList)
            { 
                //产品烧录状态（非正常&&非用户终止） 即判定为超时
                if (product.ProductState != "正常" && product.ProductState != "待重启")
                {
                    product.ProductState = "中断";
                    //通知界面刷新
                    CDeviceDataFactory.Instance.VobcContainer.dataModify.Modify();
                    product.Report.ReportWindow("VOBC产品" + product.ProductID + "建链超时，请重新尝试建链！");
                    //LogManager.InfoLog.LogProcInfo("MainWindow", "linkEstab_Click", "VOBC产品" + product.ProductID + "建链超时！");
                    if (product.CTcpClient != null)
                    {
                        product.CTcpClient.Socket_TCPClient_Dispose();
                    }
                }
            }

            //处理完毕后  清空列表
            productLinkList.Clear();

            //禁用timer 待下次点击‘建链’时在启用该timer
            
            timer2.Enabled = false;
            timer2.Dispose();
        }

        /// <summary>
        /// 执行后台线程工作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            //转换VOBC产品类对象
            VOBCProduct product = e.Argument as VOBCProduct;

            //跳过标志默认为false
            product.SkipFlag = false;

            //初始化各设备更新文件成功计数，修复缺陷，Modified @ 8.29
            foreach (VOBCDevice device in product.CBelongsDevice)
            {
                device.UpdateSuccessFileCount = 0;
            }
            //开始执行部署
            foreach (VOBCDevice device in product.CSelectedDevice)
            {
                device.RunDeploy(deployConfigCheck);
            }
        }


        private void 打开日志文件所在目录ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(System.Windows.Forms.Application.StartupPath + "\\log");
        }
      

        private void buttonCheck_Click(object sender, EventArgs e)
        {
            List<IDevice> tempSelectedDevice = selectedDevice;
            selectedDevice.Clear();
            foreach (DataGridViewRow oneProduct in dataGrid_VOBC.SelectedRows)
            {
                IProduct product = container[oneProduct.Index] as IProduct;
                selectedDevice.AddRange(product.CSelectedDevice);
                RefreshDataDetail();                
            }
        }

    }

}
