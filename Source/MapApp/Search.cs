using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MapApp
{
    public partial class Search : Form
    {
        public Search()
        {
            InitializeComponent();
        }
        //public List<string> listItem = null;
        public string SelectItem = string.Empty;
        public string SelectType = string.Empty;
        private void SelectHouse_Load(object sender, EventArgs e)
        {
            //if (listItem != null)
            //{
            //    foreach (string s in listItem)
            //    {
            //        comboBox1.Items.Add(s);
            //    }
            //}
        }

        private void btSelect_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cbType.Text))
            {
                cbType.Focus();
                this.DialogResult = DialogResult.OK;
                MessageBox.Show("请选择类型!");
                return;
            }

            if (string.IsNullOrEmpty(tbText.Text.Trim()))
            {
                tbText.Focus();
                MessageBox.Show("请输入要查询的类容!");
                return;
            }
            SelectType = cbType.Text;
            SelectItem = tbText.Text;
            this.DialogResult = DialogResult.OK;
                
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
