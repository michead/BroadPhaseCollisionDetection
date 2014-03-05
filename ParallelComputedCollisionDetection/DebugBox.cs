using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;

namespace ParallelComputedCollisionDetection
{
    public partial class DebugBox : Form
    {

        public DebugBox()
        {
            InitializeComponent();
        }

        private void DebugBox_Load(object sender, EventArgs e)
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.TransparencyKey = this.BackColor;
            this.SetBounds(0, 0, Screen.GetWorkingArea(this).Width, Screen.GetWorkingArea(this).Height);
            rtb.SetBounds(this.Width - 350, 15, 350, 250);
            rtb2.SetBounds(rtb.Location.X, 300, 350, 425);
            rtb2.Text = Program.cd.deviceInfo;
            fps_rtb.SetBounds(10, 15, 50, 20);
            comp_rtb.SetBounds(10, 40, 320, 320);
            rtb_log.SetBounds(10, 345, 320, 380);
            rtb_log.Text = Program.cd.log;
            Program.ready = true;
            this.BringToFront();
        }

        private void DebugBox_Layout(object sender, LayoutEventArgs e)
        {
            this.BringToFront();
        }
        

        public void close(){
            this.Close();
        }
    }
}
