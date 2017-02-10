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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using PerformanceLogger.Helper;
using PerformanceLogger.Model;

namespace PerformanceLogger
{
    public class PerformanceLogger
    {
        private const string StrSeparator = "\t";
        private const string StrXmlFilename = "config.xml";
        private static readonly SysCounter SysP = new SysCounter();
        private static readonly List<LocalProcess> MonitoredProcessesP = new List<LocalProcess>();
        private static Dictionary<string, bool> _dictSystem = new Dictionary<string, bool>();
        private static Dictionary<string, bool> _dictMonitoredProcesses = new Dictionary<string, bool>();
        private static string _fileName;
        private static string _folderPath;
        private static bool _writeToLogFile;
        private static int _maxNumberRecordsInLogFile;
        private static int _pollingSysParamsInterval;
        private static string[] _monitoredProcesses;
        private static int _numProcess;

        public static void Start()
        {
            try
            {
                //Init configs
                InitAppConfig();
                InitConfigXml();
                Console.WriteLine();

                //Inizialize sys net counters and interfaces
                SysP.InitNetCounters();

                //Add monitored Processes
                foreach (var monitoredProcess in _monitoredProcesses)
                    MonitoredProcessesP.Add(new LocalProcess(monitoredProcess));

                Console.WriteLine();

            }
            catch (Exception ex)
            {
                Console.WriteLine("[Exception error: {0}]", ex.Message);
            }

            var sb = new StringBuilder();
            var counterRecordLogFile = _maxNumberRecordsInLogFile;

            while (true)
            {
                try
                {

                    #region System

                    Console.WriteLine("------------------------------------------------------------------");
                    GetCounter();

                    if (counterRecordLogFile >= _maxNumberRecordsInLogFile)
                    {
                        counterRecordLogFile = 0;
                        var tailName = XmlConfigHelper.FileNameLog != "" ? XmlConfigHelper.FileNameLog : ".txt";

                        var currDate = DateTime.Now;
                        _fileName = "" + currDate.ToString("yyyy-MM-dd-HH.mm.ss_", CultureInfo.InvariantCulture) + tailName;
                        CreateSystemFileHeader(ref sb);

                        if (_writeToLogFile && _folderPath != "")
                            LoggerHelper.WriteTextAsync(_folderPath, _fileName, sb.ToString()).Wait();
                    }

                    CreateSystemRecord(ref sb);
                    counterRecordLogFile++;

                    if (_writeToLogFile && _folderPath != "")
                        LoggerHelper.WriteTextAsync(_folderPath, _fileName, sb.ToString()).Wait();

                    #endregion

                    Console.WriteLine();
                    Console.WriteLine("waiting next counter...");
                    Console.WriteLine();
                    Console.WriteLine();

                    Thread.Sleep(_pollingSysParamsInterval * 1000);

                }
                catch (Exception ex)
                {
                    Console.WriteLine("[Exception error: {0}]", ex.Message);
                }
            }
        }

        /// <summary>
        /// Inits counter values
        /// </summary>
        private static void GetCounter()
        {
            SysP.GetProcessorCpuTime();
            SysP.GetCpuPrivilegedTime();
            SysP.GetCpuinterruptTime();
            SysP.GetcpuDpcTime();
            SysP.GetMemAvailable();
            SysP.GetMemCommited();
            SysP.GetMemCommitLimit();
            SysP.GetMemCommitedPerc();
            SysP.GetMemPoolPaged();
            SysP.GetMemPoolNonPaged();
            SysP.GetMemCachedBytes();
            SysP.GetPageFile();
            SysP.GetProcessorQueueLengh();
            SysP.GetDiskQueueLengh();
            SysP.GetDiskRead();
            SysP.GetDiskWrite();
            SysP.GetDiskAverageTimeRead();
            SysP.GetDiskAverageTimeWrite();
            SysP.GetDiskTime();
            SysP.GetHandleCountCounter();
            SysP.GetThreadCount();
            SysP.GetContentSwitches();
            SysP.GetsystemCalls();
            SysP.GetCurretTrafficSent();
            SysP.GetCurretTrafficReceived();
            SysP.GetSampleTime();
        }

        /// <summary>
        /// Inits xml config file
        /// </summary>
        private static void InitConfigXml()
        {
            #region app.config

            // // =============== Reading app.config file
            if (AppConfigHelper.ExistsAppConfig())
            {
                AppConfigHelper.CheckAppConfig();
                _dictSystem = AppConfigHelper.ReadSystemAppConfig();
                _dictMonitoredProcesses = AppConfigHelper.ReadMonitedProcessesAppConfig();
            }
            else
            {
                AppConfigHelper.CreateAppConfig();
                AppConfigHelper.WriteAppConfigDefaultValues();
                _dictSystem = AppConfigHelper.ReadSystemAppConfig();
                _dictMonitoredProcesses = AppConfigHelper.ReadMonitedProcessesAppConfig();
            }

            Console.WriteLine();
            foreach (var pair in _dictSystem)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Dictionary content - {0} = {1}", pair.Key, pair.Value);
                Console.ResetColor();
            }

            foreach (var pair in _dictMonitoredProcesses)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Dictionary content - {0} = {1}", pair.Key, pair.Value);
                Console.ResetColor();
            }

            #endregion
        }

        /// <summary>
        /// Inits App config file
        /// </summary>
        private static void InitAppConfig()
        {
            #region config.xml

            // =============== Reading SysFields.xml file
            // check if the XML file exists, if not create the XML file with default 
            if (File.Exists(StrXmlFilename) == false)
                XmlConfigHelper.CreateXML(StrXmlFilename);
            if (File.Exists(StrXmlFilename))
            {
                // Check if root node in XML file exists; if it doesn't exist a new XML file is created with all application parameters
                if (XmlConfigHelper.CheckRootNode(StrXmlFilename) == false)
                    XmlConfigHelper.CreateXML(StrXmlFilename);
            }
            else
            {
                Console.WriteLine("The file {0} could not be located", StrXmlFilename);
            }

            //  check elements values in XML file 
            XmlConfigHelper.CheckXML(StrXmlFilename);

            //  Read elements values in XML file
            XmlConfigHelper.ReadXML(StrXmlFilename);

            // Compose the right name for the log file, including date and time in the front
            _fileName = DateTime.Now.ToString("yyyy-MM-dd-HH.mm.ss_", CultureInfo.InvariantCulture) + XmlConfigHelper.FileNameLog;
            _folderPath = XmlConfigHelper.FolderPath;
            _writeToLogFile = Convert.ToBoolean(XmlConfigHelper.WriteToLogFile);
            _maxNumberRecordsInLogFile = Convert.ToInt32(XmlConfigHelper.MaxNumberRecordsInLogFile);
            _pollingSysParamsInterval = Convert.ToInt32(XmlConfigHelper.PollingSysParamsInterval);
            _monitoredProcesses = XmlConfigHelper.MonitoredProcesses.Split(';');

            #endregion
        }

        /// <summary>
        ///     Create a string with record header with a list of performance monitor parameters
        /// </summary>
        public static void CreateSystemFileHeader(ref StringBuilder sb)
        {
            sb.Length = 0;
            try
            {
                foreach (var pair in _dictSystem)
                    switch (pair.Key)
                    {
                        case "NodeName":
                            if (pair.Value) sb.Append("NameNode" + StrSeparator);
                            break;
                        case "IpNumber":
                            if (pair.Value) sb.Append("IP Number" + StrSeparator);
                            break;
                        case "CPUProcessorTime":
                            if (pair.Value) sb.Append("CPU time %" + StrSeparator);
                            break;
                        case "CPUPrivilegedTime":
                            if (pair.Value) sb.Append("CPU Privileged %" + StrSeparator);
                            break;
                        case "CPUInterruptTime":
                            if (pair.Value) sb.Append("CPU Interrupt %" + StrSeparator);
                            break;
                        case "CPUDPCTime":
                            if (pair.Value) sb.Append("CPU deferred %" + StrSeparator);
                            break;
                        case "MEMAvailable":
                            if (pair.Value) sb.Append("Mem Avaialable %" + StrSeparator);
                            break;
                        case "MEMCommited":
                            if (pair.Value) sb.Append("Mem commited MB" + StrSeparator);
                            break;
                        case "MEMCommitLimit":
                            if (pair.Value) sb.Append("Mem commitLimit MB" + StrSeparator);
                            break;
                        case "MEMCommitedPerc":
                            if (pair.Value) sb.Append("Mem commitedPerc  MB" + StrSeparator);
                            break;
                        case "MEMPoolPaged":
                            if (pair.Value) sb.Append("Mem Pool Paged (MB)" + StrSeparator);
                            break;
                        case "MEMPoolNonPaged":
                            if (pair.Value) sb.Append("Mem PoolNonPaged (MB)" + StrSeparator);
                            break;
                        case "MEMCached":
                            if (pair.Value) sb.Append("Mem cache (MB)" + StrSeparator);
                            break;
                        case "PageFile":
                            if (pair.Value) sb.Append("PageFile (MB)" + StrSeparator);
                            break;
                        case "ProcessorQueueLengh":
                            if (pair.Value) sb.Append("ProcessorQueue" + StrSeparator);
                            break;
                        case "DISCQueueLengh":
                            if (pair.Value) sb.Append("DiskQueueLengh" + StrSeparator);
                            break;
                        case "DISKRead":
                            if (pair.Value) sb.Append("Disk Read (KB/s)" + StrSeparator);
                            break;
                        case "DISKWrite":
                            if (pair.Value) sb.Append("Disk Write (KB/s)" + StrSeparator);
                            break;
                        case "DISKAverageTimeRead":
                            if (pair.Value) sb.Append("Disk ms/Read" + StrSeparator);
                            break;
                        case "DISKAverageTimeWrite":
                            if (pair.Value) sb.Append("Disk ms/Write" + StrSeparator);
                            break;
                        case "DISKTime":
                            if (pair.Value) sb.Append("Disk time (%)" + StrSeparator);
                            break;
                        case "HANDLECountCounter":
                            if (pair.Value) sb.Append("Handle Count" + StrSeparator);
                            break;
                        case "THREADCount":
                            if (pair.Value) sb.Append("Thread Count" + StrSeparator);
                            break;
                        case "CONTENTSwitches":
                            if (pair.Value) sb.Append("Content Switches/s" + StrSeparator);
                            break;
                        case "SYSTEMCalls":
                            if (pair.Value) sb.Append("System Calls/s" + StrSeparator);
                            break;
                        case "NumProcess":
                            if (pair.Value) sb.Append("NumProcesses" + StrSeparator);
                            break;
                        case "NetTrafficSend":
                            if (pair.Value) sb.Append("NetTrafficSent(kbps)" + StrSeparator);
                            break;
                        case "NetTrafficReceive":
                            if (pair.Value) sb.Append("NetTrafficRecv(kbps)" + StrSeparator);
                            break;
                        case "SamplingTime":
                            if (pair.Value) sb.Append("Sampling Time" + StrSeparator);
                            break;
                    }

                foreach (var p in MonitoredProcessesP)
                    CreateMonitoredProcessesFileHeader(p, sb);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Exception error: {0}]", ex.Message);
            }
        }

        /// <summary>
        ///     Create a string with record header with a list of monitored local processes
        /// </summary>
        public static void CreateMonitoredProcessesFileHeader(LocalProcess p, StringBuilder sb)
        {
            foreach (var pair in _dictMonitoredProcesses)
                switch (pair.Key)
                {
                    case "MonitoredProcessesCPUProcessorTime":
                        if (pair.Value) sb.Append(p.ProcessName + "|" + "CPU time %" + StrSeparator);
                        break;
                    case "MonitoredProcessesMEMUsed":
                        if (pair.Value) sb.Append(p.ProcessName + "|" + "Mem Used MB" + StrSeparator);
                        break;
                }
        }

        /// <summary>
        ///     Create a string with values of performance monitor paramenters: string has a structure
        ///     of a record, with values separed by conventional char specified in strSeparator
        /// </summary>
        public static void CreateSystemRecord(ref StringBuilder sb)
        {
            sb.Length = 0;
            try
            {
                foreach (var pair in _dictSystem)
                    switch (pair.Key)
                    {
                        case "CPUProcessorTime":
                            if (pair.Value)
                            {
                                sb.Append("" + SysP.CpuProcessorTime + StrSeparator);
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine("CPU time          : {0} %", SysP.CpuProcessorTime);
                                Console.ResetColor();
                            }
                            break;
                        case "CPUPrivilegedTime":
                            if (pair.Value)
                            {
                                sb.Append("" + SysP.CpuPrivilegedTime + StrSeparator);
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine("CPU Privileged    : {0} %", SysP.CpuPrivilegedTime);
                                Console.ResetColor();
                            }
                            break;
                        case "CPUInterruptTime":
                            if (pair.Value)
                            {
                                sb.Append("" + SysP.CpuInterruptTime + StrSeparator);
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine("CPU Interrupt     : {0} %", SysP.CpuPrivilegedTime);
                                Console.ResetColor();
                            }
                            break;
                        case "CPUDPCTime":
                            if (pair.Value)
                            {
                                sb.Append("" + SysP.CpudpcTime + StrSeparator);
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine("CPU deferred      : {0} %", SysP.CpudpcTime);
                                Console.ResetColor();
                            }
                            break;
                        case "MEMAvailable":
                            if (pair.Value)
                            {
                                sb.Append("" + SysP.MemAvailable + StrSeparator);
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Mem Avaialable    : {0} MB", SysP.MemAvailable);
                                Console.ResetColor();
                            }
                            break;
                        case "MEMCommited":
                            if (pair.Value)
                            {
                                sb.Append("" + SysP.MemCommited + StrSeparator);
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Mem commited      : {0} MB", SysP.MemCommited);
                                Console.ResetColor();
                            }
                            break;
                        case "MEMCommitLimit":
                            if (pair.Value)
                            {
                                sb.Append("" + SysP.MemCommitLimit + StrSeparator);
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Mem commitLimit   : {0} MB", SysP.MemCommitLimit);
                                Console.ResetColor();
                            }
                            break;
                        case "MEMCommitedPerc":
                            if (pair.Value)
                            {
                                sb.Append("" + SysP.MemCommitedPerc + StrSeparator);
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Mem commitedPerc  : {0} %", SysP.MemCommitedPerc);
                                Console.ResetColor();
                            }
                            break;
                        case "MEMPoolPaged":
                            if (pair.Value)
                            {
                                sb.Append("" + SysP.MemPoolPaged + StrSeparator);
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Mem Pool Paged    : {0} MB", SysP.MemPoolPaged);
                                Console.ResetColor();
                            }
                            break;
                        case "MEMPoolNonPaged":
                            if (pair.Value)
                            {
                                sb.Append("" + SysP.MemPoolNonPaged + StrSeparator);
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Mem PoolNonPaged  : {0} MB", SysP.MemPoolNonPaged);
                                Console.ResetColor();
                            }
                            break;
                        case "MEMCached":
                            if (pair.Value)
                            {
                                sb.Append("" + SysP.MemCached + StrSeparator);
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Mem cache         : {0} MB", SysP.MemCached);
                                Console.ResetColor();
                            }
                            break;
                        case "PageFile":
                            if (pair.Value)
                            {
                                sb.Append("" + SysP.PageFile + StrSeparator);
                                Console.WriteLine("PageFile          : {0} MB", SysP.PageFile);
                            }
                            break;
                        case "ProcessorQueueLengh":
                            if (pair.Value)
                            {
                                sb.Append("" + SysP.ProcessorQueueLengh + StrSeparator);
                                Console.WriteLine("ProcessorQueue    : {0}   ", SysP.ProcessorQueueLengh);
                            }
                            break;
                        case "DISCQueueLengh":
                            if (pair.Value)
                            {
                                sb.Append("" + SysP.DiscQueueLengh + StrSeparator);
                                Console.WriteLine("DiskQueueLengh    : {0}   ", SysP.DiscQueueLengh);
                            }
                            break;
                        case "DISKRead":
                            if (pair.Value)
                            {
                                sb.Append("" + SysP.DiskRead + StrSeparator);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Disk Read         : {0} KB/s", SysP.DiskRead);
                                Console.ResetColor();
                            }
                            break;
                        case "DISKWrite":
                            if (pair.Value)
                            {
                                sb.Append("" + SysP.DiskWrite + StrSeparator);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Disk Write        : {0} KB/s", SysP.DiskWrite);
                                Console.ResetColor();
                            }
                            break;
                        case "DISKAverageTimeRead":
                            if (pair.Value)
                            {
                                sb.Append("" + SysP.DiskAverageTimeRead + StrSeparator);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Disk sec/Read-Avg : {0} ms", SysP.DiskAverageTimeRead);
                                Console.ResetColor();
                            }
                            break;
                        case "DISKAverageTimeWrite":
                            if (pair.Value)
                            {
                                sb.Append("" + SysP.DiskAverageTimeWrite + StrSeparator);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Disk sec/Write-Avg: {0} ms", SysP.DiskAverageTimeWrite);
                                Console.ResetColor();
                            }
                            break;
                        case "DISKTime":
                            if (pair.Value)
                            {
                                sb.Append("" + SysP.DiskTime + StrSeparator);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Disk time         : {0} %", SysP.DiskTime);
                                Console.ResetColor();
                            }
                            break;
                        case "HANDLECountCounter":
                            if (pair.Value)
                            {
                                sb.Append("" + SysP.HandleCountCounter + StrSeparator);
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine("Handle Count      : {0} ", SysP.HandleCountCounter);
                                Console.ResetColor();
                            }
                            break;
                        case "THREADCount":
                            if (pair.Value)
                            {
                                sb.Append("" + SysP.ThreadCount + StrSeparator);
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine("Thread Count      : {0} ", SysP.ThreadCount);
                                Console.ResetColor();
                            }
                            break;
                        case "CONTENTSwitches":
                            if (pair.Value)
                            {
                                sb.Append("" + SysP.ContentSwitches + StrSeparator);
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine("Content Switches/s: {0} ", SysP.ContentSwitches);
                                Console.ResetColor();
                            }
                            break;
                        case "SYSTEMCalls":
                            if (pair.Value)
                            {
                                sb.Append("" + SysP.SystemCalls + StrSeparator);
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine("System Calls/s    : {0} ", SysP.SystemCalls);
                                Console.ResetColor();
                            }
                            break;
                        case "NumProcess":
                            if (pair.Value)
                            {
                                var processlist = Process.GetProcesses();
                                _numProcess = processlist.Length;
                                sb.Append("" + _numProcess + StrSeparator);
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine("Number Processes  : {0} ", _numProcess);
                                Console.ResetColor();
                            }
                            break;
                        case "NetTrafficSend":
                            if (pair.Value)
                            {
                                sb.Append("" + SysP.NetTrafficSend + StrSeparator);
                                Console.WriteLine("NetTrafficSent    : {0} kbps", SysP.NetTrafficSend);
                            }
                            break;
                        case "NetTrafficReceive":
                            if (pair.Value)
                            {
                                sb.Append("" + SysP.NetTrafficReceive + StrSeparator);
                                Console.WriteLine("NetTrafficRecv    : {0} kbps", SysP.NetTrafficReceive);
                            }
                            break;
                        case "SamplingTime":
                            if (pair.Value)
                            {
                                sb.Append(
                                    "" + SysP.SamplingTime.ToString("yyyy-MM-dd-HH.mm.ss", CultureInfo.InvariantCulture) +
                                    StrSeparator);
                                Console.WriteLine("Sampling Time     : {0}  ",
                                    SysP.SamplingTime.ToString("yyyy-MM-dd-HH.mm.ss", CultureInfo.InvariantCulture));
                            }
                            break;
                        case "NodeName":
                            if (pair.Value)
                            {
                                sb.Append("" + SysP.NodeName + StrSeparator);
                                Console.WriteLine("Node Name         : {0}  ", SysP.NodeName);
                            }
                            break;
                        case "IpNumber":
                            if (pair.Value)
                            {
                                sb.Append("" + SysP.IpNumber + StrSeparator);
                                Console.WriteLine("Ip Number         : {0}  ", SysP.IpNumber);
                            }
                            break;
                    }

                foreach (var p in MonitoredProcessesP)
                    CreateMonitoredProcessesRecord(p, ref sb);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Exception error: {0}]", ex);
            }
        }

        /// <summary>
        ///     Create a string with values of monitored local processes
        /// </summary>
        public static void CreateMonitoredProcessesRecord(LocalProcess p, ref StringBuilder sb)
        {
            p.GetProcessorCpuTime();
            p.GetMemUsed();

            Console.WriteLine();
            Console.WriteLine("-- " + p.ProcessName.ToUpper());

            foreach (var pair in _dictMonitoredProcesses)
                switch (pair.Key)
                {
                    case "MonitoredProcessesCPUProcessorTime":
                        if (pair.Value)
                        {
                            sb.Append(p.CpuProcessorTime + StrSeparator);
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("CPU time          : {0} %", p.CpuProcessorTime);
                            Console.ResetColor();
                        }
                        break;
                    case "MonitoredProcessesMEMUsed":
                        if (pair.Value)
                        {
                            sb.Append(p.MemUsed + StrSeparator);
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("Mem Used          : {0} MB", p.MemUsed);
                            Console.ResetColor();
                        }
                        break;
                }
        }
    }
}

/*
******************************************************
WINDOWS COUNTERS
******************************************************

1 CategoryName: Processor
******************************************************
PerformanceCounter(“Processor“, “% Processor Time“, “_Total”);
The Processor\% Processor Time counter determines the percentage of time the processor is busy by measuring the percentage of time the thread of the Idle process is running and then subtracting that from 100 percent.This measurement is the amount of processor utilization

PerformanceCounter(“Processor“, “% Interrupt Time“, “_Total”);
The rate, in average number of interrupts in incidents per second, at which the processor received and serviced hardware interrupts.It does not include deferred procedure calls, which are counted separately.

PerformanceCounter(“Processor“, “% DPC Time“, “_Total”);
The percentage of time that the processor spent receiving and servicing deferred procedure calls during the sample interval.Deferred procedure calls are interrupts that run at a lower priority than standard interrupts.

PerformanceCounter(“Processor“, “% Privileged Time“, “_Total”);
The percentage of non-idle processor time spent in privileged mode.Privileged mode is a processing mode designed for operating system components and hardware-manipulating drivers. It allows direct access to hardware and all memory.The alternative, user mode, is a restricted processing mode designed for applications, environment subsystems, and integral subsystems.The operating system switches application threads to privileged mode to gain access to operating system services. This includes time spent servicing interrupts and deferred procedure calls (DPCs). A high rate of privileged time might be caused by a large number of interrupts generated by a failing device.This counter displays the average busy time as a percentage of the sample time.

 
2 CategoryName: Memory
******************************************************
PerformanceCounter(“Memory“, “Available MBytes“, null);
This measures the amount of physical memory, in megabytes, available for running processes.If this value is less than 5 percent of the total physical RAM, that means there is insufficient memory, and that can increase paging activity.

PerformanceCounter(“Memory“, “Committed Bytes“, null);
it shows the amount of virtual memory, in bytes, that can be committed without having to extend the paging file(s). Committed memory is physical memory which has space reserved on the disk paging files.There can be one or more paging files on each physical drive.If the paging file(s) are expanded, this limit increases accordingly.

PerformanceCounter(“Memory“, “Commit Limit“, null);
it shows the amount of virtual memory, in bytes, that can be committed without having to extend the paging file(s). Committed memory is physical memory which has space reserved on the disk paging files. There can be one or more paging files on each physical drive. If the paging file(s) are expanded, this limit increases accordingly.
 
PerformanceCounter(“Memory“, “% Committed Bytes In Use“, null);
it shows the ratio of Memory\ Committed Bytes to the Memory\ Commit Limit. Committed memory is physical memory in use for which space has been reserved in the paging file so that it can be written to disk. The commit limit is determined by the size of the paging file. If the paging file is enlarged, the commit limit increases, and the ratio is reduced.
 
PerformanceCounter(“Memory“, “Pool Paged Bytes“, null);
it shows the size, in bytes, of the paged pool. Memory\ Pool Paged Bytes is calculated differently than Process\ Pool Paged Bytes, so it might not equal Process(_Total )\ Pool Paged Bytes.
 
PerformanceCounter(“Memory“, “Pool Nonpaged Bytes“, null);
it shows the size, in bytes, of the nonpaged pool. Memory\ Pool Nonpaged Bytes is calculated differently than Process\ Pool Nonpaged Bytes, so it might not equal Process(_Total )\ Pool Nonpaged Bytes.
 
PerformanceCounter(“Memory“, “Cache Bytes“, null);
it shows the sum of the values of System Cache Resident Bytes, System Driver Resident Bytes, System Code Resident Bytes, and Pool Paged Resident Bytes.
 

3 CateroryName: PhysicalDisk
*****************************************************
PerformanceCounter(“PhysicalDisk“, “Disk Read Bytes/sec“, “_Total”);
PerformanceCounter(“PhysicalDisk“, “Disk Write Bytes/sec“, “_Total”);
it captures the total number of bytes sent to the disk (write) and retrieved from the disk (read) during write or read operations.
 
PerformanceCounter(“PhysicalDisk“, “Avg. Disk sec/Read“, “_Total”);
PerformanceCounter(“PhysicalDisk“, “Avg. Disk sec/Write“, “_Total”);
it captures the average time, in seconds, of a read/write of data from/to the disk.
 
PerformanceCounter(“System“, “Context Switches/sec“, null);
A context switch occurs when the kernel switches the processor from one thread to another—for example, when a thread with a higher priority than the running thread becomes ready. Context switching activity is important for several reasons. A program that monopolizes the processor lowers the rate of context switches because it does not allow much processor time for the other processes’ threads. A high rate of context switching means that the processor is being shared repeatedly—for example, by many threads of equal priority. A high context-switch rate often indicates that there are too many threads competing for the processors on the system. The System\Context Switches/sec counter reports systemwide context switches.
 

4 CategoryName: Process
******************************************************
Each process provides the resources needed to execute a program. A process has a virtual address space, executable code, open handles to system objects, a security context, a unique process identifier, environment variables, a priority class, minimum and maximum working set sizes, and at least one thread of execution.Each process is started with a single thread, often called the primary thread, but can create additional threads from any of its threads.
A thread is the entity within a process that can be scheduled for execution.All threads of a process share its virtual address space and system resources.In addition, each thread maintains exception handlers, a scheduling priority, thread local storage, a unique thread identifier, and a set of structures the system will use to save the thread context until it is scheduled.The thread context includes the thread’s set of machine registers, the kernel stack, a thread environment block, and a user stack in the address space of the thread’s process. Threads can also have their own security context, which can be used for impersonating clients. (for more information  see “About Processes and Thread” http://msdn.microsoft.com/en-us/library/windows/desktop/ms681917%28v=vs.85%29.aspx)

PerformanceCounter(“Process“, “Thread Count“, “_Total”);
The number of threads created by the process.This counter does not indicate which threads are busy and which are idle. It displays the last observed value, not an average.

PerformanceCounter(“Process“, “Handle Count“, “_Total”);
the value reports the number of handles that processes opened for objects they create.Handles are used by programs to identify resources that they must access.The value of this counter tends to rise during a memory leak.

PerformanceCounter(“Process“, “Thread Count“, “_Total”);
the value reports over time how many threads the processes create.
 

5 CategoryName: System
******************************************************

PerformanceCounter(“System“, “Context Switches/sec“, null);
A context switch occurs when the kernel switches the processor from one thread to another.A context switch might also occur when a thread with a higher priority than the running thread becomes ready or when a running thread must wait for some reason (such as an I/O operation). The Thread\Context Switches/sec counter value increases when the thread gets or loses the time of the processor.

PerformanceCounter(“System“, “System Calls/sec“, null);
This is the number of system calls being serviced by the CPU per second.By comparing the Processor’s Interrupts/sec with the System Calls/sec we can get a picture of how much effort the system requires to respond to attached hardware. On a healthy system, the Interrupts per second should be negligible in comparison to the number of System Calls per second.When the system has to repeatedly call interrupts for service, it’s indicative of a hardware failure.


PerformanceCounter(“System“, “Processor Queue Length“, null);
The System\Processor Queue Length counter is a rough indicator of the number of threads each processor is servicing.
*/
