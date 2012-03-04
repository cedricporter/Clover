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

            string initialString = 
            @"
import clr, sys
clr.AddReference('Clover')
from Clover import *
clover = CloverController.GetInstance();
";

            pythonEngine.Execute(initialString, pythonScope);
        }

        int currentLine = -1;

        List<string> codeList = new List<string>();


        /// <summary>
        /// 执行一行代码
        /// </summary>
        /// <param name="line"></param>
        public string ExecuteOneLine(string line)
        {
            var ret = pythonEngine.Execute(line, pythonScope);
            if (ret != null)
            {
                return ret.ToString();
            }
            return "";
        }

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
