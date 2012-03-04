using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace CloverPython
{
    public class CloverInterpreter
    {
        ScriptEngine pythonEngine = null;
        ScriptScope pythonScope = null;

        /// <summary>
        /// 初始化解释器
        /// </summary>
        public void InitialzeInterpreter()
        {
            pythonEngine = Python.CreateEngine();
            pythonScope = pythonEngine.CreateScope();

            namespaceList.Add("import clr, sys");
            namespaceList.Add("clr.AddReferent('Clover')");
            namespaceList.Add("from Clover import *");
        }

        int currentLine = -1;

        List<string> codeList = new List<string>();

        List<string> namespaceList = new List<string>();

        /// <summary>
        /// 加载代码
        /// </summary>
        /// <param name="code"></param>
        public void LoadCode(string code)
        {
            codeList = code.Split('\n').ToList();
 

        }

        public bool Step()
        {


            return true;
        }



    }
}
