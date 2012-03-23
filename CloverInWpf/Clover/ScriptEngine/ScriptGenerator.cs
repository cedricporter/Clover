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

        public void AddTuckingAction(List<Face> faceList, List<Face> rotatedFace, List<Face> fixedFace, 
            Edge edge, Face ceilingFace, Face floorFace, bool isPositive)
        {
            AddFoldingUpAction(faceList, rotatedFace, fixedFace, edge, isPositive);
            scripts += "ceilingFace = clover.FindFacesByID(" + ceilingFace.ID.ToString() + ")\n";
            scripts += "floorFace = clover.FindFacesByID(" + floorFace.ID.ToString() + ")\n";
            scripts += "clover.UpdateTableAfterTucking(ceilingFace, floorFace, edge, "
                + (isPositive ? "True" : "False")
                + ")\n";
            scripts += "clover.AntiOverlap()\n";
        }

        string scripts;
        public void AddFoldingUpAction(List<Face> faceList, List<Face> rotatedFace, List<Face> fixedFace, Edge edge, bool isPositive)
        {
            scripts += "\n#### New\n";
            scripts += "edge = Edge(Vertex(" + edge.Vertex1.GetPoint3D().ToString() 
                + "), Vertex(" + edge.Vertex2.GetPoint3D().ToString() + "))\n";

            string strFaceWithFoldLine = "[";
            foreach (Face face in faceList)
            {
                scripts += "faces = clover.FindFacesByID(" + face.ID.ToString() + ")\n";
                scripts += "CutFaces(faces, edge)\n";
                strFaceWithFoldLine += face.ID.ToString() + ",";
            }
            strFaceWithFoldLine = strFaceWithFoldLine.Substring(0, strFaceWithFoldLine.Length - 1);
            strFaceWithFoldLine += "]";

            string strfaceWithoutFoldLine = "[";
            foreach (Face face in rotatedFace)
            {
                strfaceWithoutFoldLine += face.ID.ToString() + ",";
            }
            strfaceWithoutFoldLine = strfaceWithoutFoldLine.Substring(0, strfaceWithoutFoldLine.Length - 1);
            strfaceWithoutFoldLine += "]";

            scripts += "faces = clover.FindFacesByIDs(List[int](" + strfaceWithoutFoldLine + "))\n";
            scripts += "RotateFaces(faces, edge, 180)\n";


            string strFixFace = "[";
            foreach (Face face in fixedFace)
            {
                strFixFace += face.ID.ToString() + ",";
            }
            strFixFace.Remove(strFixFace.Length - 1);
            strFixFace= strFixFace.Substring(0, strFixFace.Length - 1);
            strFixFace += "]";

            scripts += "faceWithFoldLine = List[int](" + strFaceWithFoldLine + ")\n";
            scripts += "faceWithoutFoldLine = List[int](" + strfaceWithoutFoldLine + ")\n";
            scripts += "fixFace = List[int](" + strFixFace + ")\n";

            scripts += "clover.UpdateTableAfterFoldUp(faceWithFoldLine, faceWithoutFoldLine, fixFace, " 
                + (isPositive ? "True" : "False") 
                + ")\n";

            scripts += "clover.AntiOverlap()\n";
        }

        public string GetScript()
        {
            return scripts;
        }





    }
}