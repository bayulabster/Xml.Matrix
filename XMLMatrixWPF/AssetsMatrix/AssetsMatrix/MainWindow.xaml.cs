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
using System.Collections.ObjectModel;
using Octokit;


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
        private const string IMAGE_FILEPATH = "http://labsterim.s3.amazonaws.com/media/labimages/PrefabData/";

        private SettingsItemData _SettingsData;
        private List<SimulationItemData> _SimulationItemData;
        private List<AssetsListItemData> _AssetsListItemData;
        private BackgroundWorker _BackgroundWorker;
        public ObservableCollection<AssetsListObjects> AssetLists;
        public ObservableCollection<SimulationListObject> _SimulationListObjects;

        public MainWindow()
        {
            
            
            _SimulationItemData = new List<SimulationItemData>();
            _AssetsListItemData = new List<AssetsListItemData>();
            AssetLists = new ObservableCollection<AssetsListObjects>();
            _SimulationListObjects = new ObservableCollection<SimulationListObject>();

           

            FetchingData();
            FetchingDataAssetList();
            InitializeComponent();

            Debug.WriteLine("main window");

            ajax1_loader_gif.IsEnabled = false;
            ajax1_loader_gif.Visibility = Visibility.Hidden;
            this.Cursor = Cursors.AppStarting;
            textBox.TextChanged += new TextChangedEventHandler(textBox_TextChanged);



            AssetsListGrid.DataContext = AssetLists;
            SimulationList.DataContext = _SimulationListObjects;
        }

        private GitHubClient AuthenticateGithub()
        {
            GitHubClient client = new GitHubClient(new ProductHeaderValue("mirsabayu-labster"));
            Credentials tokenOuth = new Credentials("0d47ff8c735bc7b3e6b704fa9eb504eeb76034be");

            return client;
        }

        private void GithubFetchingDataForSimulation()
        {

        }

        private void GithubFetchingDataForAssetDataList()
        {

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
          
        }

        private void RefreshButtonClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Refresh Button Click");
            FetchingElementData();
        }

        private void InputBoxKeyDown(object sender, KeyEventArgs e)
        {
            if(string.IsNullOrEmpty(textBox.Text))
            {
                _SimulationListObjects.Clear();
            }

        }

        private void CariButtonClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Cari Button Clicked");
            SearchForAssets(textBox.Text);
        }

        private void textBox_GotFocus_1(object sender, RoutedEventArgs e)
        {
            textBox.Text = "";
            Debug.WriteLine("got Focused text box");
        }

        private void SearchForAssets(string value)
        {
            //AssetLists.Clear();
            //string toLowerCase = value.ToLower();

            //for(int i = 0; i < _AssetsListItemData.Count; i++)
            //{
            //    string sourceIdLower = _AssetsListItemData[i].SourceId.ToLower();

            //    if(sourceIdLower.Contains(toLowerCase))
            //    {
            //        AssetsListItemData assetObject = _AssetsListItemData[i];
            //        string UriLink = IMAGE_FILEPATH + assetObject.Unity3DPackName + "/" + assetObject.SourceId + ".png";
            //        Debug.WriteLine("okokokokokokok :: "+UriLink);
            //        AssetLists.Add(new AssetsListObjects(UriLink, assetObject.GameObjectid, assetObject.Unity3DPackName, assetObject.SourceId, assetObject.Tooltip, assetObject.ExtendedTooltip, assetObject.ImportPath));
            //    }
            //}
        }

        private void SearchForGameObject(string value)
        {
            _SimulationListObjects.Clear();
            string textboxString = value.ToLower();
            Debug.WriteLine(" the keyword is :: " + value.ToLower());
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

            foreach(SimulationStruct simulation in simulationStructArray)
            {
                string[] valueArray = new string[2];
                valueArray[0] = simulation.Name;
                valueArray[1] = simulation.ElementCount.ToString();
                _SimulationListObjects.Add(new SimulationListObject(simulation.Name, simulation.ElementCount.ToString()));
            }
            
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

            AssetLists.Clear();
            foreach(AssetsListItemData assetObj in _AssetsListItemData)
            {
               if(!string.IsNullOrEmpty(textBox.Text))
                {
                    string lowerCaseTextBox = textBox.Text.ToLower();
                    string assetObjLowerCase = assetObj.SourceId.ToLower();
                    string UriLink = IMAGE_FILEPATH + assetObj.Unity3DPackName + "/" + assetObj.SourceId + ".png";
                    if (assetObjLowerCase.StartsWith(lowerCaseTextBox))
                    {
                        AssetLists.Add(new AssetsListObjects(UriLink, assetObj.GameObjectid, assetObj.Unity3DPackName, assetObj.SourceId, assetObj.Tooltip, assetObj.ExtendedTooltip, assetObj.ImportPath));
                    }
                }
            }

        }

        private void AssetsListGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid row = (DataGrid)sender;
            AssetsListObjects asset = row.SelectedItem as AssetsListObjects;
            SearchForGameObject(asset.SourceId);
        }
    }
}
