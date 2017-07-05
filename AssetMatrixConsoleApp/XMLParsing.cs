using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Diagnostics;

namespace AssetMatrixConsoleApp
{
    public class XMLParsing
    {
        public XMLParsing()
        {

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

    public class ElementItemDataParsing : XMLParsing
    {
        private SettingsItemData _SettingItemData;

        public ElementItemDataParsing(SettingsItemData settingItemData)
        {
            _SettingItemData = settingItemData;
        }

        public override List<ItemDataClass> ParseXML()
        {
            List<ItemDataClass> itemDataClass = new List<ItemDataClass>();
            foreach(string simulation in _SettingItemData.Simulations)
            {
                string xmlURL = _SettingItemData.Url + simulation;

                List<string> elementList = new List<string>();
                using (XmlTextReader xmlReader = new XmlTextReader(xmlURL))
                {
                    while (xmlReader.Read())
                    {
                        if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "Element")
                        {
                            if(xmlReader.GetAttribute("SourceId") != "" && xmlReader.GetAttribute("SourceId") != null)
                            {
                                string sourceId = xmlReader.GetAttribute("SourceId").ToLower();
                                Debug.WriteLine(sourceId);
                                elementList.Add(sourceId);
                            }
                            
                        }
                    }

                    SimulationItemData simulationData = new SimulationItemData(simulation, elementList);
                    itemDataClass.Add(simulationData);
                }
            }
            return itemDataClass;
        }
    }
}
