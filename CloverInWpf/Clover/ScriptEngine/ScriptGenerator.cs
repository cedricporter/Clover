using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clover
{
    class ScriptGenerator
    {
        static ScriptGenerator instance = null;
        static public ScriptGenerator GetInstance()
        {
            if (instance == null)
            {
                instance = new ScriptGenerator();
            }
            return instance;
        }

        private ScriptGenerator()
        {
            Initialize();
        }

        public void Initialize()
        {
            scripts = "";
        }

        string scripts;
        public void AddFoldingUpAction(List<Face> faceList, List<Face> rotatedFace, Edge edge)
        {
            scripts += "edge = Edge(Vertex(" + edge.Vertex1.GetPoint3D().ToString() 
                + "), Vertex(" + edge.Vertex2.GetPoint3D().ToString() + "))\n";
            foreach (Face face in faceList)
            {
                scripts += "faces = clover.FindFacesByID(" + face.ID.ToString() + ")\n";
                scripts += "CutFaces(faces, edge)\n";
            }
            foreach (Face face in rotatedFace)
            {
                scripts += "faces = clover.FindFacesByID(" + face.ID.ToString() + ")\n";
                scripts += "RotateFaces(faces, edge, 180)\n";
            }
            scripts += "clover.FaceGroupLookupTable.UpdateTableAfterFoldUp()\n";
            scripts += "clover.AntiOverlap()\n";
        }

        public string GetScript()
        {
            return scripts;
        }





    }
}