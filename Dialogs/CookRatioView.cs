﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CookImplement;

namespace MabiCooker2
{
    public partial class CookRatioView : Form
    {
        private const int STUFF_FLOW_SIZE = 180;

        public bool NoticeInfo = true;
        public static Cook SelectedCook;
        public static int DataIndex;

        private bool RightLeft = true;
        private Point mousePoint;
        private ListBox FavListView;
        private MabiCooker MainWindow;
        private ToolTip tooltip = new ToolTip();

        private int initial_height;
        private int bar_width = 243; // 232 -> 243 UI Patch

        public CookRatioView(MabiCooker Main, ListBox FavView)
        {
            InitializeComponent();

            if (Properties.Settings.Default.LastRatioPoint.X != 0 && Properties.Settings.Default.LastRatioPoint.Y != 0)
            {
                this.StartPosition = FormStartPosition.Manual;
                this.Location = new Point(Properties.Settings.Default.LastRatioPoint.X, Properties.Settings.Default.LastRatioPoint.Y);
            }

            this.FavListView = FavView;
            this.MainWindow = Main;
            tooltip.ShowAlways = true;
            initial_height = this.Size.Height;
            tooltip.SetToolTip(lCloseRatio, Properties.Resources.StrClose);
        }

        public void UpdateData()
        {
            if (SelectedCook == null)
            {
                return;
            }
            int[] iBuffer;
            int RankId = Cook.checkRank(SelectedCook.getRank());
            Type def = typeof(Cook.RankName);
            // lStuffs.Location = new Point(lName.Location.X + lName.Width + 5, 15);

            lStuffs.Text = SelectedCook.getStuffForString();
            lRankName.Text = Enum.GetName(def, Cook.checkRank(SelectedCook.getRank()));
            lRankName.Text = Rank.checkRank(RankId).ToString();
            lRank.Text = Rank.checkRank(RankId).ToString();
            lName.Text = SelectedCook.getName();
            lName.Location = new Point(lRankName.Location.X + lRankName.Width, lName.Location.Y);
            lRank.Text = SelectedCook.getRank().ToString();
            iBuffer = SelectedCook.getRatio();
            if (iBuffer[0] == bar_width)
            {
                lName.Text += Properties.Resources.StrCannotCook;
            }
            
            #region bar
            String[] StuffName = SelectedCook.getStuff();

            // Recalculate pixel information with relative position
            bar_width = Width;

            double[] fBuffer = new double[3];
            int[] iBufferRemap = new int[3];

            for (int i = 0; i < 3; i++)
            {
                // float
                fBuffer[i] = (float)(iBuffer[i] / 100.0) * bar_width;

                // pixel
                iBufferRemap[i] = (int)Math.Round((float)(iBuffer[i] / 100.0) * bar_width);
            }

            pbStuffOne.Width = iBufferRemap[0];
            tooltip.SetToolTip(pbStuffOne, String.Format("{0} ({1})", StuffName[0], percentage(iBuffer[0], 100)));
            pbStuffTwo.Location = new Point(iBufferRemap[0], 0);

            try
            {
                pbStuffTwo.Width = iBufferRemap[1];
                tooltip.SetToolTip(pbStuffTwo, String.Format("{0} ({1})", StuffName[1], percentage(iBuffer[1], 100)));
                pbStuffThree.Location = new Point((int)Math.Round(fBuffer[0] + fBuffer[1]), 0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                pbStuffTwo.Width = 0;
                tooltip.SetToolTip(pbStuffTwo, "");
            }
            
            try
            {
                // potentional 1 pixel error
                if ((iBufferRemap[0] + iBufferRemap[1] + iBufferRemap[2]) < bar_width)
                {
                    pbStuffThree.Location = new Point(pbStuffThree.Location.X - 1, 0);
                    iBufferRemap[2]++;
                }

                pbStuffThree.Width = iBufferRemap[2];
                tooltip.SetToolTip(pbStuffThree, String.Format("{0} ({1})", StuffName[2], percentage(iBuffer[2], 100)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                pbStuffThree.Width = 0;
                tooltip.SetToolTip(pbStuffThree, "");
            }
            #endregion

            lFavCheck_Icon(false);
            lStuffs.Location = new Point(flTitle.Location.X, flTitle.Location.Y + 18);
            if (lStuffs.Width > STUFF_FLOW_SIZE)
            {
                tiRatioView.Enabled = true;
            }
            else tiRatioView.Enabled = false;
            this.PrintRatio();
        }
        private string percentage(int numerator, int denominator)
        {
            return ((double)numerator / (double)denominator).ToString("0%");
        }
        private void cleanup()
        {

        }
        private void PrintRatio()
        {
            this.Visible = true;
        }
        private void lFavCheck_Icon(bool hover)
        {
            if (MainWindow.FavList.Contains(DataIndex) == true)
            {
                if (hover)
                    lFavCheck.Image = Properties.Resources.FavoriteRemove_hover;
                else
                    lFavCheck.Image = Properties.Resources.FavoriteRemove;
                tooltip.SetToolTip(lFavCheck, Properties.Resources.StrFavoriteRemove);
            }
            else
            {
                if (hover)
                    lFavCheck.Image = Properties.Resources.Favorite_hover;
                else
                    lFavCheck.Image = Properties.Resources.Favorite;
                tooltip.SetToolTip(lFavCheck, Properties.Resources.StrFavorite);
            }
        }

        #region Event Handler
        private void MabiCooker_RatioView_MouseDown(object sender, MouseEventArgs e)
        {
            mousePoint = new Point(e.X, e.Y);
        }
        private void MabiCooker_RatioView_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                Location = new Point(this.Left - (mousePoint.X - e.X),
                    this.Top - (mousePoint.Y - e.Y));
            }
        }
        private void CookRatioView_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.LastRatioPoint = new Point(this.Location.X, this.Location.Y);
            MabiCooker.RatioView = new CookRatioView(MainWindow, FavListView);
        }
        private void CookRatioView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) this.Visible = false;

            // Move CookRatioView with arrows
            if (e.KeyCode == Keys.Up) this.Location = new Point(this.Location.X, this.Location.Y - 1);
            else if (e.KeyCode == Keys.Down) this.Location = new Point(this.Location.X, this.Location.Y + 1);
            else if (e.KeyCode == Keys.Left) this.Location = new Point(this.Location.X - 1, this.Location.Y);
            else if (e.KeyCode == Keys.Right) this.Location = new Point(this.Location.X + 1, this.Location.Y);
        }
        private void lCloseRatio_Click(object sender, EventArgs e)
        {
            this.Visible = false;
        }
        private void lFavCheck_Click(object sender, EventArgs e)
        {
            if (MainWindow.FavList.Contains(DataIndex) == false)
            {
                MainWindow.AddFavList(DataIndex);
            }
            else
            {
                MainWindow.DeleteFavList(DataIndex);
            }
            lFavCheck_Icon(true);
            MainWindow.UpdateData(MainWindow.FavList, FavListView);
            if (MabiCooker.DetailView != null) MabiCooker.DetailView.UpdateData(DataIndex, SelectedCook);
            if (MainWindow.FavList.Count != 0) FavListView.BringToFront();
            else FavListView.SendToBack();
        }
        private void tiRatioView_Tick(object sender, EventArgs e)
        {
            if (lStuffs.Width >= STUFF_FLOW_SIZE)
            {
                if (lStuffs.Location.X < (0 - lStuffs.Width + tableLayoutPanel1.Width + flTitle.Width + lRank.Width - 10))
                    RightLeft = false;
                else if (lStuffs.Location.X > tableLayoutPanel1.Width + 5)
                    RightLeft = true;

                if (RightLeft)
                    lStuffs.Location = new Point(lStuffs.Location.X - 1, lStuffs.Location.Y);
                else
                    lStuffs.Location = new Point(lStuffs.Location.X + 1, lStuffs.Location.Y);
            }
            else return;
        }

        private void CookRatioView_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible == false)
                tiRatioView.Enabled = false;
            else if (this.Visible == true && lStuffs.Width > STUFF_FLOW_SIZE)
                tiRatioView.Enabled = true;
        }

        private void pbStuffOne_Click(object sender, EventArgs e)
        {
            if (this.NoticeInfo == true)
            {
                this.NoticeInfo = false;
                this.Height = 8;
            }
            else
            {
                this.NoticeInfo = true;
                this.Height = initial_height;
            }
        }

        private void CookRatioView_active(object sender, EventArgs e)
        {
            //tooltip.Active = true;
        }
        private void CookRatioView_deactive(object sender, EventArgs e)
        {
            //tooltip.Active = false;
        }

        private void lCloseRatio_MouseHover(object sender, EventArgs e)
        {
            lCloseRatio.Image = Properties.Resources.Close_hover;
        }

        private void lCloseRatio_MouseLeave(object sender, EventArgs e)
        {
            lCloseRatio.Image = Properties.Resources.Close;
        }

        private void lFavCheck_MouseHover(object sender, EventArgs e)
        {
            lFavCheck_Icon(true);
        }

        private void lFavCheck_MouseLeave(object sender, EventArgs e)
        {
            lFavCheck_Icon(false);
        }
        #endregion
    }
}
