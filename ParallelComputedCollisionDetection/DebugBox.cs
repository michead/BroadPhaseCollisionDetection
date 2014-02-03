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
            this.SetBounds(Screen.PrimaryScreen.Bounds.Width - 340, 0, 340, Screen.PrimaryScreen.Bounds.Height);
            rtb2.SetBounds(rtb.Location.X, 185, this.Width, 580);
            rtb2.Text = CollisionDetection.deviceInfo();
        }

        public RichTextBox getRTB()
        {
            return rtb;
        }
    }
}
