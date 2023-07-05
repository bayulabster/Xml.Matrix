using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;


namespace AssetsMatrix.Core
{
    public abstract class ItemDataClass
    {
    }

    public class GithubCharacterDataClass : ItemDataClass
    {
        public string Content { get; private set; }

        public GithubCharacterDataClass(string content)
        {
            Content = content;
        }
    }

    public class GithubAssetDataClass : ItemDataClass
    {
        public string Content { get; private set; }

        public GithubAssetDataClass(string content)
        {
            Content = content;
        }
    }

    public class GithubAnimationDataClass : ItemDataClass
    {
        public string Content { get; private set; }

        public GithubAnimationDataClass(string content)
        {
            Content = content;
        }
    }

    public class GithubSimulationDataClass : ItemDataClass
    {
        public string XMLURL { get; private set; }
        public string XMLName { get; private set; }
        public List<string> ElementList { get; private set; }

        public GithubSimulationDataClass(string xmlname, List<string> elementList)
        {
            XMLName = xmlname;
            ElementList = elementList;
        }
    }

    public class GithubSimulationDataURL : ItemDataClass
    {
        public string XMLURL { get; private set; }
        public string XMLContents { get; private set; }
        public string SimulationName { get; private set; }

        public GithubSimulationDataURL(string xmlURL, string simulationName)
        {
            XMLURL = xmlURL;
            SimulationName = simulationName;
            XMLContents = null;
        }

        public GithubSimulationDataURL(string xmlURL, string simulationName, string xmlContents)
        {
            XMLURL = xmlURL;
            SimulationName = simulationName;
            XMLContents = XMLContents;
        }
    }

    public class GithubAssetDetailData : ItemDataClass
    {

        public string assetName;
        public string xmlURL;
        public Dictionary<string, string> AssetContentString { get; set; }

        public GithubAssetDetailData()
        {
            AssetContentString = new Dictionary<string, string>();
        }

        public void AddAssetItem(string key, string content)
        {
            Dictionary<string, string> assetContent = new Dictionary<string, string>();
            assetContent.Add(key, content);
        }

    }

    public class GithubAssetDetailTemplateUrl : ItemDataClass
    {
        public string XMLName { get; private set; }
        public string XMLURL { get; private set; }

        public GithubAssetDetailTemplateUrl(string xmlname, string xmlurl)
        {
            XMLName = xmlname;
            XMLURL = xmlurl;
        }
    }

    public class GithubAssetDetailDataTemplate : ItemDataClass
    {
        public string Name { get; private set; }
        public string ImageUrl { get; private set; }
        public string AssetName { get; private set; }

        public GithubAssetDetailDataTemplate(string name, string imageurl, string assetname)
        {
            Name = name;
            ImageUrl = imageurl;
            AssetName = assetname;
        }
    }

    public class AssetsListItemData: ItemDataClass
    {
        public string ScreenShot { get; private set; }
        public string Id { get; private set; }
        public string Templates { get; private set; }
        public string GameObjectid { get; private set; }
        public string Unity3DPackName { get; private set; }
        public string SourceId { get; private set; }
        public string Tooltip { get; private set; }
        public string ExtendedTooltip { get; private set; }
        public string Type { get; private set; }
        public string ImportPath { get; private set; }
        public string AutomatedXML { get; private set; }
        public string ManualXML { get; private set; }
        public string GUIScreen { get; private set; }
        public string MinResolution { get; private set; }
        public string Comment { get; private set; }
        public string Category { get; private set; }
        public string Keyword { get; private set; }

        public AssetsListItemData(string screenShot, string id, string templates, string gameobjectId,string unity3dPackName,
                                string sourceId, string tooltip, string extendedTooltip, string type,
                                string importPath, string automatedXML, string manualXML, string guiscreen, string minResolution, string comment,
                                string category, string keyword)
        {
            ScreenShot = screenShot;
            Id = id;
            Templates = templates;
            GameObjectid = gameobjectId;
            Unity3DPackName = unity3dPackName;
            SourceId = sourceId;
            Tooltip = tooltip;
            ExtendedTooltip = extendedTooltip;
            Type = type;
            ImportPath = importPath;
            AutomatedXML = automatedXML;
            ManualXML = manualXML;
            GUIScreen = guiscreen;
            MinResolution = minResolution;
            Comment = comment;
            Category = category;
            Keyword = keyword;
        }

    }

    public class SettingsItemData : ItemDataClass
    {
        public string Url { get; private set; }
        public List<string> Simulations { get; private set; }

        public SettingsItemData(string url, List<string> simulations)
        {
            Url = url;
            Simulations = simulations;
        }

        public List<string> GetSimulationUrl
        {
            get
            {
                List<string> returnedvalue = new List<string>();
                foreach (string simulation in Simulations)
                {
                    string urlLink = Url + simulation;
                    returnedvalue.Add(urlLink);
                }

                return returnedvalue;
            }
        }
    }

    public class SimulationItemData : ItemDataClass
    {
        private string _SimulationName;
        private List<string> _ElementList;
        private XElement Document;

        public SimulationItemData(string simulationName, string xmlContent)
        {
            _SimulationName = simulationName;
            Document = XDocument.Parse(xmlContent).Root;
        }

        
        public int SearchInXML(string elem)
        {
            int counter = 0;
            string toLowerString = elem.ToLower();
            foreach (var el in Document.Elements().DescendantsAndSelf())
            {
                if (el.Name.LocalName.ToLower() != toLowerString)
                {
                    continue;
                }
                counter++;
            }
            return counter;
        }

        public string GetSimulationName
        {
            get
            {
                return _SimulationName;
            }
        }

        public List<string> GetElementList
        {
            get
            {
                return _ElementList;
            }
        }
    }
}
