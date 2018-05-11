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
   
    public partial class Login : Form
    {
        //数据库操作指令
        public static string SQLQuery = "";

   
        public enum UserType
        {
            none,
            manager,
            maintainer
        }
     


        public Login()
        {
            InitializeComponent();
            //用户类型设定
            comboBox_UserType.Items.Add("超级管理员");
            comboBox_UserType.Items.Add("维护员");
            comboBox_UserType.SelectedIndex = comboBox_UserType.Items.IndexOf("维护员");
           
        }
        

        public  UserType getusertype()
        {
           
            if (comboBox_UserType.Text == "超级管理员")
            {
                return UserType.manager;
            }
            if (comboBox_UserType.SelectedItem.ToString() == "维护员")
            {
                return UserType.maintainer;
            }
            return UserType.none;

        }
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

            MainWindow.username = textBox_Name.Text;


            //用户名为空
            if (name.Equals(""))
            {
                MessageBox.Show("用户名不能为空");
                textBox_Name.Focus();
                return;
            }
            //密码为空
            if (password.Equals(""))
            {
                MessageBox.Show("密码不能为空");
                textBox_Password.Focus();
                return;
            }
            //用户类型为空
            if (type.Equals(""))
            {
                MessageBox.Show("用户类型不能为空");
                comboBox_UserType.Focus();
                return;
            }
            bool i = CheckPassword(name,password);
            //登录成功
            if (i == true)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            //登录失败
            else
            {
                MessageBox.Show("登录失败");
            }
        }
        /// <summary>
        /// 判断密码是否正确
        /// </summary>
        /// <param name="name">用户名</param>
        /// <param name="password">密码</param>
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
           
            if (comboBox_UserType.Text == "维护员")
            {
                SQLQuery = "SELECT * FROM MaintainPsd";
            }
            if (comboBox_UserType.Text == "超级管理员")
            {
                SQLQuery = "SELECT * FROM ManagerPsd";
            }
            
        }

      

    }
}
