using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml;
using System.Diagnostics;

namespace AssetMatrixConsoleApp
{
    public class XMLParsingEventArgs : EventArgs
    {
        public List<ItemDataClass> _ItemDataClass { get; set; }

        public XMLParsingEventArgs(List<ItemDataClass> itemDataClass)
        {
            _ItemDataClass = itemDataClass;
        }
    }

    public delegate void XMLEvent(object sender, XMLParsingEventArgs args);

    public class XMLParsing
    {
        protected BackgroundWorker _BackgroundWorker;
        public event XMLEvent xmlEvent;

        public XMLParsing()
        {
            InitializeBackgroundWorker();
        }

        protected void OnXMLEvent(List<ItemDataClass> itemDataList)
        {
            XMLParsingEventArgs xmlargs = new XMLParsingEventArgs(itemDataList);

            if(xmlEvent != null)
            {
                xmlEvent(this, xmlargs);
            }
        }

        private void InitializeBackgroundWorker()
        {
            try
            {
                _BackgroundWorker = new BackgroundWorker();
                _BackgroundWorker.DoWork += new DoWorkEventHandler(BackgroundWorkerDoWork);
                _BackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundWorkerRunWorkerCompleted);
                _BackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(BackgroundWorkerProgressChanged);
                _BackgroundWorker.WorkerReportsProgress = true;
                _BackgroundWorker.WorkerSupportsCancellation = true;
            }
            catch(NullReferenceException e)
            {
                Debug.WriteLine(e);
            }
            
        }

        protected virtual void BackgroundWorkerDoWork(object sender, DoWorkEventArgs args)
        {

        }

        protected virtual void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
        {

        }

        protected virtual void BackgroundWorkerProgressChanged(object sender, ProgressChangedEventArgs args)
        {

        }

        public void StartParsing()
        {
            if(!_BackgroundWorker.IsBusy)
            {
                _BackgroundWorker.RunWorkerAsync();
            }
        }

        public void CancelParsing()
        {
            if(_BackgroundWorker.WorkerSupportsCancellation)
            {
                _BackgroundWorker.CancelAsync();
            }
        }

        public virtual List<ItemDataClass> ParseXML()
        {
            return null;
        }

    }

    public class SettingsParsing:XMLParsing
    {
        private string _url;

        public SettingsParsing(string url)
        {
            _url = url;
        }

        protected override void BackgroundWorkerDoWork(object sender, DoWorkEventArgs args)
        {
            List<ItemDataClass> itemDataClass = new List<ItemDataClass>();
            string xmlURL = _url;

            using (XmlTextReader xmlReader = new XmlTextReader(xmlURL))
            {
                string url = "";
                List<string> simulationList = new List<string>();
                
                while(xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element)
                    {
                        switch (xmlReader.Name)
                        {
                            case "url":
                                url = xmlReader.ReadInnerXml();
                                break;
                            case "simulation":
                                simulationList.Add(xmlReader.ReadInnerXml());
                                break;
                        }
                    }
                }

                SettingsItemData settingItemData = new SettingsItemData(url, simulationList);
                itemDataClass.Add(settingItemData);

                args.Result = itemDataClass;
            }
        }

        protected override void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            if(args.Error != null)
            {

            }
            else if (args.Cancelled)
            {

            }
            else
            {
                List<ItemDataClass> itemDataList = args.Result as List<ItemDataClass>;
                OnXMLEvent(itemDataList);
            }
        }


        public override List<ItemDataClass> ParseXML()
        {
            List<ItemDataClass> itemDataClass = new List<ItemDataClass>();
            string xmlUrl = _url;
            string xmlURL = _url;
            using (XmlTextReader xmlReader = new XmlTextReader(xmlURL))
            {
                string url = "";
                List<string> simulationList = new List<string>();

                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element)
                    {
                        switch (xmlReader.Name)
                        {
                            case "url":
                                url = xmlReader.ReadInnerXml();
                                break;
                            case "simulation":
                                simulationList.Add(xmlReader.ReadInnerXml());
                                break;
                        }
                    }
                }

                SettingsItemData settingItemData = new SettingsItemData(url, simulationList);
                itemDataClass.Add(settingItemData);
            }

            return itemDataClass;
        }
    }

    public class SimulationThread
    {
        private int _threadCount;
        private string _XMLurl;
        private ManualResetEvent _DoneEvent;
        public string SimulationName { get; set; }
        public List<string> ElementList { get; set; }

        public SimulationThread(int i, string simulationName, string xmlURL ,ManualResetEvent doneEvent)
        {
            _threadCount = i;
            _DoneEvent = doneEvent;
            _XMLurl = xmlURL;
            SimulationName = simulationName;
        }

        public void ThreadPoolCallback(Object ThreadContext)
        {
            int threadIndex = (int)ThreadContext;
            ElementList = GetXMLParser(_XMLurl);
            _DoneEvent.Set();
        }

        private List<string> GetXMLParser(string xmlURL)
        {
            List<string> elementList = new List<string>();

            using (XmlTextReader xmlReader = new XmlTextReader(xmlURL))
            {
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "Element")
                    {
                        if (xmlReader.GetAttribute("SourceId") != "" && xmlReader.GetAttribute("SourceId") != null)
                        {
                            string sourceId = xmlReader.GetAttribute("SourceId").ToLower();
                            Debug.WriteLine(sourceId);
                            elementList.Add(sourceId);
                        }
                    }
                }
            }

            return elementList;
        }
    }

    public class ElementItemDataParsing : XMLParsing
    {
        private const int MAX_BATCH = 64;
        private int threadWorkBatch = 1;
        private SettingsItemData _SettingItemData;

        public ElementItemDataParsing(SettingsItemData settingItemData)
        {
            _SettingItemData = settingItemData;
        }
        

        protected override void BackgroundWorkerDoWork(object sender, DoWorkEventArgs args)
        {
            List<ItemDataClass> itemDataClass = new List<ItemDataClass>();
            

            int simulationCount = _SettingItemData.Simulations.Count;
            int batchCount = (simulationCount / ((MAX_BATCH))) + 1;
            Debug.WriteLine(batchCount +" :: "+ simulationCount);
            for (int i = 0; i < batchCount; i++)
            {
                int modCount = (simulationCount % MAX_BATCH);
                int maxBatchCount = (i + 1) * MAX_BATCH < simulationCount ? MAX_BATCH : modCount;

                ManualResetEvent[] manualResetEvents = new ManualResetEvent[maxBatchCount];
                SimulationThread[] simulationThreads = new SimulationThread[maxBatchCount];
                
                for (int j = 0; j < maxBatchCount; j++)
                {
                    string xmlURL = _SettingItemData.Url + _SettingItemData.Simulations[(i * MAX_BATCH) + j];
                    Debug.WriteLine(xmlURL);
                    manualResetEvents[j] = new ManualResetEvent(false);
                    SimulationThread simulationThread = new SimulationThread(j, _SettingItemData.Simulations[(i * MAX_BATCH) + j], xmlURL, manualResetEvents[j]);
                    simulationThreads[j] = simulationThread;
                    ThreadPool.QueueUserWorkItem(simulationThreads[j].ThreadPoolCallback, j);
                }

                WaitHandle.WaitAll(manualResetEvents);
                Debug.WriteLine("All Calculation are complete....");

                for (int j = 0; j < maxBatchCount; j++)
                {
                    SimulationThread simulationThread = simulationThreads[j];
                    List<string> elementList = simulationThread.ElementList;
                    string simulationName = simulationThread.SimulationName;

                    SimulationItemData simulationData = new SimulationItemData(simulationName, elementList);
                    itemDataClass.Add(simulationData);
                }
            }

            args.Result = itemDataClass;
        }

        private int GeneratedBatchNumber
        {
            get
            {
                return 0;
            }
            
        }

        protected override void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            if (args.Error != null)
            {

            }
            else if (args.Cancelled)
            {

            }
            else
            {
                List<ItemDataClass> itemDataList = args.Result as List<ItemDataClass>;
                OnXMLEvent(itemDataList);
            }
        }
    }
}
