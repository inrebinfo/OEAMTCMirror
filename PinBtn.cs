﻿using OEAMTCMirror;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OEAMTCMirror
{
    public partial class PinBtn : Form
    {

        private LogWriter _log;
        private OriginalForm _mainForm;
        MirrorState _mirrorState;
        private bool _contextOpened = false;


        public PinBtn(LogWriter log, Form mainfrm, MirrorState stateObj)
        {
            InitializeComponent();

            this._log = log;
            this._mainForm = mainfrm as OriginalForm;
            this._mirrorState = stateObj;
            this.ShowInTaskbar = false;

            PositionButton();

            timer1.Start();

            btnStartMirror.BackgroundImage = Properties.Resources.btn_bg;

            this.ClientSize = new Size(26, 26);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // turn on WS_EX_TOOLWINDOW style bit
                cp.ExStyle |= 0x80;
                return cp;
            }
        }

        public void PositionButton()
        {
            try
            {
                uint processID;
                IntPtr foregroundWnd = User32.GetForegroundWindow();
                User32.GetWindowThreadProcessId(foregroundWnd, out processID);
                Process prc = Process.GetProcessById((int)processID);

                if (prc.ProcessName != Process.GetCurrentProcess().ProcessName && prc.MainWindowTitle != "Shell_Traywnd" && !_mainForm._excludedWindows.Contains(prc.ProcessName))
                {
                    btnStartMirror.Show();
                    IntPtr ptr = prc.MainWindowHandle;

                    User32.Rect rectangleWindow = new User32.Rect();
                    User32.GetWindowRect(ptr, ref rectangleWindow);

                    this.Top = rectangleWindow.top + 5;
                    this.Left = rectangleWindow.right - 170;
                    this.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void MirrorOnBtnClick()
        {
            try
            {
                if (!_mirrorState.Active)
                {
                    _mainForm.WindowState = FormWindowState.Minimized;

                    this.Hide();

                    Thread.Sleep(500);

                    uint processID;
                    IntPtr foregroundWnd = User32.GetForegroundWindow();
                    User32.GetWindowThreadProcessId(foregroundWnd, out processID);
                    Process prc = Process.GetProcessById((int)processID);

                    _mirrorState.SelectedProcess = prc;

                    PositionButton();


                    _mainForm.StartMirroring();
                    this.Show();

                    User32.SetForegroundWindow(prc.MainWindowHandle);
                }
                else
                {
                    _mainForm.StopMirroring();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void CheckSysTrayOpen()
        {
            try
            {
                uint processID;
                IntPtr foregroundWnd = User32.GetForegroundWindow();
                User32.GetWindowThreadProcessId(foregroundWnd, out processID);
                Process prc = Process.GetProcessById((int)processID);

                if (prc.MainWindowTitle == "Shell_Traywnd")
                {
                    this.Hide();
                }
            }
            catch (Exception ex)
            { }
        }

        #region Buttons, Hover, etc
        private void timer1_Tick(object sender, EventArgs e)
        {
            PositionButton();
            CheckSysTrayOpen();
            if (_mirrorState.Active)
            {
                btnStartMirror.BackgroundImage = Properties.Resources.btn_bg_active;
                stopMirrorToolStripMenuItem.Enabled = true;
            }
            else
            {
                btnStartMirror.BackgroundImage = Properties.Resources.btn_bg;
                stopMirrorToolStripMenuItem.Enabled = false;
            }
        }

        private void btnStartMirror_MouseDown(object sender, MouseEventArgs e)
        {
            timer1.Stop();
            _mainForm.timer1.Stop();

            if (e.Button == MouseButtons.Left)
            {
                MirrorOnBtnClick();
            }
            if (e.Button == MouseButtons.Right)
            {
                _mainForm.timer1.Stop();
                Button btnSender = (Button)sender;
                Point ptLowerLeft = new Point(0, btnSender.Height);
                ptLowerLeft = btnSender.PointToScreen(ptLowerLeft);
                ptLowerLeft.X -= 5;
                ptLowerLeft.Y -= 5;
                contextMenuStrip1.Show(ptLowerLeft);
                this._contextOpened = true;
            }
        }

        private void btnStartMirror_MouseUp(object sender, MouseEventArgs e)
        {
            timer1.Start();
            _mainForm.timer1.Interval = 5;
            _mainForm.timer1.Start();
        }
        private void stopMirrorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MirrorOnBtnClick();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (_mirrorState.Active)
            {
                _mainForm.timer1.Stop();
            }
        }

        private void contextMenuStrip1_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            _mainForm.timer1.Start();
        }
        #endregion
    }
}
