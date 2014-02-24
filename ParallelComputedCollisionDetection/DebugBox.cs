using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParallelComputedCollisionDetection
{
    public partial class DebugBox : Form
    {
        Window window;
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
            rtb2.Text = CollisionDetection.deviceInfo;
            fps_rtb.SetBounds(10, 15, 50, 20);
            comp_rtb.SetBounds(10, 40, 320, 320);
            rtb_log.SetBounds(10, 345, 320, 380);
            rtb_log.Text = CollisionDetection.log;
        }

        public RichTextBox getRTB()
        {
            return rtb;
        }

        public RichTextBox getRTB_FPS()
        {
            return fps_rtb;
        }

        public RichTextBox getComp_RTB()
        {
            return comp_rtb;
        }

        public RichTextBox getRTB_log()
        {
            return rtb_log;
        }
    }
}
