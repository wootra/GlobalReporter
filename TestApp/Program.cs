using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IOHandling;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            String argTxt = "";
            for (int i = 0; i < args.Length; i++)
            {
                argTxt += " " + args[i];
            }
                IOHandling.ProcessHandler.getProcessAfterStart(@"Z:\BuildPrograms\GlobalReporter_Release\WebkitREporter.exe", argTxt);
        }
    }
}
