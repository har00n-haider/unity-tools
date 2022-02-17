using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class Utils
{

    public static class DebuggerUtils
    {
        public static string GetOneLineStackTrace()
        {
            var stackTrace = new System.Diagnostics.StackTrace();
            string[] strArr = new string[stackTrace.FrameCount];
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                strArr[stackTrace.FrameCount - i - 1] = stackTrace.GetFrame(i).GetMethod().Name;
            }
            return string.Join(" > ", strArr);
        }
    }

}
