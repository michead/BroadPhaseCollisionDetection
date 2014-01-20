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
            this.SetBounds(Screen.PrimaryScreen.Bounds.Width - 250, 0, 250, 150);
            rtb.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            rtb.ReadOnly = true;
        }

        public RichTextBox getRTB()
        {
            return rtb;
        }
    }
}
