using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace photobooth
{
    public partial class Form_PreviewPic : Form
    {
        private int icounter = 8;
        private string strcounter = "8";
        public Form_PreviewPic(Image image)
        {
            InitializeComponent();
            this.pictureBox1.Image = image;
            this.pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            this.pictureBox1.Paint += PictureBox1_Paint;
            this.TopMost = true;
            this.WindowState = FormWindowState.Maximized;
            timer1.Interval = 1000;
            timer1.Enabled = true;
        }

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawString("Schließt in "+strcounter+" Sekunden", new Font("Arial", 20), Brushes.White, new System.Drawing.Point(200, 2));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            strcounter = icounter.ToString();
            if (--icounter == 0)
            {
                this.Dispose();
                //this.Close();
            }
        }
    }
}
