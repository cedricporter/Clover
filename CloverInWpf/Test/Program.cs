using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronPython;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace Test
{
   public class Test
    {
        public string Hello()
        {
            return "Hello World";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ScriptEngine engine = Python.CreateEngine();
            ScriptScope scope = engine.CreateScope();
            var strExpression = "CMethod('Python')";
            var sourceCode = engine.CreateScriptSourceFromString(strExpression);

            scope.SetVariable("CMethod", (Func<string, string>)TMethod);
            var actual = sourceCode.Execute<string>(scope);
            scope.RemoveVariable("CMethod");
            Console.WriteLine(actual);


            strExpression = @"
import clr, sys
clr.AddReference('Test')
from Test import *
test=Test()
test.Hello()
            ";

            sourceCode = engine.CreateScriptSourceFromString(strExpression);
            actual = sourceCode.Execute<string>(scope);
            Console.WriteLine(actual);
        }

        public static string TMethod(string info)
        {
            return "Hello:" + info;
        }
    }
}
