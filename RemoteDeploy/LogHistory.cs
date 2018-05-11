using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RemoteDeploy
{
    public partial class LogHistory : Form
    {
        

        public String timeStart;
        public String timeEnd;

        public Login.UserType usertypestat;

        /// <summary>
        /// 历史日志查询类
        /// </summary>
        public LogHistory(Login.UserType usertype)
        {
            InitializeComponent();
            SetMyCustomFormat();
            usertypestat = usertype;
        //comboBox_date.Items.Add.Menu;
        }

        //将dateTimePicker的输入框格式调整成数据库匹配的格式
        public void SetMyCustomFormat()
        {
            // Set the Format type and the CustomFormat string.
            dateTimePicker_startTime.Format = DateTimePickerFormat.Custom;
            dateTimePicker_startTime.CustomFormat = "yyyy/MM/dd HH:mm:ss";
            dateTimePicker_endTime.Format = DateTimePickerFormat.Custom;
            dateTimePicker_endTime.CustomFormat = "yyyy/MM/dd HH:mm:ss";

        }

        /// <summary>
        /// 按钮点击操作
        /// </summary>
        /// <param name="sender">发送</param>
        /// <param name="e">事件</param>
        private void button1_Click(object sender, EventArgs e)
        {
            String start = dateTimePicker_startTime.Text;
            String end = dateTimePicker_endTime.Text;

            //清空richTextBox1里面的内容
            richTextBox1.Clear();

            //边界条件：开始时间晚于结束时间
            if (string.Compare(start, end) > 0)
            {
                MessageBox.Show("开始时间不应晚于结束时间！");
                return;
            }

            DataTable dt = SqliteHelper.ExecuteNonQuery("SELECT * FROM LogHistory WHERE TIME >= '" + start + "' AND TIME <= '" + end + "';", null);

            //边界条件：查询结果为空
            if (dt.Rows.Count == 0)
            {
                MessageBox.Show("查询结果为空！", "提示");
                return;
            }

            //逐行输出到richTextBox1中
            foreach (DataRow row in dt.Rows)
            {
                richTextBox1.AppendText(row.ItemArray[1].ToString() + "\n");
            }
        }

        private void button_Clear_Click(object sender, EventArgs e)
        {
            MessageBoxButtons messForClear = MessageBoxButtons.OKCancel;
            DataTable dtDelete = SqliteHelper.ExecuteNonQuery("SELECT * FROM LogHistory;", null);

            if(0 == dtDelete.Rows.Count)
            {
                MessageBox.Show("日志历史记录已空，无需清理!","提示");
                return;
            }
            String messClear = "确定要清空吗？\n警告：清空将清除所有日志记录！";
            DialogResult choice = MessageBox.Show(messClear,"警告",messForClear);
            if(choice == DialogResult.OK)
            {
                
                try
                {
                    richTextBox1.Clear();
                    dtDelete = SqliteHelper.ExecuteNonQuery("DELETE FROM LogHistory;", null);
                    if (0 == dtDelete.Rows.Count)
                    {
                        MessageBox.Show("历史日志已清空", "成功");
                    }
                }
                catch(InvalidCastException eclear)
                {
                    throw (eclear);
                }
            }
            else
            {
                return;
            }
        }

        private void LogHistory_Load(object sender, EventArgs e)
        {
            if (Login.UserType.manager == usertypestat)
            {
                button_Clear.Visible = true;
            }
        }
    }
}
