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
using Cloo;
using Cloo.Bindings;

namespace ParallelComputedCollisionDetection
{
    class Program
    {
        public static Window window;
        public static DebugBox db;
        public static Thread t;
        public static CollisionDetection cd;
        public static bool ready = false;

        //[STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            window = new Window();
            cd = new CollisionDetection();
            //cd.deviceSetUp();
            t = new Thread(RunForm);
            //t.Start();
            window.Run(60.0);
            //cd.DisposeBuffers();
            //cd.DisposeComponents();
            //cd.DisposeQueueAndContext();
        }

        public static void RunForm()
        {
            Application.Run(db = new DebugBox());
        }
    }
}
