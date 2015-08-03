using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Security.Principal;

namespace MapleLegend
{
    public partial class Form1 : Form
    {
        /* INFO
         * 
         * Written by LunchBox/Code @ MapleLegends
         * ------------------------
         * 
         * Code should be somewhat self-documenting, since nothing too serious is going on here.
         * Should not cause an autoban for those who are afraid of that.
         * If you have any questions, feel free to ask them in the thread.
         * Feel free to make improvements and change the code to your liking - I made it but public use.
         * 
         * /

        /* IMPORTS */
        //Move form
        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        //Embed
        [DllImport("USER32.DLL")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("USER32.dll")]
        private static extern bool MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool repaint);
        [DllImport("user32.dll", SetLastError = true)]
        static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, UInt32 dwNewLong);

        /* VARS */
        public const int    WM_NCLBUTTONDOWN = 0xA1;
        public const int    HT_CAPTION = 0x2;
        const int           GWL_STYLE = (-16);

        const UInt32        WS_POPUP = 0x80000000;
        const UInt32        WS_CHILD = 0x40000000;

        public bool         bEmbedded;
        public bool         bMenuOn = true;

        public Process      pMaple;
        public Size         sMapleSize = new Size(800, 600);

        public Form1()
        {
            InitializeComponent();
            settingsLoad();

            /*fix options panel*/
            panel_Options.Size = new Size(200, 195);

            /*load random gif*/
            Random rnd = new Random();
            object O = Properties.Resources._1;
            switch(rnd.Next(1, 6))
            {
                case 1:
                    O = Properties.Resources._1;
                    break;
                case 2:
                    O = Properties.Resources._2;
                    break;
                case 3:
                    O = Properties.Resources._3;
                    break;
                case 4:
                    O = Properties.Resources._4;
                    break;
                case 5:
                    O = Properties.Resources._5;
                    break;
            }
            pic_Slime.Image = (Image)O;
        }

        void settingsLoad()
        {
            /*processName*/
            if (Properties.Settings.Default.ProcessName.Length > 0)
                Options_txt_ProcessName.Text = Properties.Settings.Default.ProcessName;
        }

        void settingsSave()
        {
            /*processName*/
            Properties.Settings.Default.ProcessName = Options_txt_ProcessName.Text;

            /*save*/
            Properties.Settings.Default.Save();
        }

        Process getMapleProc()
        {
            Process[] proc = Process.GetProcessesByName(Options_txt_ProcessName.Text);
            if (proc.Length > 0)
            {
                return proc[0];
            }

            return null;
        }

        public static bool IsAdmin()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                    .IsInRole(WindowsBuiltInRole.Administrator);
        }

        void SetEmbed()
        {
            if (pMaple != null)
            {
                SetParent(pMaple.MainWindowHandle, this.panel_MapleStory.Handle);
                MoveWindow(pMaple.MainWindowHandle, 0, 0, this.Width, this.Height, true);

                uint style = GetWindowLong(pMaple.MainWindowHandle, GWL_STYLE);
                style = (style | WS_POPUP) & (~WS_CHILD);
                SetWindowLong(pMaple.MainWindowHandle, GWL_STYLE, style);
            }
        }

        void DetatchEmbed()
        {
            if (pMaple != null)
            {
                SetParent(pMaple.MainWindowHandle, IntPtr.Zero);
                pMaple = null;
                btn_Embed.Visible = true;
                bEmbedded = false;

                /*remove option*/
                panel_Options.Size = new Size(200, 195);
                Options_btn_Detatch.Visible = false;
                panel_Options.Visible = false;

                /*set bg and stuff*/
                panel_MapleStory.BackgroundImage = Properties.Resources.resource_BG;
                pic_Slime.Visible = true;
            }
        }

        void HideMenu(bool b)
        {
            if (b)
            {
                bMenuOn = false;
                panel_Opacity.Visible = false;
                panel_Options.Visible = false;
                panel_Menu.Size = new Size(20, 33);
            }
            else
            {
                bMenuOn = true;
                panel_Opacity.Visible = false;
                panel_Options.Visible = false;
                panel_Menu.Size = new Size(sMapleSize.Width, 33);
            }
        }

        void setSize()
        {
            try
            {
                string[] s = Options_cmb_Res.Text.Split('x'); //example 800x600
                sMapleSize = new Size(
                    Convert.ToInt32(s[0].Trim()),  //x
                    Convert.ToInt32(s[1].Trim())); //y
            }
            catch
            {
                MessageBox.Show("Bad resolution input.\nFormat example: 800x600");
                sMapleSize = new Size(800, 600);
                Options_cmb_Res.Text = "800x600";
            }

            this.Size = sMapleSize;
            panel_Menu.Width = sMapleSize.Width;
            panel_MapleStory.BackgroundImageLayout = ImageLayout.Stretch;
            SetEmbed();
        }

        private void Options_btn_Save_Click(object sender, EventArgs e)
        {
            settingsSave();
            setSize();
            panel_Options.Visible = false;
        }

        private void Options_btn_Detatch_Click(object sender, EventArgs e)
        {
            DetatchEmbed();
        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            if (bEmbedded)
            {
                DialogResult dialogResult = MessageBox.Show("Do you want to detatch the game before closing?", "Closeing...", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    DetatchEmbed();
                }
                else
                {
                    using (Process pM = getMapleProc())
                    {
                        pM.Kill();
                    }
                }
            }

            Application.Exit();
        }

        private void btn_Minimize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void btn_Embed_Click(object sender, EventArgs e)
        {
            if (IsAdmin())
            {
                Process pM = getMapleProc();
                if (pM != null)
                {
                    /* close video if playing */
                    if (web != null)
                    {
                        web_Close.Stop();
                        web.Dispose();
                        panel_MapleStory.Controls.Remove(web);
                    }

                    /* set new size */
                    this.Size = sMapleSize;
                    panel_Menu.Width = sMapleSize.Width;

                    /* set form name */
                    this.Text = Options_txt_ProcessName.Text;

                    /* remove images */
                    panel_MapleStory.BackgroundImage = null;
                    pic_Slime.Visible = false;

                    /* set variables */
                    pMaple = pM;
                    bEmbedded = true;
                    btn_Embed.Visible = false;

                    /* embed window */
                    SetEmbed();

                    /* enable detaching */
                    Options_btn_Detatch.Visible = true;
                    panel_Options.Size = new Size(200, 261);
                }
                else
                {
                    MessageBox.Show("Cant find '" + Options_txt_ProcessName.Text + ".exe'\n"
                    + "Make sure the process is running.", "Error");
                }
            }
            else
            {
                MessageBox.Show("This program needs to be run as Admin", "Error");
            }
        }

        private void btn_Opacity_Click(object sender, EventArgs e)
        {
            panel_Opacity.Visible = !panel_Opacity.Visible;
            panel_Options.Visible = false;
        }

        private void btn_Options_Click(object sender, EventArgs e)
        {
            panel_Options.Visible = !panel_Options.Visible;
            panel_Opacity.Visible = false;
        }

        private void panel_Menu_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void panel_Menu_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                SetEmbed();
            }
        }

        private void slider_Opacity_Scroll(object sender, EventArgs e)
        {
            this.Opacity = slider_Opacity.Value * 0.01;
        }

        private void btn_ToggleMenu_Click(object sender, EventArgs e)
        {
            if(bMenuOn)
            {
                HideMenu(true);
                btn_ToggleMenu.Text = "»";
            }
            else
            {
                HideMenu(false);
                btn_ToggleMenu.Text = "«";
            }
        }

        private void Options_btn_Save_MouseEnter(object sender, EventArgs e)
        {
            pic_Divider2.Visible = false;
        }

        private void Options_btn_Save_MouseLeave(object sender, EventArgs e)
        {
            pic_Divider2.Visible = true;
        }

        private void btn_Help_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
              "1. Run the game you want to embed in window mode\n"
            + "2. Run this program as Adminstrator and press 'Embed Game'\n\n"
            + "Change Opacity by pressing Opacity and move the slider\n"
            + "Toggle menu by pressing the [»] button top left\n"
            + "Detatch the game by going to Options > Detatch Game", "Help window");
        }

        WebBrowser web;
        private void pic_Slime_Click(object sender, EventArgs e)
        {
            /* play video :) */
            web = new WebBrowser();
            web.Dock = DockStyle.Fill;
            web.Navigate("https://www.youtube.com/v/EHHMkIbd2wI?autoplay=1");
            panel_MapleStory.Controls.Add(web);
            pic_Slime.Visible = false;
            web_Close.Start();
        }

        private void web_Close_Tick(object sender, EventArgs e)
        {
            web_Close.Stop();
            web.Dispose();
            panel_MapleStory.Controls.Remove(web);
        }
    }
}