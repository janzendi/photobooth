using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pb2
{
    public partial class FormBar : Form
    {
        System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(NewForm));
        static Main formMain;
        public FormBar()
        {
            InitializeComponent();
            this.TopMost = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;
            this.btnExit.BackgroundImageLayout = ImageLayout.Zoom;
            this.btnExit.BackgroundImage = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + @"\exit.png");
            this.btnReload.BackgroundImageLayout = ImageLayout.Zoom;
            this.btnReload.BackgroundImage = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + @"\reload.png");
            this.DesktopLocation = new Point(0, 0);
            this.StartPosition = FormStartPosition.Manual;
            this.Left = 0;
            this.Top = 0;

        }
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(uint dwDesiredAccess, bool bInheritHandle, uint dwThreadId);


        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern bool TerminateThread(IntPtr hThread, uint dwExitCode);

        private void btnExit_Click(object sender, EventArgs e)
        {
            if (thread!=null && thread.ThreadState == System.Threading.ThreadState.Running)
            {
                if (false)
                {
                    foreach (System.Diagnostics.ProcessThread pt in System.Diagnostics.Process.GetCurrentProcess().Threads)
                    {
                        IntPtr ptrThread = OpenThread(1, false, (uint)pt.Id);
                        if (thread.ManagedThreadId != pt.Id)
                        {
                            try
                            {
                                TerminateThread(ptrThread, 1);
                            }
                            catch (Exception ex)
                            {
                                Console.Out.WriteLine(ex.ToString());
                            }
                        }
                    }
                }
                thread.Abort();
            }
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            if (thread.ThreadState == System.Threading.ThreadState.Aborted)
            {
                thread.Abort();
            }
            thread = new System.Threading.Thread(new System.Threading.ThreadStart(NewForm));
            thread.Start();
        }

        private void FormBar_Shown(object sender, EventArgs e)
        {
            if (thread.ThreadState == System.Threading.ThreadState.Unstarted)
            {
                thread.Start();
            }
        }

        private static void NewForm()
        {
            try
            {

                formMain = new Main();
                formMain.ShowDialog();
            }
            catch (System.Threading.ThreadInterruptedException)
            {
                formMain.Dispose();
            }
        }
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
            Application.Run(new FormBar());
        }
    }
}
