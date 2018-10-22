using System.Collections.Generic;


namespace AssetsMatrix.Core
{
    public abstract class ItemDataClass
    {
    }

    public class GithubAssetDataClass : ItemDataClass
    {
        public string Content { get; private set; }

        public GithubAssetDataClass(string content)
        {
            Content = content;
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

        public SimulationItemData(string simulationName, List<string> elementList)
        {
            _SimulationName = simulationName;
            _ElementList = elementList;
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
