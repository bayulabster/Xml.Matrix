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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

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
        private List<GithubAssetDataClass> _AssetListItemDataGithub;
        private List<GithubAnimationDataClass> _AnimationListItemDataGithub;
        private List<GithubCharacterDataClass> _CharacterListItemDataGithub;
        private List<GithubAssetDetailData> _AssetListDataDetails;
        private List<GithubAssetDetailDataTemplate> _AssetListDataTemplate;
        private string _AssetXMLName;
        private int _AssetsSelection;

        public ObservableCollection<AssetsListObjects> AssetLists;
        public ObservableCollection<SimulationListObject> _SimulationListObjects;
        public ObservableCollection<AssetDetailsEntryObject> _AssetDetailList;

        private bool _bIsLogin;

        private GitHubClient _Client;

        public MainWindow()
        {
            InitializeComponent();
            AssetLists = new ObservableCollection<AssetsListObjects>();
            _SimulationListObjects = new ObservableCollection<SimulationListObject>();
            _AssetDetailList = new ObservableCollection<AssetDetailsEntryObject>();
            _SimulationItemData = new List<SimulationItemData>();
#if WEB_URL
           
            _AssetsListItemData = new List<AssetsListItemData>();
          

            //FetchingData();
            //FetchingDataAssetList();
          
#elif GITHUB
            _AssetListItemDataGithub = new List<GithubAssetDataClass>();
            _AssetListDataDetails = new List<GithubAssetDetailData>();
            _AssetListDataTemplate = new List<GithubAssetDetailDataTemplate>();
            _AnimationListItemDataGithub = new List<GithubAnimationDataClass>();
            _CharacterListItemDataGithub = new List<GithubCharacterDataClass>();

#endif
            SetLoading(false);
            simulation_repo_textBox.Text = "master";
            asset_repo_textBox.Text = "2019";

            AssetsListGrid.DataContext = AssetLists;
            SimulationList.DataContext = _SimulationListObjects;
            AssetDetailsList.ItemsSource = _AssetDetailList;

            _AssetsSelection = comboBox.SelectedIndex = 0;
        }

        private void SetLoading(bool value)
        {
            if (value)
            {
                ajax1_loader_gif.IsEnabled = true;
                ajax1_loader_gif.Visibility = Visibility.Visible;

                this.Cursor = Cursors.Wait;

                //button
                button1.IsEnabled = false;
                button_Login.IsEnabled = false;
                AssetDetailsAdd.IsEnabled = false;

                //field
                username_field.IsEnabled = false;
                password_field.IsEnabled = false;
                textBox.IsEnabled = false;
                assetdetails_key.IsEnabled = false;
                assetdetails_content.IsEnabled = false;

                //table
                AssetsListGrid.IsEnabled = false;


            }
            else
            {
                ajax1_loader_gif.IsEnabled = false;
                ajax1_loader_gif.Visibility = Visibility.Hidden;
                this.Cursor = Cursors.AppStarting;
                textBox.TextChanged += new TextChangedEventHandler(textBox_TextChanged);

                //button
                button1.IsEnabled = true;
                button_Login.IsEnabled = true;
                AssetDetailsAdd.IsEnabled = true;

                //field
                username_field.IsEnabled = true;
                password_field.IsEnabled = true;
                textBox.IsEnabled = true;
                assetdetails_key.IsEnabled = true;
                assetdetails_content.IsEnabled = true;

                //table
                AssetsListGrid.IsEnabled = true;
            }
        }

        private GitHubClient AuthenticateGithub(string username, string credentials)
        {
            try
            {
                GitHubClient client = new GitHubClient(new ProductHeaderValue(username));
                Credentials credentialsClient = new Credentials(credentials);
                client.Credentials = credentialsClient;

                if (IsAuthenticationValid(client))
                {
                    return client;
                }

                return null;
            }
            catch (Exception e)
            {
                return null;
            }

        }

        private bool IsAuthenticationValid(GitHubClient client)
        {
            User test = null;

            try
            {
                test = client.User.Current().GetAwaiter().GetResult();
                Debug.WriteLine(test.Id);
                if (test.Id != 0)
                    return true;


            }
            catch (Exception e)
            {
                Debug.WriteLine("Authorization error :: " + e.Message + "\n" + e.StackTrace);
                return false;
            }

            return test != null;

        }

        private void button_Login_Click(object sender, RoutedEventArgs e)
        {
            _bIsLogin = true;
            Debug.WriteLine("button login clicked");
            string username = username_field.Text;
            string password = password_field.Text;
            _Client = AuthenticateGithub(username, password);

            SetLoading(true);
            FethcingDataFromGithub();
        }

        private void FethcingDataFromGithub()
        {
            GithubFetchingDataForAssetDetails();
            //GithubFetchingDataForAssetDataDetailsFromTemplate();
        }

        private void GithubFetchingDataForAssetDataDetailsFromTemplate()
        {
            if (!_bIsLogin)
                SetLoading(true);

            string repoName = "Labster.XmlTemplates";
            string branchName = "master";

            GithubAssetDetailTemplateDataParsing dataParsing = new GithubAssetDetailTemplateDataParsing(_Client, repoName, branchName);
            dataParsing.StartParsing();
            dataParsing.xmlEvent += OnGithubFetchingAssetTemplateCompleted;
        }

        private void OnGithubFetchingAssetTemplateCompleted(object sender, XMLParsingEventArgs args)
        {
            _AssetListDataTemplate.Clear();

            if(args._ItemDataClass != null)
            {
                foreach(ItemDataClass item in args._ItemDataClass)
                {
                    GithubAssetDetailDataTemplate data = item as GithubAssetDetailDataTemplate;
                    _AssetListDataTemplate.Add(data);
                }
            }

            GithubFetchingDataForAssetDetails();
        }


        private void GithubFetchingDataForAssetDetails()
        {
            if (!_bIsLogin)
                SetLoading(true);

            string repoName = "AssetDetails";
            string branchName = "master";

            GithubAssetDetailDataParsing githubParsing = new GithubAssetDetailDataParsing(_Client, repoName, branchName, _AssetXMLName);
            githubParsing.StartParsing();
            githubParsing.xmlEvent += OnGithubFetchingAssetDetailsCompleted;

        }

        private void OnGithubFetchingAssetDetailsCompleted(object sender, XMLParsingEventArgs args)
        {

            _AssetListDataDetails.Clear();

            if (args._ItemDataClass != null)
                foreach (ItemDataClass item in args._ItemDataClass)
                {
                    GithubAssetDetailData assetDetailData = item as GithubAssetDetailData;
                    _AssetListDataDetails.Add(assetDetailData);
                }

            if (_bIsLogin)
                GithubFetchingDataForAssetDataList();
            else
                SetLoading(false);
        }

        private void GithubFetchingDataForAssetDataList()
        {
            if (!_bIsLogin)
                SetLoading(true);

            string repoName = "Labster.Art.Lab";
            string branchName = asset_repo_textBox.Text;

            GithubAssetListDataParsing githubParsing = new GithubAssetListDataParsing(_Client, repoName, branchName);
            githubParsing.StartParsing();
            githubParsing.xmlEvent += OnGithubFetchingDataComplete;
        }

        private void OnGithubFetchingDataComplete(object sender, XMLParsingEventArgs args)
        {
            _AssetListItemDataGithub.Clear();

            if (args._ItemDataClass != null)
            {
                foreach (ItemDataClass data in args._ItemDataClass)
                {
                    GithubAssetDataClass assetData = data as GithubAssetDataClass;
                    _AssetListItemDataGithub.Add(assetData);
                }
            }

            string repoName = "Labster.Art.Animations";
            string branchName = asset_repo_textBox.Text;

            GithubAnimationListDataParsing githubParsing = new GithubAnimationListDataParsing(_Client, repoName, branchName);
            githubParsing.StartParsing();
            githubParsing.xmlEvent += OnGithubFetchingAnimationDataCompleted;
           
        }

        private void OnGithubFetchingAnimationDataCompleted(object sender, XMLParsingEventArgs args)
        {
            _AnimationListItemDataGithub.Clear();

            if (args._ItemDataClass != null)
            {
                foreach (ItemDataClass data in args._ItemDataClass)
                {
                    GithubAnimationDataClass assetData = data as GithubAnimationDataClass;
                    _AnimationListItemDataGithub.Add(assetData);
                }
            }
            GithubFetchingDataForCharacter();
        }

        private void GithubFetchingDataForCharacter()
        {
            string repoName = "Labster.Art.LabCharacters";
            string branchName = simulation_repo_textBox.Text;

            GithubCharacterListDataParsing githubParsing = new GithubCharacterListDataParsing(_Client, repoName, branchName);
            githubParsing.StartParsing();
            githubParsing.xmlEvent += OnGithubFetchingCharacterDataCompleted;
        }

        private void OnGithubFetchingCharacterDataCompleted(object sender, XMLParsingEventArgs args)
        {
            _CharacterListItemDataGithub.Clear();

            if(args._ItemDataClass != null)
            {
                foreach(ItemDataClass data in args._ItemDataClass)
                {
                    GithubCharacterDataClass assetData = data as GithubCharacterDataClass;
                    _CharacterListItemDataGithub.Add(assetData);
                }
            }

            GithubFetchingDataForSimulation();
        }

        private void GithubFetchingDataForSimulation()
        {
            if (!_bIsLogin)
                SetLoading(true);

            string repoName = "Labster.Simulations";
            string branchName = simulation_repo_textBox.Text;

            GithubSimulationListDataParsing githubParsing = new GithubSimulationListDataParsing(_Client, repoName, branchName);
            githubParsing.StartParsing();
            githubParsing.xmlEvent += OnGithubFetchingSimulationDataCompleted;
        }

        private void OnGithubFetchingSimulationDataCompleted(object sender, XMLParsingEventArgs args)
        {
            Debug.WriteLine("Fetching Simulation Data Completed");
            _SimulationItemData.Clear();

            if (args._ItemDataClass != null)
            {
                foreach (ItemDataClass data in args._ItemDataClass)
                {
                    SimulationItemData simulationData = data as SimulationItemData;
                    _SimulationItemData.Add(simulationData);
                }
            }

            if (_bIsLogin)
                _bIsLogin = false;

            SetLoading(false);
        }

        #region WEB_URL

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

            foreach (ItemDataClass item in itemList)
            {
                AssetsListItemData assetsItem = item as AssetsListItemData;
                _AssetsListItemData.Add(assetsItem);
            }
        }

        private void FetchingElementData()
        {
            ajax1_loader_gif.IsEnabled = true;
            ajax1_loader_gif.Visibility = Visibility.Visible;

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

            this.Cursor = Cursors.Arrow;

        }

        #endregion

        private void RefreshButtonClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Refresh Button Click");
            FetchingElementData();
        }

        private void InputBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(textBox.Text))
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
            AssetLists.Clear();
            string toLowerCase = value.ToLower();

#if WEB_URL
            for (int i = 0; i < _AssetsListItemData.Count; i++)
            {
                string sourceIdLower = _AssetsListItemData[i].SourceId.ToLower();

                if (sourceIdLower.Contains(toLowerCase))
                {
                    AssetsListItemData assetObject = _AssetsListItemData[i];
                    string UriLink = IMAGE_FILEPATH + assetObject.Unity3DPackName + "/" + assetObject.SourceId + ".png";
                    Debug.WriteLine("okokokokokokok :: " + UriLink);
                    AssetLists.Add(new AssetsListObjects(UriLink, assetObject.GameObjectid, assetObject.Unity3DPackName, assetObject.SourceId, assetObject.Tooltip, assetObject.ExtendedTooltip, assetObject.ImportPath));
                }
            }

#elif GITHUB
            ProcessDataList(toLowerCase);
#endif
        }

        private void ProcessDataList(string value, bool bIsAll=false, bool bIsStrictMode = false)
        {
            switch (_AssetsSelection)
            {
                case 0:
                    ProcessDataListAssets(value, bIsAll, bIsStrictMode);
                    break;
                case 1:
                    ProcessDataListAnimation(value, bIsAll, bIsStrictMode);
                    break;
                case 2:
                    ProcessDataListCharacter(value, bIsAll);
                    break;
            }
        }

        private void ProcessDataListAssets(string value, bool bIsAll = false, bool isStrictMode = false)
        {
            try
            {
                if (bIsAll)
                    _AssetListItemDataGithub.ForEach(assetObj => { AssetLists.Add(new AssetsListObjects("", assetObj.Content, assetObj.Content, assetObj.Content, "", "", "")); });

                foreach (GithubAssetDataClass assetObj in _AssetListItemDataGithub)
                {
                    string assetObjLowerCase = assetObj.Content.ToLower();
                    bool isMatch = isStrictMode ? assetObjLowerCase.Equals(value) :
                    Regex.IsMatch(assetObjLowerCase, value);
                    if (isMatch)
                    {
                        AssetLists.Add(new AssetsListObjects("", assetObj.Content, assetObj.Content, assetObj.Content, "", "", ""));
                    }
                }
            }catch(Exception e)
            {

            }
            
        }

        private void ProcessDataListAnimation(string value,bool bIsAll = false, bool isStrictMode = false)
        {
            try
            {
                if (bIsAll)
                    _AnimationListItemDataGithub.ForEach(assetObj => { AssetLists.Add(new AssetsListObjects("", assetObj.Content, assetObj.Content, assetObj.Content, "", "", "")); });

                foreach (GithubAnimationDataClass assetObj in _AnimationListItemDataGithub)
                {
                    string assetObjLowerCase = assetObj.Content.ToLower();
                    bool isMatch = isStrictMode ? assetObjLowerCase.Equals(value) :
                    Regex.IsMatch(assetObjLowerCase, value);
                    if (isMatch)
                    {
                        AssetLists.Add(new AssetsListObjects("", assetObj.Content, assetObj.Content, assetObj.Content, "", "", ""));
                    }
                }
            }catch(Exception e)
            {

            }
            
        }

        private void ProcessDataListCharacter(string value, bool bIsAll = false, bool isStrictMode = false)
        {
            try
            {
                if (bIsAll)
                    _CharacterListItemDataGithub.ForEach(assetObj => { AssetLists.Add(new AssetsListObjects("", assetObj.Content, assetObj.Content, assetObj.Content, "", "", "")); });

                foreach (GithubCharacterDataClass assetObj in _CharacterListItemDataGithub)
                {
                    string assetObjLowerCase = assetObj.Content.ToLower();
                    bool isMatch = isStrictMode ? assetObjLowerCase.Equals(value) :
                    Regex.IsMatch(assetObjLowerCase, value);
                    if (isMatch)
                    {
                        AssetLists.Add(new AssetsListObjects("", assetObj.Content, assetObj.Content, assetObj.Content, "", "", ""));
                    }
                }
            }
            catch(Exception e)
            {

            }
            
        }

        

        private void SearchForGameObject(string value)
        {
            _SimulationListObjects.Clear();
            string textboxString = value.Contains("/") ? AssetMatrixStaticFunction.TrimString(value.ToLower()) : value.ToLower();
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
                _SimulationListObjects.Add(new SimulationListObject(simulation.Name, simulation.ElementCount.ToString()));
            }


        }

        private void SearchForAssetsFromSimulations(string value)
        {
            AssetLists.Clear();
            string simulationName = value.ToLower();
            foreach(SimulationItemData simData in _SimulationItemData)
            {
                if(simData.GetSimulationName.ToLower().Contains(simulationName))
                {
                    foreach(string assetElement in simData.GetElementList)
                    {
                        ProcessDataList(assetElement,false,true);
                    }
                    return;
                }
            }

        }

        private void SearchListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchListBox.ItemsSource != null)
            {
                SearchListBox.Visibility = Visibility.Collapsed;
                textBox.TextChanged -= new TextChangedEventHandler(textBox_TextChanged);
                if (SearchListBox.SelectedIndex != -1)
                {
                    textBox.Text = SearchListBox.SelectedItem.ToString();
                }
                textBox.TextChanged += new TextChangedEventHandler(textBox_TextChanged);
            }
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

            AssetLists.Clear();
            _SimulationListObjects.Clear();

            if (string.IsNullOrEmpty(textBox.Text))
                return;

            string lowerCaseTextBox = textBox.Text.ToLower();
            bool bIsSimulation = false;
            bool bIsAll = false;
            string engineKeyword = "engine_";
            string allKeyword = "(all)";

            if (textBox.Text.ToLower().Contains(engineKeyword))
                bIsSimulation = true;

            if (textBox.Text.ToLower().Contains(allKeyword))
            {
                lowerCaseTextBox = string.Empty;
                bIsAll = true;
            }
                
#if WEB_URL
            foreach (AssetsListItemData assetObj in _AssetsListItemData)
            {

            if (!string.IsNullOrEmpty(textBox.Text))
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
#elif GITHUB
            if (bIsSimulation)
            {
                foreach (SimulationItemData simulation in _SimulationItemData)
                {
                    if (!bIsAll)
                    {
                        string simulationName = simulation.GetSimulationName.ToLower();
                        if (simulationName.StartsWith(lowerCaseTextBox))
                        {
                            _SimulationListObjects.Add(new SimulationListObject(simulation.GetSimulationName, ""));
                        }

                    }
                    else
                    {
                        _SimulationListObjects.Add(new SimulationListObject(simulation.GetSimulationName, ""));
                    }

                }

            }
            else
            {
                ProcessDataList(lowerCaseTextBox, bIsAll);
            }
#endif

        }

        private void AssetDetailsAddAction(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Asset details add action");

            string content = assetdetails_content.Text;
            string key = assetdetails_key.Text;
            bool isText = IsContainImage(content) == false;
            if (content != "" || key != "")
                _AssetDetailList.Add(new AssetDetailsEntryObject(content, key, content, isText));
            assetdetails_content.Text = "";
            assetdetails_key.Text = "";
        }

        private bool IsContainImage(string value)
        {
            string lowercase = value.ToLower();

            return value.Contains(".jpeg") || value.Contains(".png") || value.Contains(".bmp") || value.Contains(".jpg");
        }

        private void AssetDetailSaveAction(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Asset detail save action");
            string xmlAsset = CreateXMLForAsset(_AssetDetailList);
            SendAssetToGithub(xmlAsset);
        }

        private string CreateXMLForAsset(ObservableCollection<AssetDetailsEntryObject> asset)
        {
            string xmlString = "";
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.Encoding = new UTF8Encoding(false);
            using (MemoryStream stream = new MemoryStream())
            {
                using (XmlWriter writer = XmlWriter.Create(stream, settings))
                {
                    writer.WriteStartElement("xml_asset");
                    foreach (AssetDetailsEntryObject assetObj in asset)
                    {
                        writer.WriteStartElement("asset_detail");
                        writer.WriteElementString("asset_key", assetObj.key_name);
                        if(assetObj.Is_text)
                            writer.WriteElementString("asset_content", assetObj.content_name);
                        else
                            writer.WriteElementString("asset_content", assetObj.ImageFilePath.AbsoluteUri);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.Flush();
                }

                xmlString = Encoding.Default.GetString(stream.ToArray());
            }
            

            Debug.WriteLine(xmlString.ToString());
            
            return xmlString;
        }

        private async Task<int> SendAssetToGithub(string value)
        {
            try
            {
                var owner = GithubDataParsingBase.GITHUB_REPO_OWNER;
                var repo = "AssetDetails";
                var branch = "master";

                foreach (GithubAssetDetailData item in _AssetListDataDetails)
                {
                    if (item.assetName == _AssetXMLName)
                    {
                        var existing_file = await _Client.Repository.Content.GetAllContentsByRef(owner, repo, _AssetXMLName, branch);
                        var UpdateChangeSet = await _Client.Repository.Content.UpdateFile(owner, repo, _AssetXMLName, new UpdateFileRequest(_AssetXMLName + " is Updated :: " + DateTime.UtcNow, value, existing_file.First().Sha, branch));

                        return 0;
                    }
                }

                var createChangeSet = await _Client.Repository.Content.CreateFile(owner, repo, _AssetXMLName, new CreateFileRequest(_AssetXMLName + " Is Created ::  " + DateTime.UtcNow, value));
            }
            catch (Exception e)
            {
                Debug.WriteLine("Asset matrix error :: " + e.Message + "\n" + e.StackTrace);
            }
            return 0;
        }

        private void AssetsListGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _AssetDetailList.Clear();
            DataGrid row = (DataGrid)sender;
            AssetsListObjects asset = row.SelectedItem as AssetsListObjects;
            SearchForGameObject(asset.SourceId);
            _AssetXMLName = asset.SourceId + ".xml";
            GetAssetDetailsData();
        }

        private void GetAssetDetailsData()
        {
            _AssetDetailList.Clear();

            foreach (GithubAssetDetailDataTemplate data in _AssetListDataTemplate)
            {
                if(data.AssetName.ToLower() == _AssetXMLName.ToLower())
                {
                    string key_name = "Name";
                    string key_image = "MainImage";
                    string content_name = data.Name;
                    string content_imageurl = data.ImageUrl;
                    bool isText = IsContainImage(content_imageurl) == false;

                    AssetDetailsEntryObject objName = new AssetDetailsEntryObject("", key_name, content_name, false);
                    AssetDetailsEntryObject objImage = new AssetDetailsEntryObject(content_imageurl, key_image, "", true);
                    _AssetDetailList.Add(objName);
                    _AssetDetailList.Add(objImage);
                }
            }

            foreach(GithubAssetDetailData data in _AssetListDataDetails)
            {
                if(data.assetName == _AssetXMLName)
                {
                    
                    foreach(KeyValuePair<string,string> pair in data.AssetContentString)
                    {
                        string key = pair.Key;
                        string content = pair.Value;
                        bool isText = IsContainImage(content) == false;

                        AssetDetailsEntryObject obj = new AssetDetailsEntryObject(content, key, content, isText);
                        _AssetDetailList.Add(obj);
                    }
                    return;
                }
            }
        }


        private void AddAssetDetailDataFromGithub(GithubAssetDetailData item)
        {
           foreach(KeyValuePair<string, string> pair in item.AssetContentString)
            {
                string key = pair.Key;
                string content = pair.Value;
                bool isText = (IsContainImage(content) == false);

                AssetDetailsEntryObject obj = new AssetDetailsEntryObject(content, key, content, isText);
                _AssetDetailList.Add(obj);
            }
            
        }

        private void AssetDetailsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListView obj = (ListView)sender;
            AssetDetailsEntryObject assetDetails = obj.SelectedItem as AssetDetailsEntryObject;
            _AssetDetailList.Remove(assetDetails);
            
        }

        private void OnMouseClick_RefreshSimulationListData(object sender, RoutedEventArgs e)
        {

            GithubFetchingDataForSimulation();
        }

        private void OnMouseClick_RefreshAssetListData(object sender, RoutedEventArgs e)
        {
            GithubFetchingDataForAssetDataList();
        }

        private void OnMouseClick_RefreshAssetDetailData(object sender, RoutedEventArgs e)
        {
            _AssetXMLName = "";
            GithubFetchingDataForAssetDetails();
        }

        private void SimulationList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _AssetDetailList.Clear();
            DataGrid row = (DataGrid)sender;
            SimulationListObject asset = row.SelectedItem as SimulationListObject;
            SearchForAssetsFromSimulations(asset.Name);
            //SearchForGameObject(asset.SourceId);
            //_AssetXMLName = asset.SourceId + ".xml";
        }

        private void checkBox_assets_Checked(object sender, RoutedEventArgs e)
        {
           
        }

        private void comboBox_Selected(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(sender);
            //_AssetsSelection
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = (ComboBox)sender;
            _AssetsSelection = box.SelectedIndex;
        }
    }
}
