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
import System.Collections.Generic.List as List
clr.AddReference('Clover')
from Clover import *
# 获取CloverController的实例
clover = CloverController.GetInstance();
# 取出所有的函数指针
FindFacesByVertex = clover.FindFacesByVertex
GetFoldingLine = clover.GetFoldingLine
GetVertex = clover.GetVertex
CutFaces = clover.AnimatedCutFaces
_CutFaces = clover.CutFaces
_RotateFaces = clover.RotateFaces
RotateFaces = clover.AnimatedRotateFaces
Undo = clover.Undo
Redo = clover.Redo
UpdateFaceGroupTable = clover.UpdateFaceGroupTable
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
            try
            {
                var ret = pythonEngine.Execute(line, pythonScope);
                if (ret != null)
                {
                    return ret.ToString();
                }
            }
            catch (System.Exception ex)
            {
                //throw ex;
                return ex.Message;
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
