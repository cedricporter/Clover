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
        public void AddFoldingUpAction(List<Face> faceList, Edge edge)
        {
            foreach (Face face in faceList)
            {
                scripts += "FindFaceByID(" + face.ID.ToString() + ")\n";
                scripts += "CutFaces(face, edge)\n";
            }
        }

        public string GetScript()
        {
            return scripts;
        }





    }
}