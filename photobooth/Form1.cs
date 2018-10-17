using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using EOSDigital.API;
using EOSDigital.SDK;

namespace photobooth
{
    public partial class Form1 : Form
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
        #endregion

        public Form1()
        {

            try
            {
                CamList = new List<Camera>();
                InitializeComponent();
                //this.TopMost = true;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                this.KeyDown += Form_KeyDown;
                this.FormClosing += MainForm_FormClosing;
                this.WindowState = FormWindowState.Maximized;
                //this.BackgroundImage = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + @"\bg.png");
                APIHandler = new CanonAPI();
                APIHandler.CameraAdded += APIHandler_CameraAdded;
                ErrorHandler.SevereErrorHappened += ErrorHandler_SevereErrorHappened;
                ErrorHandler.NonSevereErrorHappened += ErrorHandler_NonSevereErrorHappened;
                LiveViewPicBox.Paint += LiveViewPicBox_Paint;
                LVBw = LiveViewPicBox.Width;
                LVBh = LiveViewPicBox.Height;
                RefreshCamera();
                IsInit = true;
            }
            catch (DllNotFoundException) { ReportError("Canon DLLs not found!", true); }
            catch (Exception ex) { ReportError(ex.Message, true); }
            try
            {
                if (IsInit)
                {
                    this.LiveViewPicBox.MouseDown += Form1_MouseDown;
                }
                for (int i = 0; i < 3; i++)
                {
                    PictureBox pictureBox = new PictureBox();
                    pictureBox.BackColor = Color.Black;
                    pictureBox.Width = 280;
                    pictureBox.Height = 210;
                    pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBox.MaximumSize = new System.Drawing.Size(280, 210);
                    pictureBoxes.Add(pictureBox);
                    flowLayoutPanel1.Controls.Add(pictureBox);
                }
            }
            catch (Exception)
            {
                throw;
            }
            watch();
        }
        private FileSystemWatcher watcher;
        private void watch()
        {
            watcher = new FileSystemWatcher();
            watcher.Path = strSavePathTextBox;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = "*.JPG";
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;
        }
        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (iPicBox > pictureBoxes.Count) iPicBox = 0;
            OnChangedPic(pictureBoxes[iPicBox], e.FullPath);
            iPicBox++;
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
                }
                catch (Exception)
                {
                    throw;
                }
                MainCamera?.Dispose();
                RefreshCamera();
            }
        }
        

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                MainCamera.TakePhoto();
                //MainCamera?.Dispose();
                //RefreshCamera();
            }
            catch (Exception)
            {

            }
        }

        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                IsInit = false;
                MainCamera?.Dispose();
                APIHandler?.Dispose();
            }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }


        #region API Events

        private void APIHandler_CameraAdded(CanonAPI sender)
        {
            try { Invoke((Action)delegate { RefreshCamera(); }); }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        private void MainCamera_StateChanged(Camera sender, StateEventID eventID, int parameter)
        {
            try { if (eventID == StateEventID.Shutdown && IsInit) { Invoke((Action)delegate { CloseSession(); }); } }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        int temp = 0;
        private void MainCamera_ProgressChanged(object sender, int progress)
        {
            try { Invoke((Action)delegate { temp = progress; }); }
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
                LiveViewPicBox.Invalidate();
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

        private void SessionButton_Click(object sender, EventArgs e)
        {
            if (MainCamera?.SessionOpen == true) CloseSession();
            else OpenSession();
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            try { RefreshCamera(); }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        #endregion

        #region Settings

        private void TakePhotoButton_Click(object sender, EventArgs e)
        {
            try
            {
                MainCamera.TakePhotoShutterAsync();
            }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }
    
        #endregion

        #region Live view
        
        private void LiveViewPicBox_SizeChanged(object sender, EventArgs e)
        {
            try
            {
                LVBw = LiveViewPicBox.Width;
                LVBh = LiveViewPicBox.Height;
                LiveViewPicBox.Invalidate();
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
                        e.Graphics.DrawImage(Evf_Bmp, 0, 0, w, h);
                    }
                }
            }
        }

        private void FocusNear3Button_Click(object sender, EventArgs e)
        {
            try { MainCamera.SendCommand(CameraCommand.DriveLensEvf, (int)DriveLens.Near3); }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        private void FocusNear2Button_Click(object sender, EventArgs e)
        {
            try { MainCamera.SendCommand(CameraCommand.DriveLensEvf, (int)DriveLens.Near2); }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        private void FocusNear1Button_Click(object sender, EventArgs e)
        {
            try { MainCamera.SendCommand(CameraCommand.DriveLensEvf, (int)DriveLens.Near1); }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        private void FocusFar1Button_Click(object sender, EventArgs e)
        {
            try { MainCamera.SendCommand(CameraCommand.DriveLensEvf, (int)DriveLens.Far1); }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        private void FocusFar2Button_Click(object sender, EventArgs e)
        {
            try { MainCamera.SendCommand(CameraCommand.DriveLensEvf, (int)DriveLens.Far2); }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        private void FocusFar3Button_Click(object sender, EventArgs e)
        {
            try { MainCamera.SendCommand(CameraCommand.DriveLensEvf, (int)DriveLens.Far3); }
            catch (Exception ex) { ReportError(ex.Message, false); }
        }

        #endregion

        #region Subroutines

        private void CloseSession()
        {
            MainCamera.CloseSession();
        }

        private void RefreshCamera()
        {
            CamList.Clear();
            CamList = APIHandler.GetCameraList();
            if (CamList.Count > 0)
            {
                OpenSession();
                MainCamera.SetSetting(PropertyID.SaveTo, (int)SaveTo.Both);
                MainCamera.SetCapacity(4096, int.MaxValue);
            }
            else
            {
                //refresh
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {

            LVBw = LiveViewPicBox.Width;
            LVBh = LiveViewPicBox.Height;
        }

        string strtempcamera = null;
        private void OpenSession()
        {
            if (CamList.Count > 0)
            {
                MainCamera = CamList[0];
                MainCamera.OpenSession();
                MainCamera.LiveViewUpdated += MainCamera_LiveViewUpdated;
                MainCamera.ProgressChanged += MainCamera_ProgressChanged;
                MainCamera.StateChanged += MainCamera_StateChanged;
                MainCamera.DownloadReady += MainCamera_DownloadReady;

                strtempcamera = MainCamera.DeviceName;
                AvList = MainCamera.GetSettingsList(PropertyID.Av);
                TvList = MainCamera.GetSettingsList(PropertyID.Tv);
                ISOList = MainCamera.GetSettingsList(PropertyID.ISO);

                if (!MainCamera.IsLiveViewOn) { MainCamera.StartLiveView(); }
                else { MainCamera.StopLiveView(); }
            }
        }

        private void ReportError(string message, bool lockdown)
        {
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

            }
        }

        #endregion        
    }
}
