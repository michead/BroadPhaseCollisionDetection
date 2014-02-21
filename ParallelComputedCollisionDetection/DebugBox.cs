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
            this.SetBounds(Screen.PrimaryScreen.Bounds.Width - 350, 0, 380, 500);
            rtb2.SetBounds(rtb.Location.X, 300, this.Width, 350);
            rtb2.Text = CollisionDetection.deviceInfo;
        }

        public RichTextBox getRTB()
        {
            return rtb;
        }

        public RichTextBox getRTB_FPS()
        {
            return fps_rtb;
        }
    }
}
