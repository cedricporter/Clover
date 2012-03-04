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
        }




    }
}
