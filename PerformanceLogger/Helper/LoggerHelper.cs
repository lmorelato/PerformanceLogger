using System;
using System.IO;
using System.Threading.Tasks;

namespace PerformanceLogger.Helper
{
    public static class LoggerHelper
    {
        public static async Task WriteTextAsync(string folderPath, string fileName, string logMessage)
        {
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            try
            {
                using (var sw = new StreamWriter(folderPath + fileName, true))
                {
                    var logLine = $"{logMessage}\r\n";
                    await sw.WriteAsync(logLine);
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Not found");
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error reading from {0}. Message = {1}", folderPath + fileName, ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("In Main catch block. Caught: {0}", ex.Message);
                Console.WriteLine("Inner Exception is {0}", ex.InnerException);
            }
        }


    }
}
