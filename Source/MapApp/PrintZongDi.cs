using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace MapApp
{
    public partial class PrintZongDi : Form
    {
        public string 使用人 = string.Empty;
        public string 宗地编号 = string.Empty;
        public string 图号 = string.Empty;
        public string 使用面积 = string.Empty;
        public string 坐落 = string.Empty;
        public Bitmap bitmap = null;

        public PrintZongDi()
        {
            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            //this.textBox1.Text = "宗地编号：" + 图号;
            this.textBox1.Text = 使用人;
            this.textBox2.Text = 宗地编号;
            this.textBox3.Text = 图号;
            this.textBox4.Text = 使用面积;
            this.textBox5.Text = 坐落;
            try
            {
                string file = Path.Combine(Application.StartupPath, string.Format("images\\{0}.jpg", 图号));
                string nofile = Path.Combine(Application.StartupPath, @"images\no.jpg");
                if (File.Exists(file))
                {
                    pictureBox1.Image = Image.FromFile(file);
                }
                else
                {
                    pictureBox1.Image = Image.FromFile(nofile);
                }

            }
            catch
            {
                pictureBox1.Image = Image.FromFile(Path.Combine(Application.StartupPath, @"\images\no.jpg")); 
            }

        }
    }
}
