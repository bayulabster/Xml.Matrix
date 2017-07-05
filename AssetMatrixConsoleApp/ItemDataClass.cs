using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace AssetMatrixConsoleApp
{
    public abstract class ItemDataClass
    {
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
                foreach(string simulation in Simulations)
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
