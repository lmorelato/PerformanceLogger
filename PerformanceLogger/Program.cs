/*
* Mainly inspired by:
*     https://blogs.msdn.microsoft.com/faber/2014/11/20/tracking-windows-performance-counters-by-application/
*
* Credits to:
*    https://social.msdn.microsoft.com/profile/faberspot
*
* Other Code references:
*     http://msdn.microsoft.com/en-us/library/1xtk877y%28v=vs.110%29.aspx
*     http://blogs.msdn.com/b/csharpfaq/archive/2012/01/23/using-async-for-file-access-alan-berman.aspx
*/

namespace PerformanceLogger
{
    internal class Program
    {
        private static void Main()
        {
            //Starts monitoring
            PerformanceLogger.Start();
        }
    }
}