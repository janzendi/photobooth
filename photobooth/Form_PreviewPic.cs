using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Printing;

namespace photobooth
{
    public partial class Form_PreviewPic : Form
    {
        private int icounter = 10;
        private Image imagefile;
        public Form_PreviewPic(Image image, string picpath)
        {
            InitializeComponent();
            this.pictureBox1.Image = image;
            this.pictureBox1.Tag = picpath;
            this.pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            this.pictureBox1.Paint += PictureBox1_Paint;
            //this.TopMost = true;
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            timer1.Interval = 1000;
            timer1.Enabled = true;
            button1.BackgroundImage = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + @"\if_print_172530.png");
            button1.BackgroundImageLayout = ImageLayout.Zoom;
            imagefile = Image.FromFile(pictureBox1.Tag.ToString());
            //button1.Enabled = false;
            this.BackgroundImage = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + @"\74801-amazing-gold-glitzer-hintergrundbilder-1920x1080.jpg");
            this.BackgroundImageLayout = ImageLayout.Zoom;
        }
        
        

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawString("Schließt in "+icounter.ToString()+" Sekunden", new Font("Arial", 20), Brushes.White, new System.Drawing.Point(200, 2));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (--icounter == 0)
            {
                this.Dispose();
                //this.Close();
            }
            pictureBox1.Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            PrintDocument printDocument = new PrintDocument();
            printDocument.PrintPage += PrintDocument_PrintPage;
            printDocument.PrinterSettings.PrinterName = "Brother MFC-J5910DW Printer";
            printDocument.Print();
            button1.Enabled = false;
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            //Point ulCorner = new Point(100, 100);
            //e.Graphics.DrawImage(Image.FromFile(pictureBox1.Tag.ToString()), ulCorner);


            e.Graphics.DrawImage(imagefile, e.PageSettings.PrintableArea.X - e.PageSettings.HardMarginX, e.PageSettings.PrintableArea.Y - e.PageSettings.HardMarginY, e.PageSettings.Landscape ? e.PageSettings.PrintableArea.Height : e.PageSettings.PrintableArea.Width, e.PageSettings.Landscape ? e.PageSettings.PrintableArea.Width : e.PageSettings.PrintableArea.Height);


        }
        /*
public void Print(string path)
{
PrintDialog SelectedPrinter = new PrintDialog();
if (SelectedPrinter.ShowDialog() == true)
{
PrintCapabilities printerCapabilities = SelectedPrinter.PrintQueue.GetPrintCapabilities();
Size PageSize = new Size(printerCapabilities.PageImageableArea.ExtentWidth, printerCapabilities.PageImageableArea.ExtentHeight);
Size PrintableImageSize = new Size();
  DrawingVisual drawVisual = new DrawingVisual();
  ImageBrush imageBrush = new ImageBrush();
  imageBrush.ImageSource = new BitmapImage(path);
  imageBrush.Stretch = Stretch.Fill;
  imageBrush.TileMode = TileMode.None;
  imageBrush.AlignmentX = AlignmentX.Center;
  imageBrush.AlignmentY = AlignmentY.Center;
  if (imageBrush.ImageSource.Width > imageBrush.ImageSource.Height)
      PrintableImageSize = new Size(768, 576); //8x6
  else PrintableImageSize = new Size(576, 768); //6x8 
  double xcor = 0; double ycor = 0;
  if (imageBrush.ImageSource.Width > imageBrush.ImageSource.Height)
  {
      if ((PageSize.Width - PrintableImageSize.Height) > 0)
          xcor = (PageSize.Width - PrintableImageSize.Height) / 2;
      if ((PageSize.Height - PrintableImageSize.Width) > 0)
          ycor = (PageSize.Height - PrintableImageSize.Width) / 2;
  }
  else
  {
      if ((PageSize.Width - PrintableImageSize.Width) > 0)
          xcor = (PageSize.Width - PrintableImageSize.Width) / 2;
      if ((PageSize.Height - PrintableImageSize.Height) > 0)
          ycor = (PageSize.Height - PrintableImageSize.Height) / 2;
  }
  using (DrawingContext drawingContext = drawVisual.RenderOpen())
  {
      if (imageBrush.ImageSource.Width > imageBrush.ImageSource.Height)
      {
          drawingContext.PushTransform(new RotateTransform(90, PrintableImageSize.Width / 2, PrintableImageSize.Height / 2));
      }
      drawingContext.DrawRectangle(imageBrush, null, new Rect(xcor, ycor, PrintableImageSize.Width, PrintableImageSize.Height));
  }
  SelectedPrinter.PrintVisual(drawVisual, Print);

}
}
*/

    }
}
