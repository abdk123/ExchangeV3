using BWR.ShareKernel.Exceptions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BWR.Infrastructure.Exceptions
{
    public class Tracing
    {
        static string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public static void SaveException(Exception ex)
        {
            Task.Factory.StartNew(() =>
            {
                System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(ex, true);
                string exption = "FileName: " + Environment.NewLine;
                exption += trace.GetFrame(0).GetFileName() + Environment.NewLine;
                exption += "MethodName:" + Environment.NewLine;
                exption += trace.GetFrame(0).GetMethod() + Environment.NewLine;
                exption += "Line:" + Environment.NewLine;
                exption += trace.GetFrame(0).GetFileLineNumber() + Environment.NewLine;
                exption += "Column:" + Environment.NewLine;
                exption += trace.GetFrame(0).GetFileColumnNumber() + Environment.NewLine;
                File.WriteAllText(path + @"\log.txt", exption);
            });
        }
    }
}
