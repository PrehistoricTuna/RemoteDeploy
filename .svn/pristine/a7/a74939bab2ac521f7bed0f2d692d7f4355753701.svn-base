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
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }
        //public static string str;//public类型的实例字段
        private void button1_Click(object sender, EventArgs e)
        {
            String name = textBox_Name.Text;
            String password = textBox_Password.Text;
            if (name.Equals("admin") && password.Equals("123456"))
            {
                this.Hide();
                MessageBox.Show("登陆成功");
            }
            else
            {
                MessageBox.Show("登录失败");
            }
        }
    }
}
