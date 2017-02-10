using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PerformanceLogger.Model
{
    /// <summary>
    /// Monitored local proccess
    /// </summary>
    public class LocalProcess
    {
        private List<PerformanceCounter> _cpuProcessorTime;
        private List<PerformanceCounter> _memUsed;

        public LocalProcess(string processName)
        {
            ProcessName = processName;
            Start();
        }

        public string ProcessName { get; internal set; }
        public string NodeName { get; set; }
        public string IpNumber { get; set; }
        public float CpuProcessorTime { get; set; }
        public float MemUsed { get; set; }

        public void Start()
        {
            _cpuProcessorTime = new List<PerformanceCounter>();
            _memUsed = new List<PerformanceCounter>();

            var arr = Process.GetProcessesByName(ProcessName);

            for (var index = 0; index < arr.Length; index++)
            {
                var process = arr[index];
                try
                {
                    _cpuProcessorTime.Add(index == 0
                        ? new PerformanceCounter("Process", "% Processor Time", process.ProcessName)
                        : new PerformanceCounter("Process", "% Processor Time", process.ProcessName + "#" + index));
                }
                catch (Exception)
                {
                    //ignored
                }

                try
                {
                    _memUsed.Add(index == 0
                        ? new PerformanceCounter("Process", "Working Set", process.ProcessName)
                        : new PerformanceCounter("Process", "Working Set", process.ProcessName + "#" + index));
                }
                catch (Exception)
                {
                    //ignored
                }
            }
        }

        public void GetProcessorCpuTime()
        {
            float sum = 0;
            foreach (var p in _cpuProcessorTime)
                try
                {
                    sum += (float) Math.Round(p.NextValue(), 1);
                }
                catch (Exception)
                {
                    //ignored
                }
            CpuProcessorTime = sum;
        }

        public void GetMemUsed()
        {
            float sum = 0;
            foreach (var p in _memUsed)
                try
                {
                    var value = p.NextValue() / (1024 * 1024);
                    sum += value;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    //ignored
                }

            MemUsed = sum;
        }
    }
}