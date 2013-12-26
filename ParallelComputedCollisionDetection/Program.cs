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
        [STAThread]
        public static void Main(string[] args)
        {
            using (Window window = new Window())
            {
                window.Run(60.0);
            }
        }
    }
}
