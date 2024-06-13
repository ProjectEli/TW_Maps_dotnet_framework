using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TW_Maps_dotnet_framework
{
    partial class TopModal
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        const int baseClientWidth = 350;
        const int baseClientHeight = 200;
        bool fixedLocation = false;
        bool disabledShortcut = false;
        private ContextMenuStrip contextMenuStrip = new ContextMenuStrip();

        // for always on top
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        // create scale list
        static List<double> scales = new List<double>() { 0.5, 0.75, 1.0, 1.5, 2.0 };
        static List<Keys> primaryShortcuts = new List<Keys>() { Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5 };
        static List<Keys> secondaryShortcuts = new List<Keys>() { Keys.NumPad1, Keys.NumPad2, Keys.NumPad3, Keys.NumPad4, Keys.NumPad5 };

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // TW_maps
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::TW_Maps_dotnet_framework.Properties.Resources.BlackholeMap_shortcut;
            this.BackgroundImageLayout = ImageLayout.Zoom;
            this.ClientSize = new Size((int)(baseClientWidth), (int)(baseClientHeight));
            this.ControlBox = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "TopModal";
            this.Text = "TW_Maps";
            this.TopMost = true;
            this.ResumeLayout(false);

            this.MouseDown += TopModal_MouseDown;
            this.KeyDown += TopModal_KeyDown;

            // Context menu
            this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(cms_Opening);
            this.ContextMenuStrip = contextMenuStrip;
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
        }


        private void cms_Opening(object sender, CancelEventArgs e)
        {
            Control c = contextMenuStrip.SourceControl as Control;
            contextMenuStrip.Items.Clear();
            if (c != null)
            {
                ToolStripMenuItem toolStripMenuItemFixLocation = new ToolStripMenuItem("위치 고정");
                toolStripMenuItemFixLocation.Click += (s, me) => { fixedLocation = !fixedLocation; };
                toolStripMenuItemFixLocation.Checked = fixedLocation;
                contextMenuStrip.Items.Add(toolStripMenuItemFixLocation);

                ToolStripMenuItem toolStripMenuItemDisabledShortcut = new ToolStripMenuItem("단축키 비활성화");
                toolStripMenuItemDisabledShortcut.Click += (s, me) => { disabledShortcut = !disabledShortcut; };
                toolStripMenuItemDisabledShortcut.Checked = disabledShortcut;
                contextMenuStrip.Items.Add(toolStripMenuItemDisabledShortcut);

                contextMenuStrip.Items.Add(new ToolStripSeparator());
                for (int k = 0; k < scales.Count; k++)
                {
                    double scale = scales[k];
                    string txt = "크기 " + ((int)(scale * 100)).ToString() + "%";
                    ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(txt);
                    toolStripMenuItem.ShortcutKeyDisplayString = primaryShortcuts[k].ToString().Replace("D", "") + " 또는 " + secondaryShortcuts[k].ToString();
                    toolStripMenuItem.Click += (s, me) =>
                    {
                        ClientSize = new Size((int)(baseClientWidth * scale), (int)(baseClientHeight * scale));
                    };
                    contextMenuStrip.Items.Add(toolStripMenuItem);
                }

                contextMenuStrip.Items.Add(new ToolStripSeparator());
                ToolStripMenuItem toolStripMenuItemClose = new ToolStripMenuItem("종료");
                toolStripMenuItemClose.Click += (s, me) => { Close(); };
                toolStripMenuItemClose.ShortcutKeyDisplayString = "Esc";
                contextMenuStrip.Items.Add(toolStripMenuItemClose);
            }
            e.Cancel = false;
        }

        // Snap function
        private const int SnapDist = 75;
        private bool DoSnap(int pos, int edge)
        {
            int delta = pos - edge;
            return delta > 0 && delta <= SnapDist;
        }
        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            Screen scn = Screen.FromPoint(this.Location);
            if (DoSnap(this.Left, scn.WorkingArea.Left)) this.Left = scn.WorkingArea.Left;
            if (DoSnap(this.Top, scn.WorkingArea.Top)) this.Top = scn.WorkingArea.Top;
            if (DoSnap(scn.WorkingArea.Right, this.Right)) this.Left = scn.WorkingArea.Right - this.Width;
            if (DoSnap(scn.WorkingArea.Bottom, this.Bottom)) this.Top = scn.WorkingArea.Bottom - this.Height;
        }

        private void TopModal_KeyDown(object sender, KeyEventArgs e)
        {
            double scale = 1.0;
            if (!disabledShortcut)
            {
                switch (e.KeyCode)
                {
                    case Keys.Escape:
                        Close();
                        break;
                    case Keys.D1:
                    case Keys.NumPad1:
                        scale = scales[0];
                        ClientSize = new Size((int)(baseClientWidth * scale), (int)(baseClientHeight * scale));
                        break;
                    case Keys.D2:
                    case Keys.NumPad2:
                        scale = scales[1];
                        ClientSize = new Size((int)(baseClientWidth * scale), (int)(baseClientHeight * scale));
                        break;
                    case Keys.D3:
                    case Keys.NumPad3:
                        scale = scales[2];
                        ClientSize = new Size((int)(baseClientWidth * scale), (int)(baseClientHeight * scale));
                        break;
                    case Keys.D4:
                    case Keys.NumPad4:
                        scale = scales[3];
                        ClientSize = new Size((int)(baseClientWidth * scale), (int)(baseClientHeight * scale));
                        break;
                    case Keys.D5:
                    case Keys.NumPad5:
                        scale = scales[4];
                        ClientSize = new Size((int)(baseClientWidth * scale), (int)(baseClientHeight * scale));
                        break;
                }
            }
        }


        // Draggable window
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        private void TopModal_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (!fixedLocation)
                    {
                        ReleaseCapture();
                        SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                    }
                    break;
            }
        }

        #endregion
    }
}

