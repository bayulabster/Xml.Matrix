using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;

namespace AssetMatrixConsoleApp
{
    struct SimulationStruct
    {
        public string Name;
        public int ElementCount;

        public SimulationStruct(string name, int elementCount)
        {
            Name = name;
            ElementCount = elementCount;
        }

        
    }

    public partial class AssetsMatrixForm : Form
    {
        private SettingsItemData _SettingsData;
        private List<SimulationItemData> _SimulationItemData;

        public AssetsMatrixForm()
        {
            _SimulationItemData = new List<SimulationItemData>();
            InitializeComponent();
            FetchingData();
        }

        private void FetchingData()
        {
            SettingsParsing settings = new SettingsParsing("settings.xml");
            List<ItemDataClass> itemList = settings.ParseXML();
            _SettingsData = itemList[0] as SettingsItemData;
        }

        private void OnFetchData(object sender, EventArgs e)
        {
            label3.Text = "Load data from server";

            ElementItemDataParsing elementItemDataParsing = new ElementItemDataParsing(_SettingsData);
            List<ItemDataClass> elementItemData = elementItemDataParsing.ParseXML();

            foreach (ItemDataClass element in elementItemData)
            {
                SimulationItemData simulationData = element as SimulationItemData;
                _SimulationItemData.Add(simulationData);
            }

            label3.Text = "Data Is Ready";
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void OnTextBoxInput(object sender, KeyEventArgs e)
        {
            listView1.Items.Clear();

            if(e.KeyCode == Keys.Return)
            {
                SearchForGameObject(textBox1);
            }
        }

        private void SearchForGameObject(TextBox textbox)
        {
            TextBox textObj = textbox;
            string textboxString = textObj.Text.ToLower();
            Debug.WriteLine(" the keyword is :: " + textObj.Text.ToLower());
            string[] simulationArray = new string[_SimulationItemData.Count];
            List<SimulationStruct> simulationStructArray = new List<SimulationStruct>();

            //TODO: Ugly, need refactoring
            for (int i = 0; i < _SimulationItemData.Count; i++)
            {
                foreach (string element in _SimulationItemData[i].GetElementList)
                {
                    if (element == textboxString)
                    {
                        simulationArray[i] = _SimulationItemData[i].GetSimulationName;
                        break;
                    }
                }
            }


            //TODO: Ugly, need refactoring
            for (int i = 0; i < _SimulationItemData.Count; i++)
            {
                for (int j = 0; j < simulationArray.Length; j++)
                {
                    if (_SimulationItemData[i].GetSimulationName == simulationArray[j])
                    {
                        int count = 0;
                        string name = _SimulationItemData[i].GetSimulationName;
                        foreach (string element in _SimulationItemData[i].GetElementList)
                        {
                            if (element == textboxString)
                            {
                                count++;
                            }
                        }

                        simulationStructArray.Add(new SimulationStruct(name, count));
                    }
                }
            }

            foreach (SimulationStruct simulation in simulationStructArray)
            {
                string[] valueArray = new string[2];
                valueArray[0] = simulation.Name;
                valueArray[1] = simulation.ElementCount.ToString();
                ListViewItem listViewItem = new ListViewItem(valueArray);
                listViewItem.Name = simulation.Name;
                listView1.Items.Add(listViewItem);
            }

            textObj.Text = "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SearchForGameObject(textBox1);
        }
    }
}
