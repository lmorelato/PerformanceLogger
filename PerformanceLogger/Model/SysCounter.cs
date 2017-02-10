using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace PerformanceLogger.Model
{
    /// <summary>
    /// System Performance Counter
    /// </summary>
    public class SysCounter
    {
        private readonly PerformanceCounter _contentSwitches = new PerformanceCounter("System", "Context Switches/sec", null);
        private readonly PerformanceCounter _cpuDpcTime = new PerformanceCounter("Processor", "% DPC Time", "_Total");
        private readonly PerformanceCounter _cpuInterruptTime = new PerformanceCounter("Processor", "% Interrupt Time", "_Total");
        private readonly PerformanceCounter _cpuPrivilegedTime = new PerformanceCounter("Processor", "% Privileged Time", "_Total");
        private readonly PerformanceCounter _cpuProcessorTime = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        private readonly PerformanceCounter _diskAverageTimeRead = new PerformanceCounter("PhysicalDisk", "Avg. Disk sec/Read", "_Total");
        private readonly PerformanceCounter _diskAverageTimeWrite = new PerformanceCounter("PhysicalDisk", "Avg. Disk sec/Write", "_Total");
        private readonly PerformanceCounter _diskQueueLengh = new PerformanceCounter("PhysicalDisk", "Avg. Disk Queue Length", "_Total");
        private readonly PerformanceCounter _diskRead = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", "_Total");
        private readonly PerformanceCounter _diskTime = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
        private readonly PerformanceCounter _diskWrite = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", "_Total");
        private readonly PerformanceCounter _handleCountCounter = new PerformanceCounter("Process", "Handle Count", "_Total");
        private string[] _interfaces;
        private readonly PerformanceCounter _memAvailable = new PerformanceCounter("Memory", "Available MBytes", null);
        private readonly PerformanceCounter _memCached = new PerformanceCounter("Memory", "Cache Bytes", null);
        private readonly PerformanceCounter _memCommited = new PerformanceCounter("Memory", "Committed Bytes", null);
        private readonly PerformanceCounter _memCommitedPerc = new PerformanceCounter("Memory", "% Committed Bytes In Use", null);
        private readonly PerformanceCounter _memCommitLimit = new PerformanceCounter("Memory", "Commit Limit", null);
        private readonly PerformanceCounter _memPollNonPaged = new PerformanceCounter("Memory", "Pool Nonpaged Bytes", null);
        private readonly PerformanceCounter _memPollPaged = new PerformanceCounter("Memory", "Pool Paged Bytes", null);
        private readonly PerformanceCounter _pageFile = new PerformanceCounter("Paging File", "% Usage", "_Total");
        private PerformanceCounterCategory _performanceNetCounterCategory;
        private readonly PerformanceCounter _processorQueueLengh = new PerformanceCounter("System", "Processor Queue Length", null);
        private readonly PerformanceCounter _systemCalls = new PerformanceCounter("System", "System Calls/sec", null);
        private readonly PerformanceCounter _threadCount = new PerformanceCounter("Process", "Thread Count", "_Total");
        private PerformanceCounter[] _trafficReceivedCounters;
        private PerformanceCounter[] _trafficSentCounters;

        public string NodeName { get; set; }
        public string IpNumber { get; set; }
        public float CpuProcessorTime { get; set; }
        public float CpuPrivilegedTime { get; set; }
        public float CpuInterruptTime { get; set; }
        public float CpudpcTime { get; set; }
        public float MemAvailable { get; set; }
        public float MemCommited { get; set; }
        public float MemCommitLimit { get; set; }
        public float MemCommitedPerc { get; set; }
        public float MemPoolPaged { get; set; }
        public float MemPoolNonPaged { get; set; }
        public float MemCached { get; set; }
        public float PageFile { get; set; }
        public float ProcessorQueueLengh { get; set; }
        public float DiscQueueLengh { get; set; }
        public float DiskRead { get; set; }
        public float DiskWrite { get; set; }
        public float DiskAverageTimeRead { get; set; }
        public float DiskAverageTimeWrite { get; set; }
        public float DiskTime { get; set; }
        public float HandleCountCounter { get; set; }
        public float ThreadCount { get; set; }
        public int ContentSwitches { get; set; }
        public int SystemCalls { get; set; }
        public float NetTrafficSend { get; set; }
        public float NetTrafficReceive { get; set; }
        public DateTime SamplingTime { get; set; }

        public void InitNetCounters()
        {
            // PerformanceCounter(CategoryName,CounterName,InstanceName)
            _performanceNetCounterCategory = new PerformanceCounterCategory("Network Interface");
            _interfaces = _performanceNetCounterCategory.GetInstanceNames();

            var length = _interfaces.Length;

            if (length > 0)
            {
                _trafficSentCounters = new PerformanceCounter[length];
                _trafficReceivedCounters = new PerformanceCounter[length];
            }

            for (var i = 0; i < length; i++)
            {
                // Initializes a new, read-only instance of the PerformanceCounter class.
                // 1st paramenter: "categoryName"-The name of the performance counter category (performance object) with which 
                //                                this performance counter is associated. 
                // 2nd paramenter: "CounterName" -The name of the performance counter. 
                // 3rd paramenter: "instanceName" -The name of the performance counter category instance, or an empty string (""), if the category contains a single instance. 
                _trafficReceivedCounters[i] = new PerformanceCounter("Network Interface", "Bytes Sent/sec", _interfaces[i]);
                _trafficSentCounters[i] = new PerformanceCounter("Network Interface", "Bytes Sent/sec", _interfaces[i]);
            }

            NodeName = Dns.GetHostName(); // Get the local computer host name.

            //Get the local computer IP number
            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    IpNumber = ip.ToString();

            // List of all names of the network interfaces
            for (var i = 0; i < length; i++)
                Console.WriteLine("Name netInterface: {0}", _performanceNetCounterCategory.GetInstanceNames()[i]);
        }

        public void GetProcessorCpuTime()
        {
            var tmp = _cpuProcessorTime.NextValue();
            CpuProcessorTime = (float)Math.Round(tmp, 1);
            // Environment.ProcessorCount: return the total number of cores
            //            CPUProcessorTime = cpuProcessorTime.NextValue() / Environment.ProcessorCount;
        }

        public void GetCpuPrivilegedTime()
        {
            var tmp = _cpuPrivilegedTime.NextValue();
            CpuPrivilegedTime = (float)Math.Round(tmp, 1);
        }

        public void GetCpuinterruptTime()
        {
            var tmp = _cpuInterruptTime.NextValue();
            CpuInterruptTime = (float)Math.Round(tmp, 1);
        }

        public void GetcpuDpcTime()
        {
            var tmp = _cpuDpcTime.NextValue();
            CpudpcTime = (float)Math.Round(tmp, 1);
        }

        public void GetPageFile()
        {
            PageFile = _pageFile.NextValue();
        }

        public void GetProcessorQueueLengh()
        {
            ProcessorQueueLengh = _processorQueueLengh.NextValue();
        }

        public void GetMemAvailable()
        {
            MemAvailable = _memAvailable.NextValue();
        }

        public void GetMemCommited()
        {
            MemCommited = _memCommited.NextValue() / (1024 * 1024);
        }

        public void GetMemCommitLimit()
        {
            MemCommitLimit = _memCommitLimit.NextValue() / (1024 * 1024);
        }

        public void GetMemCommitedPerc()
        {
            var tmp = _memCommitedPerc.NextValue();
            // return the value of Memory Commit Limit
            MemCommitedPerc = (float)Math.Round(tmp, 1);
        }

        public void GetMemPoolPaged()
        {
            var tmp = _memPollPaged.NextValue() / (1024 * 1024);
            MemPoolPaged = (float)Math.Round(tmp, 1);
        }

        public void GetMemPoolNonPaged()
        {
            var tmp = _memPollNonPaged.NextValue() / (1024 * 1024);
            MemPoolNonPaged = (float)Math.Round(tmp, 1);
        }

        public void GetMemCachedBytes()
        {
            // return the value of Memory Cached in MBytes
            MemCached = _memCached.NextValue() / (1024 * 1024);
        }

        public void GetDiskQueueLengh()
        {
            DiscQueueLengh = _diskQueueLengh.NextValue();
        }

        public void GetDiskRead()
        {
            var tmp = _diskRead.NextValue() / 1024;
            DiskRead = (float)Math.Round(tmp, 1);
        }

        public void GetDiskWrite()
        {
            var tmp = _diskWrite.NextValue() / 1024;
            // round 1 digit decimal
            DiskWrite = (float)Math.Round(tmp, 1);
        }

        public void GetDiskAverageTimeRead()
        {
            var tmp = _diskAverageTimeRead.NextValue() * 1000;
            // round 1 digit decimal
            DiskAverageTimeRead = (float)Math.Round(tmp, 1);
        }

        public void GetDiskAverageTimeWrite()
        {
            var tmp = _diskAverageTimeWrite.NextValue() * 1000;
            // round 1 digit decimal
            DiskAverageTimeWrite = (float)Math.Round(tmp, 1);
        }

        public void GetDiskTime()
        {
            var tmp = _diskTime.NextValue();
            DiskTime = (float)Math.Round(tmp, 1);
        }


        public void GetHandleCountCounter()
        {
            HandleCountCounter = _handleCountCounter.NextValue();
        }

        public void GetThreadCount()
        {
            ThreadCount = _threadCount.NextValue();
        }

        public void GetContentSwitches()
        {
            // convert to integer
            ContentSwitches = (int)Math.Ceiling(_contentSwitches.NextValue());
        }

        public void GetsystemCalls()
        {
            // convert to integer
            SystemCalls = (int)Math.Ceiling(_systemCalls.NextValue());
        }

        public void GetCurretTrafficSent()
        {
            var length = _interfaces.Length;
            var sendSum = 0.0F;

            for (var i = 0; i < length; i++)
                sendSum += _trafficSentCounters[i].NextValue();
            var tmp = 8 * (sendSum / 1024);
            NetTrafficSend = (float)Math.Round(tmp, 1);
        }

        public void GetCurretTrafficReceived()
        {
            var length = _interfaces.Length;
            var receiveSum = 0.0F;

            for (var i = 0; i < length; i++)
                receiveSum += _trafficReceivedCounters[i].NextValue();

            var tmp = 8 * (receiveSum / 1024);
            NetTrafficReceive = (float)Math.Round(tmp, 1);
        }

        public void GetSampleTime()
        {
            SamplingTime = DateTime.Now;
        }
    }
}