using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EOSDigital.API;
using EOSDigital.SDK;
using System.IO;

namespace pb2
{
    public partial class Main : Form
    {

        #region Variables

        CanonAPI APIHandler;
        Camera MainCamera;
        CameraValue[] AvList;
        CameraValue[] TvList;
        CameraValue[] ISOList;
        List<Camera> CamList;
        bool IsInit = false;
        Bitmap Evf_Bmp;
        int LVBw, LVBh, w, h;
        float LVBratio, LVration;

        int ErrCount;
        object ErrLock = new object();
        object LvLock = new object();

        string strSavePathTextBox = AppDomain.CurrentDomain.BaseDirectory + "\\photobooth";
        List<PictureBox> pictureBoxes = new List<PictureBox>();
        int iPicBox = 0;
        string strCounter = "";
        private FileSystemWatcher watcher;
        #endregion

        public Main()
        {
            InitializeComponent();
            watch();
            //this.TopMost = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackgroundImage = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + @"\bg.png");
            this.BackgroundImageLayout = ImageLayout.Zoom;
            this.Shown += Main_Shown;
        }

        private void Main_Shown(object sender, EventArgs e)
        {
            try
            {
                APIHandler = new CanonAPI();
                APIHandler.CameraAdded += APIHandler_CameraAdded;
                ErrorHandler.SevereErrorHappened += ErrorHandler_SevereErrorHappened;
                ErrorHandler.NonSevereErrorHappened += ErrorHandler_NonSevereErrorHappened;
                pictureBox.Paint += LiveViewPicBox_Paint;
                LVBw = pictureBox.Width;
                LVBh = pictureBox.Height;
                OpenSession();
                IsInit = true;
            }
            catch (DllNotFoundException) { ReportError("Canon DLLs not found!", true); }
            catch (Exception ex) { ReportError(ex.Message, true); }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {

            try
            {
                IsInit = false;
                MainCamera?.Dispose();
                APIHandler?.Dispose();
            }
            catch (Exception ex)
            {
                ReportError(ex.Message, false); 
            }
        }

        #region filewatcher
        private void watch()
        {
            watcher = new FileSystemWatcher();
            try
            {
                watcher.Path = strSavePathTextBox;
            }
            catch (Exception)
            {
                System.IO.Directory.CreateDirectory(strSavePathTextBox);
            }
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = "*.JPG";
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;
        }
        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            MainCamera.StartLiveView();
            //OnChangedPic(pictureBox, e.FullPath);
        }
        private delegate void dgOnChangedPic(PictureBox pictureBox, string path);
        private void OnChangedPic(PictureBox pictureBox, string path)
        {
            if (pictureBox.InvokeRequired)
            {
                pictureBox.BeginInvoke(new dgOnChangedPic(OnChangedPic), new object[] { pictureBox, path });
            }
            else
            {
                try
                {
                    pictureBox.Image = Image.FromFile(path);
                    pictureBox.Tag = path;
                }
                catch (Exception)
                {

                }
                
            }
        }
        #endregion

        #region API Events

        private void APIHandler_CameraAdded(CanonAPI sender)
        {
            try { Invoke((Action)delegate { OpenSession(); }); }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        private void MainCamera_StateChanged(Camera sender, StateEventID eventID, int parameter)
        {
            try { if (eventID == StateEventID.Shutdown && IsInit) { Invoke((Action)delegate { CloseSession(); }); } }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        private void MainCamera_ProgressChanged(object sender, int progress)
        {
            try { Invoke((Action)delegate { this.Text += progress; }); }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        private void MainCamera_LiveViewUpdated(Camera sender, Stream img)
        {
            try
            {
                lock (LvLock)
                {
                    Evf_Bmp?.Dispose();
                    Evf_Bmp = new Bitmap(img);
                }
                pictureBox.Invalidate();
            }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        private void MainCamera_DownloadReady(Camera sender, DownloadInfo Info)
        {
            try
            {
                string dir = null;
                Invoke((Action)delegate { dir = strSavePathTextBox; });
                sender.DownloadFile(Info, dir);
                Invoke((Action)delegate { this.Text += ""; });
            }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        private void ErrorHandler_NonSevereErrorHappened(object sender, ErrorCode ex)
        {
            ReportError($"SDK Error code: {ex} ({((int)ex).ToString("X")})", false);
        }

        private void ErrorHandler_SevereErrorHappened(object sender, Exception ex)
        {
            ReportError(ex.Message, true);
        }

        #endregion

        #region Session
        
        #endregion

        #region Settings

        private void TakePhotoButton_Click(object sender, EventArgs e)
        {

            if (pictureBox.Enabled)
            {
                new System.Threading.Thread(new System.Threading.ThreadStart(ThreadMakePicture)).Start();
            }
        }
        private void ThreadMakePicture()
        {

            try
            {
                strCounter = "5";
                System.Threading.Thread.Sleep(1000);
                strCounter = "4";
                System.Threading.Thread.Sleep(1000);
                strCounter = "3";
                pictureBox.Invalidate();
                System.Threading.Thread.Sleep(1000);
                strCounter = "2";
                System.Threading.Thread.Sleep(1000);
                strCounter = "1";
                System.Threading.Thread.Sleep(1000);
                MainCamera.StopLiveView();
                MainCamera.TakePhotoAsync();
                //MainCamera?.Dispose();
                //RefreshCamera();
            }
            catch (Exception)
            {

            }
            strCounter = "";
        }

        #endregion

        #region Live view

        private void LiveViewPicBox_SizeChanged(object sender, EventArgs e)
        {
            try
            {
                LVBw = pictureBox.Width;
                LVBh = pictureBox.Height;
                pictureBox.Invalidate();
            }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        private void LiveViewPicBox_Paint(object sender, PaintEventArgs e)
        {
            if (MainCamera == null || !MainCamera.SessionOpen) return;

            if (!MainCamera.IsLiveViewOn) e.Graphics.Clear(BackColor);
            else
            {
                lock (LvLock)
                {
                    if (Evf_Bmp != null)
                    {
                        LVBratio = LVBw / (float)LVBh;
                        LVration = Evf_Bmp.Width / (float)Evf_Bmp.Height;
                        if (LVBratio < LVration)
                        {
                            w = LVBw;
                            h = (int)(LVBw / LVration);
                        }
                        else
                        {
                            w = (int)(LVBh * LVration);
                            h = LVBh;
                        }
                        e.Graphics.DrawImage(Evf_Bmp, (pictureBox.Width-w)/2, 0, w, h);
                        e.Graphics.DrawString(strCounter, new Font("Arial", 130), Brushes.White, new System.Drawing.Point(2, 2));
                    }
                }
            }
        }
        
        #endregion

        #region Subroutines

        private void CloseSession()
        {
            MainCamera.CloseSession();
            this.Text = "No open session";
        }
        
        
        private void OpenSession()
        {
            CamList = APIHandler.GetCameraList();

            if (MainCamera?.SessionOpen == true)
                CamList.FindIndex(t => t.ID == MainCamera.ID); //TODO
            else if (CamList.Count > 0)
            {
                MainCamera = CamList[0];
                MainCamera.OpenSession();
                MainCamera.LiveViewUpdated += MainCamera_LiveViewUpdated;
                MainCamera.ProgressChanged += MainCamera_ProgressChanged;
                MainCamera.StateChanged += MainCamera_StateChanged;
                MainCamera.DownloadReady += MainCamera_DownloadReady;
                
                this.Text = MainCamera.DeviceName;
                MainCamera.SetSetting(PropertyID.SaveTo, (int)SaveTo.Both);
                MainCamera.SetCapacity(4096, int.MaxValue);
                pictureBox.Click += TakePhotoButton_Click;
                MainCamera.StartLiveView();
            }
        }

        private void ReportError(string message, bool lockdown)
        {
            if (message == "COMM_DISCONNECTED")
            {
                //TODO EDSDK killen.
                MainCamera.Dispose();
                Application.Restart();
                return;
            }
            int errc;
            lock (ErrLock) { errc = ++ErrCount; }

            if (lockdown) EnableUI(false);

            if (errc < 4) MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (errc == 4) MessageBox.Show("Many errors happened!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            lock (ErrLock) { ErrCount--; }
        }

        private void EnableUI(bool enable)
        {
            if (InvokeRequired) Invoke((Action)delegate { EnableUI(enable); });
            else
            {
                pictureBox.Enabled = enable;
            }
        }

        #endregion        
    }


    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }
    }
}
