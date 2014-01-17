using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace ParallelComputedCollisionDetection
{
    public interface Body
    {
        double getRadius();
        Vector3 getPos();

        void setPos(Vector3 pos);

        void calculateBoundingSphere();

        Sphere getBSphere();

        void Draw();
    }
}
