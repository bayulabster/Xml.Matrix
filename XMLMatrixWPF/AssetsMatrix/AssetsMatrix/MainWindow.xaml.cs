using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AssetsMatrix.Core;
using System.ComponentModel;
using System.Diagnostics;


namespace AssetsMatrix
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

    public class SimulationListObject
    {
        public SimulationListObject(string name, string count)
        {
            Name = name;
            Count = count;
        }

        public string Name { get; set; }
        public string Count { get; set; }
    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SettingsItemData _SettingsData;
        private List<SimulationItemData> _SimulationItemData;
        private List<AssetsListItemData> _AssetsListItemData;
        private BackgroundWorker _BackgroundWorker;
        //private ListViewLabNameSorter _ListViewLabSorter;

        public MainWindow()
        {
            
            _SimulationItemData = new List<SimulationItemData>();
            _AssetsListItemData = new List<AssetsListItemData>();
            //FetchingData();
            FetchingDataAssetList();
            InitializeComponent();

            Debug.WriteLine("main window");

            ajax1_loader_gif.IsEnabled = false;
            ajax1_loader_gif.Visibility = Visibility.Hidden;
            this.Cursor = Cursors.AppStarting;
            textBox.TextChanged += new TextChangedEventHandler(textBox_TextChanged);
        }

        private void FetchingData()
        {
            SettingsParsing settings = new SettingsParsing("settings.xml");
            settings.StartParsing();
            settings.xmlEvent += SettingsOnComplete;
        }

        private void SettingsOnComplete(object sender, XMLParsingEventArgs args)
        {
            List<ItemDataClass> itemList = args._ItemDataClass;
            _SettingsData = itemList[0] as SettingsItemData;

            Debug.WriteLine("Begin fetcing data");

            FetchingElementData();
        }

        private void FetchingDataAssetList()
        {
            AssetsListParsing assetList = new AssetsListParsing("assetslist.xml");
            assetList.StartParsing();
            assetList.xmlEvent += AssetListOnComplete;
        }

        private void AssetListOnComplete(object sender, XMLParsingEventArgs args)
        {
            List<ItemDataClass> itemList = args._ItemDataClass;

            foreach(ItemDataClass item in itemList)
            {
                AssetsListItemData assetsItem = item as AssetsListItemData;
                _AssetsListItemData.Add(assetsItem);
            }
        }

        private void FetchingElementData()
        {
            ajax1_loader_gif.IsEnabled = true;
            ajax1_loader_gif.Visibility = Visibility.Visible;

            button1.IsEnabled = false;
            this.Cursor = Cursors.Wait;

            ElementItemDataParsing elementItemDataParsing = new ElementItemDataParsing(_SettingsData);
            elementItemDataParsing.StartParsing();
            elementItemDataParsing.xmlEvent += ElementOnComplete;
        }

        private void ElementOnComplete(object sender, XMLParsingEventArgs args)
        {
            List<ItemDataClass> elementItemData = args._ItemDataClass;
            foreach (ItemDataClass element in elementItemData)
            {
                SimulationItemData simulationData = element as SimulationItemData;
                _SimulationItemData.Add(simulationData);
            }

            ajax1_loader_gif.IsEnabled = false;
            ajax1_loader_gif.Visibility = Visibility.Hidden;

            button1.IsEnabled = true;
            this.Cursor = Cursors.Arrow;
            //searchButton.Enabled = true;
            //pictureBox2.Enabled = false;
            //pictureBox2.Visible = false;
            //this.Cursor = Cursors.Default;
        }

        private void RefreshButtonClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Refresh Button Click");
            FetchingElementData();
        }

        private void InputBoxKeyDown(object sender, KeyEventArgs e)
        {
            Debug.WriteLine("Input Cari text box" + e.Key.ToString());
            if(e.Key == Key.Return)
            {
                SimulationList.Items.Clear();
                SearchForGameObject(textBox);
            }

        }

        private void CariButtonClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Cari Button Clicked");
            SearchForGameObject(textBox);
        }

        private void textBox_GotFocus_1(object sender, RoutedEventArgs e)
        {
            textBox.Text = "";
            Debug.WriteLine("got Focused text box");
        }

        private void SearchForGameObject(TextBox textbox)
        {
            SimulationList.Items.Clear();
            SimulationList.ItemsSource = null;
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
                SimulationList.Items.Add(new SimulationListObject(simulation.Name, simulation.ElementCount.ToString()));
            }

            textObj.Text = "";
           
        }

        private void SearchListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(SearchListBox.ItemsSource != null)
            {
                SearchListBox.Visibility = Visibility.Collapsed;
                textBox.TextChanged -= new TextChangedEventHandler(textBox_TextChanged);
                if(SearchListBox.SelectedIndex != -1)
                {
                    textBox.Text = SearchListBox.SelectedItem.ToString();
                }
                textBox.TextChanged += new TextChangedEventHandler(textBox_TextChanged);
            }
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string typedText = textBox.Text;
            List<string> autoList = new List<string>();
            autoList.Clear();

            foreach(SimulationItemData simulation in _SimulationItemData)
            {
                foreach(string element in simulation.GetElementList)
                {
                    if(!string.IsNullOrEmpty(textBox.Text))
                    {
                        if(element.StartsWith(textBox.Text) && !autoList.Contains(element))
                        {
                            autoList.Add(element);
                        }
                    }
                }
            }

            if(autoList.Count > 0)
            {
                SearchListBox.ItemsSource = autoList;
                SearchListBox.Visibility = Visibility.Visible;
            }
            else if(textBox.Text.Equals(""))
            {
                SearchListBox.Visibility = Visibility.Collapsed;
                SearchListBox.ItemsSource = null;
            }
            else
            {
                SearchListBox.Visibility = Visibility.Collapsed;
                SearchListBox.ItemsSource = null;
            }
        }
    }
}
