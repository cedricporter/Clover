using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;
namespace Clover.AbstractLayer
{
    class Convert3DInfoTo2D
    {
        private Convert3DInfoTo2D();
        static private Convert3DInfoTo2D MySelf = null; 

        static public Convert3DInfoTo2D GetConverter()
        {
            if (MySelf == null)
            {
                MySelf = new Convert3DInfoTo2D();
            }
            return MySelf;
        }

        public bool ConvertPos(Vector3 pos3D, Camera camera, ref Vector2 pos2D)
        {
            Matrix4 mv = camera.ViewMatrix;
            Matrix4 pv = camera.ProjectionMatrix;
            Vector4 p3D;
            p3D.x = pos3D.x;
            p3D.y = pos3D.y;
            p3D.z = pos3D.z;
            p3D.w = 1;

            Vector4 v4 = pv * mv * p3D;

            if (v4.w == 0)
            {
                return false;
            }

            v4.x /= v4.w;
            v4.y /= v4.w;

            pos2D.x = v4.x;
            pos2D.y = v4.y;
            return true;
        }
    }
}
