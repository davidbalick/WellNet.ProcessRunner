using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WellNet.ProcessRunner;

namespace TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestFilenameWithDate();
            Debug.WriteLine(string.Format("'{0}'", Path.GetDirectoryName("abc.txt")));
        }

        private static void TestFilenameWithDate()
        {
            var tests = new[] { @"c:\hapy horses\myfile.txt", @"c:\hapy horses\myfile_{date}.txt", @"c:\hapy horses\myfile{MM/dd/yyyy}.txt" };
            foreach (var test in tests)
                Debug.WriteLine(StaticResources.FilenameWithDate(test));
        }
    }
}
