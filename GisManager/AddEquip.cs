using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GisManager
{
    public partial class AddEquip : Form
    {
        public string Name;
        public AddEquip()
        {
            InitializeComponent();
        }


        private void btOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbName.Text.Trim()))
            {
                MessageBox.Show("名称不能为空！");
                tbName.Focus();
            }
            else
            {
                Name = tbName.Text;
                this.DialogResult = DialogResult.OK;
            }
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}