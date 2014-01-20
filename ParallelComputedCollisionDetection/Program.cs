using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;

namespace ParallelComputedCollisionDetection
{
    class Program
    {
        public static Window window;
        public static DebugBox db;
        public static Thread t;

        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            window = new Window();
            t = new Thread(RunForm);
            t.Start();
            window.Run(60.0);
        }

        public static void RunForm()
        {
            Thread.Sleep(500);
            Application.Run(db = new DebugBox());
        }
    }
}
