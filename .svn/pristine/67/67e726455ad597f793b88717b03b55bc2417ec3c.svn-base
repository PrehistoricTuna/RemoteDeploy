using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;

namespace RemoteDeploy
{
    public delegate void ChangeButtonState(bool topmost);
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
            comboBox_UserType.Items.Add("VOBC");
            comboBox_UserType.Items.Add("ZC");
            dataGridView1.DataSource = SqliteHelper.ExecuteNonQuery("SELECT * FROM VOBCPassword", null);
            dataGridView2.DataSource = SqliteHelper.ExecuteNonQuery("SELECT * FROM ZCPassword", null);
        }
        public static string SQLQuery = "";
        public event ChangeButtonState ChangeState;
       /// <summary>
       /// 登录
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            String name = textBox_Name.Text;
            String password = textBox_Password.Text;
            String type = comboBox_UserType.Text;
            if (name.Equals(""))
            {
                MessageBox.Show("用户名不能为空");
                textBox_Name.Focus();
                return;
            }
            if (password.Equals(""))
            {
                MessageBox.Show("密码不能为空");
                textBox_Password.Focus();
                return;
            }
            if (type.Equals(""))
            {
                MessageBox.Show("用户类型不能为空");
                comboBox_UserType.Focus();
                return;
            }
            bool i = CheckPassword(name,password);

            if (i == true)
            {
                
                    this.Close();
                    MessageBox.Show("登陆成功");
                    ChangeState(true);
               
            }
            else
            {
                MessageBox.Show("登录失败");
            }
        }
        /// <summary>
        /// 判断密码是否正确
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool CheckPassword(string name, string password)
        {
            bool state = false;
            DataTable datatable = new DataTable();
            datatable = SqliteHelper.ExecuteNonQuery(SQLQuery, null);
            for (int i = 0; i < datatable.Rows.Count; i++)
            {
                if (name == datatable.Rows[i]["name"].ToString())
                {
                    if (password == datatable.Rows[i]["password"].ToString())
                    {
                        state = true;
                        break;
                    }
                }
            }

            return state;
        }
        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// 用户类型选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_UserType_SelectedIndexChanged(object sender, EventArgs e)
        {
           
            if (comboBox_UserType.Text == "VOBC")
            {
                SQLQuery = "SELECT * FROM VOBCPassword";
            }
            if (comboBox_UserType.Text == "ZC")
            {
                SQLQuery = "SELECT * FROM ZCPassword";
            }
        }

    }
}
