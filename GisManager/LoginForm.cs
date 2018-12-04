using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GisManager
{
    public partial class LoginForm : Form
    {
        public static int UserType;
        public LoginForm()
        {
            InitializeComponent();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            Application.ExitThread();
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(tbUserName.Text.Trim()) || String.IsNullOrEmpty(tbPassWD.Text.Trim()))
            {
                MessageBox.Show("用户密码不能为空");
            }
            else
            {
                string sql = String.Format("select UserType from SysUser where UserName='{0}' and PassWd='{1}'", tbUserName.Text, tbPassWD.Text);
                DataTable dt = DBHelper.Instance.GetDataTable(sql);
                if (dt == null || dt.Rows.Count <= 0)
                {
                    MessageBox.Show("用户名或密码错误！");
                }
                else
                {
                    UserType = Convert.ToInt32(dt.Rows[0][0]);
                    this.Hide();
                    MainForm frmMain = new MainForm();
                    frmMain.ShowDialog();
                    Application.ExitThread();
                }
            }
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            tbUserName.Focus();
        }
    }
}