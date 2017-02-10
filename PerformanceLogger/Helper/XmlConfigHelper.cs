using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace PerformanceLogger.Helper
{
    internal class XmlConfigHelper
    {
        // set variables to default values
        // public static string FolderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        public static string FileNameLog = "Log.txt";
        public static string FolderPath = @"Log\";
        public static string WriteToLogFile = "true";
        public static string MaxNumberRecordsInLogFile = "50";
        public static string PollingSysParamsInterval = "10";
        public static string MonitoredProcesses = "chrome;firefox;iexplore";


        // CreateXML: Write an XML file with paramenters to control 
        //  FolderPath               : log file folder
        //  FileName                 : log file name 
        //  WriteToLogFile           : flag to control wrting operation ("true" to enable file log; "false" to disable file log) 
        //  MaxNumberRecordsInLogFile: total number of recrod in log file
        //  PollingSysParamsInterval : sampling interval
        //  MonitoredProcesses       : local computer processes to be monitored
        public static void CreateXML(string strXMLFilename)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                NewLineOnAttributes = true
            };

            try
            {
                using (var writer = XmlWriter.Create(strXMLFilename, settings))
                {
                    writer.WriteStartElement("AppValues");
                    writer.WriteElementString("FolderPath", FolderPath);
                    writer.WriteElementString("FileName", FileNameLog);
                    writer.WriteElementString("WriteToLogFile", WriteToLogFile);
                    writer.WriteElementString("MaxNumberRecordsInLogFile", MaxNumberRecordsInLogFile);
                    writer.WriteElementString("PollingSysParamsInterval", PollingSysParamsInterval);
                    writer.WriteElementString("MonitoredProcesses", MonitoredProcesses);
                    writer.WriteEndElement();
                    writer.Flush();
                    writer.Close();
                }
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine("\nMessage ---\n{0}", ex.Message);
                Console.WriteLine("\nHelpLink ---\n{0}", ex.HelpLink);
                Console.WriteLine("\nSource ---\n{0}", ex.Source);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("The writer is closed");
                Console.WriteLine("\nMessage ---\n{0}", ex.Message);
                Console.WriteLine("\nHelpLink ---\n{0}", ex.HelpLink);
                Console.WriteLine("\nSource ---\n{0}", ex.Source);
            }
            catch (EncoderFallbackException ex)
            {
                Console.WriteLine("There is a character in the buffer that is a valid XML character");
                Console.WriteLine("\nMessage ---\n{0}", ex.Message);
                Console.WriteLine("\nHelpLink ---\n{0}", ex.HelpLink);
                Console.WriteLine("\nSource ---\n{0}", ex.Source);
            }
        }
        
        public static void CheckXML(string strXMLFilename)
        {
            string[] arr =
            {
                "FolderPath", "FileName", "WriteToLogFile", "MaxNumberRecordsInLogFile",
                "PollingSysParamsInterval", "MonitoredProcesses"
            };
            var dictionary = new Dictionary<string, string>();

            // XmlDocument reads an XML from string or from file
            var xmlDoc = new XmlDocument();
            try
            {
                dictionary.Add("FolderPath", FolderPath);
                dictionary.Add("FileName", FileNameLog);
                dictionary.Add("WriteToLogFile", WriteToLogFile);
                dictionary.Add("MaxNumberRecordsInLogFile", MaxNumberRecordsInLogFile);
                dictionary.Add("PollingSysParamsInterval", PollingSysParamsInterval);
                dictionary.Add("MonitoredProcesses", MonitoredProcesses);

                xmlDoc.Load(strXMLFilename);

                if (xmlDoc == null) throw new InvalidOperationException("config file doesn't exist!");

                foreach (var s in arr)
                {
                    string s1, s2;

                    if (xmlDoc.SelectSingleNode("AppValues").SelectSingleNode(s) != null)
                    {
                        s1 = xmlDoc.SelectSingleNode("AppValues").SelectSingleNode(s).Name;
                        s2 = xmlDoc.SelectSingleNode("AppValues").SelectSingleNode(s).InnerText;
                        Console.WriteLine("Element: {0}, Value: {1}", s1, s2);
                    }
                    else
                    {
                        var elem = xmlDoc.CreateElement(s);
                        var text = xmlDoc.CreateTextNode(dictionary[s]);
                        xmlDoc.DocumentElement.AppendChild(elem);
                        xmlDoc.DocumentElement.LastChild.AppendChild(text);
                        xmlDoc.Save(strXMLFilename);
                        Console.WriteLine("Creante new Element: {0}, Value: {1}", s, dictionary[s]);
                    }
                }
            }
            catch (XmlException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Exception object Line, pos: (" + ex.LineNumber + "," + ex.LinePosition + ")");
                Console.WriteLine("Exception source URI: (" + ex.SourceUri + ")");
            }
            catch (FileNotFoundException ex)
            {
                // Put the more specific exception first.
                Console.WriteLine(ex.ToString());
            }
            catch (IOException ex)
            {
                // Put the less specific exception last.
                Console.WriteLine(ex.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        
        public static void ReadXML(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    // XmlDocument to read an XML from string or from file
                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(fileName);
                    if (xmlDoc == null) throw new InvalidOperationException("config file doesn't exist!");

                    foreach (XmlNode node in xmlDoc.SelectSingleNode("//AppValues"))
                        switch (node.Name)
                        {
                            case "FolderPath":
                                Console.WriteLine("FolderPath: {0}", node.InnerText);
                                FolderPath = node.InnerText.Trim();
                                break;
                            case "FileName":
                                Console.WriteLine("FileName: {0}", node.InnerText);
                                FileNameLog = node.InnerText;
                                break;
                            case "WriteToLogFile":
                                Console.WriteLine("WriteToLogFile: {0}", node.InnerText);
                                WriteToLogFile = node.InnerText.Trim();
                                break;
                            case "MaxNumberRecordsInLogFile":
                                Console.WriteLine("MaxNumberRecordsInLogFile: {0}", node.InnerText);
                                MaxNumberRecordsInLogFile = node.InnerText.Trim();
                                break;
                            case "PollingSysParamsInterval":
                                Console.WriteLine("PollingSysParamsInterval: {0}", node.InnerText);
                                PollingSysParamsInterval = node.InnerText.Trim();
                                break;
                            case "MonitoredProcesses":
                                Console.WriteLine("MonitoredProcesses: {0}", node.InnerText);
                                MonitoredProcesses = node.InnerText.Trim();
                                break;
                        }
                }
            }
            catch (XmlException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Exception object Line, pos: (" + ex.LineNumber + "," + ex.LinePosition + ")");
                Console.WriteLine("Exception source URI: (" + ex.SourceUri + ")");
            }
            catch (FileNotFoundException ex)
            {
                // Put the more specific exception first.
                Console.WriteLine(ex.ToString());
            }
            catch (IOException ex)
            {
                // Put the less specific exception last.
                Console.WriteLine(ex.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static bool CheckRootNode(string strXMLFilename)
        {
            var CheckRootNode = false;
            if (File.Exists(strXMLFilename))
            {
                var reader = new XmlTextReader(strXMLFilename);
                reader.WhitespaceHandling = WhitespaceHandling.None;

                while (reader.Read())
                    if (reader.NodeType == XmlNodeType.Element)
                        if (reader.Name == "AppValues")
                        {
                            Console.WriteLine(reader.Name);
                            CheckRootNode = true;
                        }
                reader.Close();
                reader.Dispose();
            }
            return CheckRootNode;
        }
    }
}