using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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

        StreamWriter writer;

        public void Initialize()
        {
            totalScript = "";
            if (writer != null)
                writer.Close();
            writer = File.CreateText("scripts.txt");
        }

        string totalScript = "";

        public void AddTuckingAction(List<Face> faceList, List<Face> rotatedFace, List<Face> fixedFace, 
            Edge edge, Face ceilingFace, Face floorFace, bool isPositive)
        {
            ActionCore(faceList, rotatedFace, fixedFace, edge, isPositive);

            string scripts = "";

            scripts += "ceilingFace = clover.FindFacesByID(" + ceilingFace.ID.ToString() + ")\r\n";
            scripts += "floorFace = clover.FindFacesByID(" + floorFace.ID.ToString() + ")\r\n";
            scripts += "clover.UpdateTableAfterTucking(ceilingFace, floorFace, edge, "
                + (isPositive ? "True" : "False")
                + ")\r\n";
            scripts += "clover.AntiOverlap()\r\n";

            totalScript += scripts;

            writer.Write(scripts);
            writer.Flush();
        }

        public void ActionCore(List<Face> faceList, List<Face> rotatedFace, List<Face> fixedFace, Edge edge, bool isPositive)
        {
            string scripts = "";

            scripts += "\r\n#### New\r\n";
            scripts += "edge = Edge(Vertex(" + edge.Vertex1.GetPoint3D().ToString() 
                + "), Vertex(" + edge.Vertex2.GetPoint3D().ToString() + "))\r\n";

            string strFaceWithFoldLine = "[";
            foreach (Face face in faceList)
            {
                scripts += "faces = clover.FindFacesByID(" + face.ID.ToString() + ")\r\n";
                scripts += "CutFaces(faces, edge)\r\n";
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

            scripts += "faces = clover.FindFacesByIDs(List[int](" + strfaceWithoutFoldLine + "))\r\n";
            scripts += "RotateFaces(faces, edge, 180)\r\n";


            string strFixFace = "[";
            foreach (Face face in fixedFace)
            {
                strFixFace += face.ID.ToString() + ",";
            }
            strFixFace.Remove(strFixFace.Length - 1);
            strFixFace= strFixFace.Substring(0, strFixFace.Length - 1);
            strFixFace += "]";

            scripts += "faceWithFoldLine = List[int](" + strFaceWithFoldLine + ")\r\n";
            scripts += "faceWithoutFoldLine = List[int](" + strfaceWithoutFoldLine + ")\r\n";
            scripts += "fixFace = List[int](" + strFixFace + ")\r\n";

            scripts += "clover.UpdateTableAfterFoldUp(faceWithFoldLine, faceWithoutFoldLine, fixFace, " 
                + (isPositive ? "True" : "False") 
                + ")\r\n";

            totalScript += scripts;

            writer.Write(scripts);
            writer.Flush();
        }

        public void AddFoldingUpAction(List<Face> faceList, List<Face> rotatedFace, List<Face> fixedFace, Edge edge, bool isPositive)
        {
            string scripts = "";

            ActionCore(faceList, rotatedFace, fixedFace, edge, isPositive);

            scripts += "clover.AntiOverlap()\r\n";

            totalScript += scripts;

            writer.Write(scripts);
            writer.Flush();
        }

        public string GetScript()
        {
            return totalScript;
        }





    }
}