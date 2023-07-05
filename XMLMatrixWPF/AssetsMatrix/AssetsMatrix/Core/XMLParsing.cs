using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml;
using System.Diagnostics;
using System.Xml.Linq;
using System.Net;
using Octokit;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace AssetsMatrix.Core
{
    public class XMLParsingEventArgs : EventArgs
    {
        public List<ItemDataClass> _ItemDataClass { get; set; }
        
        public XMLParsingEventArgs(List<ItemDataClass> itemDataClass)
        {
            _ItemDataClass = itemDataClass;
        }
    }

    public class XMLProgressEventArgs : EventArgs
    {
        public int Progress { get; set; }

        public XMLProgressEventArgs(int progress)
        {
            Progress = progress;
        }
    }

    public delegate void XMLEvent(object sender, XMLParsingEventArgs args);
    public delegate void XMLProgressEvent(object sender, XMLProgressEventArgs args);
    
        
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

            if (xmlEvent != null)
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
            catch (NullReferenceException e)
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
            if (!_BackgroundWorker.IsBusy)
            {
                _BackgroundWorker.RunWorkerAsync();
            }
        }

        public void CancelParsing()
        {
            if (_BackgroundWorker.WorkerSupportsCancellation)
            {
                _BackgroundWorker.CancelAsync();
            }
        }

        public virtual List<ItemDataClass> ParseXML()
        {
            return null;
        }

    }

    #region GITHUB

    public abstract class GithubDataParsingBase : XMLParsing
    {
        protected GitHubClient _Client;
        protected string _BranchName;
        protected string _RepoName;
        public const string GITHUB_REPO_OWNER = "Livit";

        public GithubDataParsingBase(GitHubClient client, string repoName, string branchName  )
        {
            _Client = client;
            _RepoName = repoName;
            _BranchName = branchName;
        }

        protected override void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            if (args.Error != null)
            {

            }
            else
            {
                List<ItemDataClass> itemDataList = args.Result as List<ItemDataClass>;
                OnXMLEvent(itemDataList);
            }

        }

    }

    public class GithubAssetListDataParsing :GithubDataParsingBase
    {

        public GithubAssetListDataParsing(GitHubClient client, string repoName, string branchName) : base(client, repoName, branchName) { }

        protected override void BackgroundWorkerDoWork(object sender, DoWorkEventArgs args)
        {
            try
            {
                List<string> AssetDataList = GetAssetDataFromGithub(_Client).Result;

                List<ItemDataClass> AssetDataClassList = new List<ItemDataClass>();

                foreach (string s in AssetDataList)
                {
                    var assetData = new GithubAssetDataClass(s);
                    AssetDataClassList.Add(assetData);
                }

                args.Result = AssetDataClassList;

                base.BackgroundWorkerDoWork(sender, args);
            }
            catch(Exception e)
            {
                Debug.WriteLine("GithubAssetListDataParsing_BackroundWorkerDoWork :: " + e.Message + " : " + e.StackTrace);
            }
            
        }

        private async Task<List<string>> GetAssetDataFromGithub(GitHubClient client)
        {
            var file = await client.Repository.Content.GetAllContentsByRef(GITHUB_REPO_OWNER, _RepoName, "Common.txt", _BranchName);

            string returnedString = "";
            foreach (var f in file)
            {
                returnedString = f.Content;
            }


            List<String> ListOfAssets = GetAllAssetsFromGithubText(returnedString);

            return ListOfAssets;
        }

        private List<String> GetAllAssetsFromGithubText(string value)
        {
            List<String> returnedString = new List<string>();

            string returnValue = value;
            string[] stringArray = returnValue.Split('\n');
            foreach (string s in stringArray)
            {
                if(s.Length>0)
                {
                    returnedString.Add(AssetMatrixStaticFunction.ExtractStringFromPath(s));
                }
            }
            return returnedString;
        }

        
        
    }

    public class GithubAnimationListDataParsing : GithubDataParsingBase
    {

        public GithubAnimationListDataParsing(GitHubClient client, string repoName, string branchName) : base(client, repoName, branchName) { }

        protected override void BackgroundWorkerDoWork(object sender, DoWorkEventArgs args)
        {
            try
            {
                List<string> AssetDataList = GetAnimationDataFromGithub(_Client).Result;

                List<ItemDataClass> AssetDataClassList = new List<ItemDataClass>();

                foreach (string s in AssetDataList)
                {
                    var assetData = new GithubAnimationDataClass(s);
                    AssetDataClassList.Add(assetData);
                }

                args.Result = AssetDataClassList;

                base.BackgroundWorkerDoWork(sender, args);
            }
            catch (Exception e)
            {
                Debug.WriteLine("GithubAssetListDataParsing_BackroundWorkerDoWork :: " + e.Message + " : " + e.StackTrace);
            }

        }

        private async Task<List<string>> GetAnimationDataFromGithub(GitHubClient client)
        {
            var file = await client.Repository.Content.GetAllContentsByRef(GITHUB_REPO_OWNER, _RepoName, "Animation.txt", _BranchName);
            var file2 = await client.Repository.Content.GetAllContentsByRef(GITHUB_REPO_OWNER, _RepoName, "Scenes.txt", _BranchName);

            string returnedString = "";
            foreach (var f in file)
            {
                returnedString += f.Content;
            }

            foreach (var f in file2)
            {
                returnedString += f.Content;
            }

            List<String> ListOfAssets = GetAllAnimationsFromGithubText(returnedString);

            return ListOfAssets;
        }

        private List<String> GetAllAnimationsFromGithubText(string value)
        {
            List<String> returnedString = new List<string>();

            string returnValue = value;
            string[] stringArray = returnValue.Split('\n');
            foreach (string s in stringArray)
            {
                if (s.Length > 0)
                {
                    returnedString.Add(AssetMatrixStaticFunction.ExtractStringFromPath(s));
                }
            }
            return returnedString;
        }

    }

    public class GithubCharacterListDataParsing : GithubDataParsingBase
    {

        public GithubCharacterListDataParsing(GitHubClient client, string repoName, string branchName) : base(client, repoName, branchName) { }

        protected override void BackgroundWorkerDoWork(object sender, DoWorkEventArgs args)
        {
            try
            {
                List<string> AssetDataList = GetCharacterDataFromGithub(_Client).Result;

                List<ItemDataClass> AssetDataClassList = new List<ItemDataClass>();

                foreach (string s in AssetDataList)
                {
                    var assetData = new GithubCharacterDataClass(s);
                    AssetDataClassList.Add(assetData);
                }

                args.Result = AssetDataClassList;

                base.BackgroundWorkerDoWork(sender, args);
            }
            catch (Exception e)
            {
                Debug.WriteLine("GithubAssetListDataParsing_BackroundWorkerDoWork :: " + e.Message + " : " + e.StackTrace);
            }

        }

        private async Task<List<string>> GetCharacterDataFromGithub(GitHubClient client)
        {
            var file = await client.Repository.Content.GetAllContentsByRef(GITHUB_REPO_OWNER, _RepoName, "Characters.txt", _BranchName);

            string returnedString = "";
            foreach (var f in file)
            {
                returnedString += f.Content;
            }

            List<String> ListOfAssets = GetAllCharactersFromGithubText(returnedString);

            return ListOfAssets;
        }

        private List<String> GetAllCharactersFromGithubText(string value)
        {
            List<String> returnedString = new List<string>();

            string returnValue = value;
            string[] stringArray = returnValue.Split('\n');
            foreach (string s in stringArray)
            {
                if (s.Length > 0)
                {
                    returnedString.Add(AssetMatrixStaticFunction.ExtractStringFromPath(s));
                }
            }
            return returnedString;
        }

    }

    public class GithubSimulationListDataParsing : GithubDataParsingBase
    {
        private const int MAX_BATCH = 64;

        public GithubSimulationListDataParsing(GitHubClient client, string repoName, string branchName) : base(client, repoName, branchName) { }

        protected override void BackgroundWorkerDoWork(object sender, DoWorkEventArgs args)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            ProcessGithubSimulationData(worker, args);
            base.BackgroundWorkerDoWork(sender, args);
            
        }

        private void ProcessGithubSimulationData(BackgroundWorker worker, DoWorkEventArgs args)
        {
            try
            {
                List<ItemDataClass> itemDataClass = new List<ItemDataClass>();
                List<GithubSimulationDataURL> simulationsList =  GetSimulationDataFromGithub().Result;
                int simulationsCount = simulationsList.Count;

                SimulationThread[] simulationThreads = new SimulationThread[simulationsCount];
                for (int i = 0; i < simulationsCount; i++)
                {
                    SimulationThread simulationThread = new SimulationThread(i, simulationsList[i].SimulationName, simulationsList[i].XMLURL);
                    simulationThreads[i] = simulationThread;

                    string simulationName = simulationThread.SimulationName;

                    SimulationItemData simulationData = new SimulationItemData(simulationName, simulationThread.XmlString);
                    itemDataClass.Add(simulationData);
                }

                Debug.WriteLine("All Calculation are complete....");

                args.Result = itemDataClass;
            }
            catch(Exception e)
            {
                Debug.WriteLine("Process Simulation Error :: " + e.Message + " : " + e.StackTrace);
            }
           
        }

        private async Task<List<GithubSimulationDataURL>>  GetSimulationDataFromGithub()
        {

            var files = await _Client.Repository.Content.GetArchive(GITHUB_REPO_OWNER, _RepoName, ArchiveFormat.Zipball, _BranchName);
            

            List<GithubSimulationDataURL> githubList = new List<GithubSimulationDataURL>();

            using (MemoryStream memoryStream = new MemoryStream(files))
            {
                using (var zipInputStream = new ZipInputStream(memoryStream))
                {
                    ZipEntry zipEntry;
                    while((zipEntry = zipInputStream.GetNextEntry()) != null)
                    {
                        if (zipEntry.Name.Contains("Engine") && zipEntry.IsFile)
                        {
                            var simulationName = AssetMatrixStaticFunction.ExtractStringFromPath(zipEntry.Name);
                            StreamReader reader = new StreamReader(zipInputStream);
                            var read = reader.ReadToEnd();
                            githubList.Add(new GithubSimulationDataURL(read, simulationName));
                        }
                    }
                }
            }
            
            return githubList;
        }

        protected override void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            if (args.Error != null)
            {

            }
            else
            {
                List<ItemDataClass> itemDataList = args.Result as List<ItemDataClass>;
                OnXMLEvent(itemDataList);
            }
        }
    }

    public class GithubAssetDetailDataParsing:GithubDataParsingBase
    {
        private string _xmlName;
        private const int MAX_BATCH = 32;

        public GithubAssetDetailDataParsing(GitHubClient client, string repoName, string branchName, string xmlname) : base(client, repoName, branchName) {

            _xmlName = xmlname;

        }


        protected override void BackgroundWorkerDoWork(object sender, DoWorkEventArgs args)
        {
            try
            {
                BackgroundWorker worker = (BackgroundWorker)sender;

                List<ItemDataClass> itemDataClass = new List<ItemDataClass>();
                List<GithubAssetDetailData> assetDetailsList = GetAssetDetailsData(_Client).Result;
                int assetdetailCount = assetDetailsList.Count;

                if (assetdetailCount <= 0) return;
                int batchCount = (assetdetailCount / ((MAX_BATCH))) + 1;
                Debug.WriteLine(batchCount + " :: " + assetdetailCount);
                for (int i = 0; i < batchCount; i++)
                {
                    int modCount = (assetdetailCount % MAX_BATCH);
                    int maxBatchCount = (i + 1) * MAX_BATCH < assetdetailCount ? MAX_BATCH : modCount;

                    ManualResetEvent[] manualResetEvents = new ManualResetEvent[maxBatchCount];
                    AssetDetailDataThread[] assetThread = new AssetDetailDataThread[maxBatchCount];

                    for (int j = 0; j < maxBatchCount; j++)
                    {
                        int percentage = ((i * MAX_BATCH) + j) / assetdetailCount;
                        worker.ReportProgress(percentage);
                        manualResetEvents[j] = new ManualResetEvent(false);
                        AssetDetailDataThread simulationThread = new AssetDetailDataThread(j, assetDetailsList[(i * MAX_BATCH) + j].assetName, assetDetailsList[(i * MAX_BATCH) + j].xmlURL, manualResetEvents[j]);
                        assetThread[j] = simulationThread;
                        ThreadPool.QueueUserWorkItem(assetThread[j].ThreadPoolCallback, j);
                    }

                    WaitHandle.WaitAll(manualResetEvents);
                    Debug.WriteLine("All Calculation are complete....");

                    for (int j = 0; j < maxBatchCount; j++)
                    {
                        AssetDetailDataThread asset = assetThread[j];
                        Dictionary<string, string> content = asset.Content;
                        string assetName = asset.AssetName;

                        GithubAssetDetailData assetData = new GithubAssetDetailData();
                        assetData.assetName = assetName;
                        assetData.AssetContentString = content;
                        
                        itemDataClass.Add(assetData);
                    }

                    args.Result = itemDataClass;
                }

                base.BackgroundWorkerDoWork(sender, args);
            }
            catch(Exception e)
            {
                Debug.WriteLine("error in asset details :: " + e.Message + "\n" + e.StackTrace);
            }

            

            
        }

        private async Task<List<GithubAssetDetailData>> GetAssetDetailsData(GitHubClient client)
        {
            List<GithubAssetDetailData> listData = new List<GithubAssetDetailData>();

            var files = await client.Repository.Content.GetAllContents(GITHUB_REPO_OWNER, _RepoName);

            foreach(var file in files)
            {
                if(file.Name.Contains(".xml"))
                {
                    GithubAssetDetailData data = new GithubAssetDetailData();
                    data.assetName = file.Name;
                    data.xmlURL = file.DownloadUrl;

                    listData.Add(data);
                }
            }

            return listData;
        }

       
    }

    public class GithubAssetDetailTemplateDataParsing : GithubDataParsingBase
    {
        private const int MAX_BATCH = 64;

        public GithubAssetDetailTemplateDataParsing(GitHubClient client, string repoName, string branchName) : base(client, repoName, branchName)
        {


        }


        protected override void BackgroundWorkerDoWork(object sender, DoWorkEventArgs args)
        {
            try
            {
                BackgroundWorker worker = (BackgroundWorker)sender;

                List<ItemDataClass> itemDataClass = new List<ItemDataClass>();
                List<GithubAssetDetailTemplateUrl> assetDetailsList = GetAssetTemplateData(_Client).Result;
                int assetdetailCount = assetDetailsList.Count;

                if (assetdetailCount <= 0) return;
                int batchCount = (assetdetailCount / ((MAX_BATCH))) + 1;
                Debug.WriteLine(batchCount + " :: " + assetdetailCount);
                for (int i = 0; i < batchCount; i++)
                {
                    int modCount = (assetdetailCount % MAX_BATCH);
                    int maxBatchCount = (i + 1) * MAX_BATCH < assetdetailCount ? MAX_BATCH : modCount;

                    ManualResetEvent[] manualResetEvents = new ManualResetEvent[maxBatchCount];
                    AssetDetailTemplateThread[] assetThread = new AssetDetailTemplateThread[maxBatchCount];

                    for (int j = 0; j < maxBatchCount; j++)
                    {
                        int percentage = ((i * MAX_BATCH) + j) / assetdetailCount;
                        worker.ReportProgress(percentage);
                        manualResetEvents[j] = new ManualResetEvent(false);
                        AssetDetailTemplateThread simulationThread = new AssetDetailTemplateThread(j, assetDetailsList[(i * MAX_BATCH) + j].XMLName, assetDetailsList[(i * MAX_BATCH) + j].XMLURL, manualResetEvents[j]);
                        assetThread[j] = simulationThread;
                        ThreadPool.QueueUserWorkItem(assetThread[j].ThreadPoolCallback, j);
                    }

                    WaitHandle.WaitAll(manualResetEvents);
                    Debug.WriteLine("All Calculation are complete....");

                    for (int j = 0; j < maxBatchCount; j++)
                    {
                        AssetDetailTemplateThread asset = assetThread[j];
                        itemDataClass.Add(asset.Content);
                    }

                 
                }

                args.Result = itemDataClass;

                base.BackgroundWorkerDoWork(sender, args);
            }
            catch (Exception e)
            {
                Debug.WriteLine("error in asset details template :: " + e.Message + "\n" + e.StackTrace);
            }




        }

        private async Task<List<GithubAssetDetailTemplateUrl>> GetAssetTemplateData(GitHubClient client)
        {
            List<GithubAssetDetailTemplateUrl> listData = new List<GithubAssetDetailTemplateUrl>();

            var files = await client.Repository.Content.GetAllContents(GITHUB_REPO_OWNER, _RepoName);

            foreach (var file in files)
            {
                if (file.Name.Contains(".xml"))
                {
                    GithubAssetDetailTemplateUrl data = new GithubAssetDetailTemplateUrl(file.Name, file.DownloadUrl);
                    listData.Add(data);
                }
            }

            return listData;
        }


    }

    #endregion

    #region WEB_URL

    public class SettingsParsing : XMLParsing
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

                args.Result = itemDataClass;
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

    public class AssetsListParsing :XMLParsing
    {
        private string _URL;

        public AssetsListParsing(string url)
        {
            _URL = url;
            Debug.WriteLine("fewifjweofjoeifewo");
        }

        protected override void BackgroundWorkerDoWork(object sender, DoWorkEventArgs args)
        {
            List<ItemDataClass> itemDataClass = new List<ItemDataClass>();

            Debug.WriteLine("BackgroundWorker AssetsListParsing");
            Debug.WriteLine(_URL);
            using (XmlTextReader xmlReader = new XmlTextReader(_URL))
            {
                
                while (xmlReader.Read())
                {
                    if(xmlReader.NodeType == XmlNodeType.Element)
                    {
                        if (xmlReader.Name == "row")
                        {
                            string gameObjectId = "";
                            string unity3dPackName = "";
                            string sourceId = "";
                            string tooltip = "";
                            string extendedTooltip = "";
                            string type = "";
                            string importPath = "";
                            string automatedXML = "";

                            XElement elem = XNode.ReadFrom(xmlReader) as XElement;
                            if(elem != null)
                            {
                                gameObjectId = elem.Element("E").Value;
                                unity3dPackName = elem.Element("F").Value;
                                sourceId = elem.Element("G").Value;
                                tooltip = elem.Element("H").Value;
                                extendedTooltip = elem.Element("I").Value;
                                type = elem.Element("J").Value;
                                importPath = elem.Element("K").Value;
                                automatedXML = elem.Element("L").Value;

                            }
                                AssetsListItemData assetItemData = new AssetsListItemData("", "", "", gameObjectId, unity3dPackName, sourceId, tooltip, extendedTooltip,
                                                                                type, importPath, automatedXML, "", "", "", "", "", "");
                                itemDataClass.Add(assetItemData);
                        }
                    }
                }
                
            }

            args.Result = itemDataClass;
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

    public class SimulationThread
    {
        private int _threadCount;
        public string SimulationName { get; set; }
        public string XmlString { get; set; }

    public SimulationThread(int i, string simulationName, string xmlURL)
        {
            _threadCount = i;
            XmlString = xmlURL;
            SimulationName = simulationName;
        }

    }

    public class AssetDetailDataThread
    {
        private int _threadCount;
        private string _XMLurl;
        private ManualResetEvent _DoneEvent;
        public string AssetName { get; set; }
        public Dictionary<string,string> Content { get; set; }

        public AssetDetailDataThread(int i, string assetName, string xmlURL, ManualResetEvent doneEvent)
        {
            _threadCount = i;
            _DoneEvent = doneEvent;
            _XMLurl = xmlURL;
            AssetName = assetName;
        }

        public void ThreadPoolCallback(Object ThreadContext)
        {
            int threadIndex = (int)ThreadContext;
            Content = GetXMLParser(_XMLurl);
            _DoneEvent.Set();
        }

        private Dictionary<string, string> GetXMLParser(string xmlURL)
        {
            Dictionary<string, string> list = new Dictionary<string, string>();

            try
            {

            using (XmlTextReader reader = new XmlTextReader(xmlURL))
            {
                string key = "";
                string content = "";

                while (reader.Read())
                {
                    
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                            switch (reader.Name)
                            {
                                case "asset_key":
                                    key = reader.ReadInnerXml();
                                    break;
                                case "asset_content":
                                    content = reader.ReadInnerXml();
                                    if(key != "" && content != "")
                                        list.Add(key, content);
                                    break;
                            }
                    }
                    
                }
            }

            }
            catch(Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }


            return list;
        }
    }

    public class AssetDetailTemplateThread
    {
        private int _threadCount;
        private string _XMLurl;
        private ManualResetEvent _DoneEvent;
        public string AssetName { get; set; }
        public GithubAssetDetailDataTemplate Content { get; set; }

        public AssetDetailTemplateThread(int i, string assetName, string xmlURL, ManualResetEvent doneEvent)
        {
            _threadCount = i;
            _DoneEvent = doneEvent;
            _XMLurl = xmlURL;
            AssetName = assetName;
        }

        public void ThreadPoolCallback(Object ThreadContext)
        {
            int threadIndex = (int)ThreadContext;
            Content = GetXMLParser(_XMLurl);
            _DoneEvent.Set();
        }

        private GithubAssetDetailDataTemplate GetXMLParser(string xmlURL)
        {
            string name = "";
            string image = "";

            try
            {
               

                using (XmlTextReader reader = new XmlTextReader(xmlURL))
                {
                    

                    while (reader.Read())
                    {

                        if (reader.NodeType == XmlNodeType.Element && reader.Name == "ElementTemplate" )
                        {
                            if (reader.GetAttribute("Name") != null && reader.GetAttribute("Name") != "")
                                name = reader.GetAttribute("Name");

                            if (reader.GetAttribute("Image") != null && reader.GetAttribute("Image") != "")
                            {
                                image = reader.GetAttribute("Image");
                                return new GithubAssetDetailDataTemplate(name, image, AssetName);
                            }
                                
                        }

                    }

                 
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }

            return null;

        }
    }

    public class ElementItemDataParsing : XMLParsing
    {
        private const int MAX_BATCH = 64;
        private SettingsItemData _SettingItemData;

        public ElementItemDataParsing(SettingsItemData settingItemData)
        {
            _SettingItemData = settingItemData;
        }


        protected override void BackgroundWorkerDoWork(object sender, DoWorkEventArgs args)
        {
            List<ItemDataClass> itemDataClass = new List<ItemDataClass>();

            BackgroundWorker worker = (BackgroundWorker)sender;

            int simulationCount = _SettingItemData.Simulations.Count;
            int batchCount = (simulationCount / ((MAX_BATCH))) + 1;
            Debug.WriteLine(batchCount + " :: " + simulationCount);
            for (int i = 0; i < batchCount; i++)
            {
                int modCount = (simulationCount % MAX_BATCH);
                int maxBatchCount = (i + 1) * MAX_BATCH < simulationCount ? MAX_BATCH : modCount;

                ManualResetEvent[] manualResetEvents = new ManualResetEvent[maxBatchCount];
                SimulationThread[] simulationThreads = new SimulationThread[maxBatchCount];

                for (int j = 0; j < maxBatchCount; j++)
                {
                    int percentage = ((i * MAX_BATCH) + j) / simulationCount;
                    worker.ReportProgress(percentage);
                    string xmlURL = _SettingItemData.Url + _SettingItemData.Simulations[(i * MAX_BATCH) + j];
                    Debug.WriteLine(xmlURL);
                    manualResetEvents[j] = new ManualResetEvent(false);
                    SimulationThread simulationThread = new SimulationThread(j, _SettingItemData.Simulations[(i * MAX_BATCH) + j], xmlURL);
                    simulationThreads[j] = simulationThread;
                }

                WaitHandle.WaitAll(manualResetEvents);
                Debug.WriteLine("All Calculation are complete....");

                for (int j = 0; j < maxBatchCount; j++)
                {
                    SimulationThread simulationThread = simulationThreads[j];
                    string simulationName = simulationThread.SimulationName;

                    SimulationItemData simulationData = new SimulationItemData(simulationName, simulationThread.XmlString);
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

    #endregion
}
