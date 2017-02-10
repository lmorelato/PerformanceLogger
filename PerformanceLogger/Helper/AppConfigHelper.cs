using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace PerformanceLogger.Helper
{
    internal class AppConfigHelper
    {
        public static string[] Arr =
        {
            "NodeName",
            "IpNumber",
            "CPUProcessorTime",
            "CPUPrivilegedTime",
            "CPUInterruptTime",
            "CPUDPCTime",
            "MEMAvailable",
            "MEMCommited",
            "MEMCommitLimit",
            "MEMCommitedPerc",
            "MEMPoolPaged",
            "MEMPoolNonPaged",
            "MEMCached",
            "PageFile",
            "ProcessorQueueLengh",
            "DISCQueueLengh",
            "DISKRead",
            "DISKWrite",
            "DISKAverageTimeRead",
            "DISKAverageTimeWrite",
            "DISKTime",
            "HANDLECountCounter",
            "THREADCount",
            "CONTENTSwitches",
            "SYSTEMCalls",
            "NumProcess",
            "NetTrafficSend",
            "NetTrafficReceive",
            "SamplingTime",
            "MonitoredProcessesCPUProcessorTime",
            "MonitoredProcessesMEMUsed"
        };

        public static bool ExistsAppConfig()
        {
            return Exists(Assembly.GetEntryAssembly());
        }

        public static bool Exists(Assembly assembly)
        {
            return File.Exists(assembly.Location + ".config");
        }

        public static void CreateAppConfig()
        {
            var loc = Assembly.GetEntryAssembly().Location;

            // <projectname>.exe.config will be the name of the file that will be created, in this case in the same folder as the exe itself.
            // null means that the default encoding will be used which is UTF-8
            var writer = new XmlTextWriter(string.Concat(loc, ".config"), null);
            writer.Formatting = Formatting.Indented;

            // add declaration to the XML: 
            //    <?xml version="1.0"?>
            writer.WriteStartDocument();

            //add the root element to the XML.
            writer.WriteStartElement("configuration");

            //we will add a node called profile
            writer.WriteStartElement("appSettings");
            //  Close "appSettings" node
            writer.WriteEndElement();

            //  Close "configuration" root element
            writer.WriteEndElement();

            writer.WriteEndDocument();

            //save the file.
            writer.Close();
        }

        public static void WriteAppConfigDefaultValues()
        {
            foreach (var s in Arr)
            {
                Console.WriteLine(s);
                WriteAppConfigElement(s, "true");
            }
        }

        public static void CheckAppConfig()
        {
            try
            {
                // Get the AppSettings section.
                var appSettings = ConfigurationManager.AppSettings;

                // count the no of keys in app settings 
                var cntAppSettingKeys = appSettings.Count;
                if (cntAppSettingKeys == 0)
                    WriteAppConfigDefaultValues();
            }
            catch (ConfigurationErrorsException ex)
            {
                Console.WriteLine("[Exception error: {0}]", ex);
            }
        }

        public static Dictionary<string, bool> ReadSystemAppConfig()
        {
            var dict = new Dictionary<string, bool>();
            try
            {
                // Get the AppSettings section.
                var appSettings = ConfigurationManager.AppSettings;
                if (appSettings.Count == 0)
                    Console.WriteLine("[ReadAppSettings: {0}]", "AppSettings is empty ");

                for (var i = 0; i < appSettings.Count; i++)
                    if (!appSettings.GetKey(i).StartsWith("MonitoredProcesses") && Arr.Contains(appSettings.GetKey(i)))
                        dict.Add(appSettings.GetKey(i),
                            Convert.ToBoolean(ConfigurationManager.AppSettings[appSettings.GetKey(i)]));

                return dict;
            }
            catch (ConfigurationErrorsException ex)
            {
                Console.WriteLine("[Exception error: {0}]", ex);
            }
            return dict;
        }

        public static Dictionary<string, bool> ReadMonitedProcessesAppConfig()
        {
            var dict = new Dictionary<string, bool>();
            try
            {
                // Get the AppSettings section.
                var appSettings = ConfigurationManager.AppSettings;
                if (appSettings.Count == 0)
                    Console.WriteLine("[ReadAppSettings: {0}]", "AppSettings is empty ");

                for (var i = 0; i < appSettings.Count; i++)
                    if (appSettings.GetKey(i).StartsWith("MonitoredProcesses") && Arr.Contains(appSettings.GetKey(i)))
                        dict.Add(appSettings.GetKey(i),
                            Convert.ToBoolean(ConfigurationManager.AppSettings[appSettings.GetKey(i)]));

                return dict;
            }
            catch (ConfigurationErrorsException ex)
            {
                Console.WriteLine("[Exception error: {0}]", ex);
            }
            return dict;
        }
        
        public static void WriteAppConfigElement(string key, string value)
        {
            // load config document for current assembly
            var xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

                // retrieve appSettings node
                var node = xmlDoc.SelectSingleNode("//appSettings");

                if (node == null)
                    throw new InvalidOperationException("appSettings section not found in config file.");

                // select the 'add' element that contains the key
                var elem = (XmlElement) node.SelectSingleNode($"//add[@key='{key}']");

                if (elem != null)
                {
                    // add value for key
                    elem.SetAttribute("value", value);
                }
                else
                {
                    // key was not found so create the 'add' element 
                    // and set it's key/value attributes 
                    elem = xmlDoc.CreateElement("add");
                    elem.SetAttribute("key", key);
                    elem.SetAttribute("value", value);
                    node.AppendChild(elem);
                }
                xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (XmlException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Exception object Line, pos: (" + ex.LineNumber + "," + ex.LinePosition + ")");
                Console.WriteLine("Exception source URI: (" + ex.SourceUri + ")");
            }
        }

        public static void RemoveAppConfigElement(string key)
        {
            // load config document for current assembly
            var xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

                // retrieve appSettings node
                var node = xmlDoc.SelectSingleNode("//appSettings");

                if (node == null)
                    throw new InvalidOperationException("appSettings section not found in config file.");
                // select the 'add' element that contains the key
                var elem = (XmlElement) node.SelectSingleNode($"//add[@key='{key}']");

                if (elem != null)
                {
                    // remove 'add' element with corresponding key
                    node.RemoveChild(node.SelectSingleNode($"//add[@key='{key}']"));
                    xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                }
            }
            catch (XmlException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Exception object Line, pos: (" + ex.LineNumber + "," + ex.LinePosition + ")");
                Console.WriteLine("Exception source URI: (" + ex.SourceUri + ")");
            }
            catch (NullReferenceException e)
            {
                throw new Exception($"The key {key} does not exist.", e);
            }
        }

        //  to modify the application settings value:
        public static void ModifyAppConfigElement(string sKey, string sValue)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            foreach (XmlElement element in xmlDoc.DocumentElement)
                if (element.Name.Equals("appSettings"))
                    foreach (XmlNode node in element.ChildNodes)
                        if (node.Attributes[0].Value.Equals(sKey))
                            node.Attributes[1].Value = sValue;
            xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}