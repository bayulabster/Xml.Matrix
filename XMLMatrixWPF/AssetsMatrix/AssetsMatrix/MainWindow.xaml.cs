using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AssetsMatrix.Core;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Animation;
using System.Xml.Linq;
using Octokit;
using System.Net;
using System.Resources;


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

    public class TagNames
    {
        public TagNames(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string IMAGE_FILEPATH = "http://labsterim.s3.amazonaws.com/media/labimages/PrefabData/";
        private const string CREDENTIAL_FILEPATH = "CREDENTIAL.resource";
        private const double HEIGHT = 30;

        private List<SimulationItemData> _SimulationItemData;
        private List<GithubAssetDataClass> _AssetListItemDataGithub;
        private List<GithubAnimationDataClass> _AnimationListItemDataGithub;
        private List<GithubCharacterDataClass> _CharacterListItemDataGithub;
        private List<GithubAssetDetailData> _AssetListDataDetails;
        private List<GithubAssetDetailDataTemplate> _AssetListDataTemplate;

        public ObservableCollection<AssetsListObjects> AssetLists;
        public ObservableCollection<SimulationListObject> _SimulationListObjects;
        public ObservableCollection<TagNames> _TagNamesList;

        private bool _bIsLogin;
        private bool _bIsSelected;

        private GitHubClient _Client;

        public MainWindow()
        {
            InitializeComponent();
            AssetLists = new ObservableCollection<AssetsListObjects>();
            _SimulationListObjects = new ObservableCollection<SimulationListObject>();
            _SimulationItemData = new List<SimulationItemData>();
            _TagNamesList = new ObservableCollection<TagNames>();

            _AssetListItemDataGithub = new List<GithubAssetDataClass>();
            _AssetListDataDetails = new List<GithubAssetDetailData>();
            _AssetListDataTemplate = new List<GithubAssetDetailDataTemplate>();
            _AnimationListItemDataGithub = new List<GithubAnimationDataClass>();
            _CharacterListItemDataGithub = new List<GithubCharacterDataClass>();

            ReadUserNameAndCredential();
            FilledTagNames();

            _bIsSelected = false;
            SetLoading(false);
            simulation_repo_textBox.Text = "master";
            SimulationList.DataContext = _SimulationListObjects;
            listBoxSearch.Visibility = Visibility.Hidden;
            
        }

        private void ReadUserNameAndCredential()
        {
            try
            {
                using (ResourceReader reader = new ResourceReader(CREDENTIAL_FILEPATH))
                {
                    if (reader == null)
                    {
                        return;
                    }

                    string username = string.Empty;
                    string password = string.Empty;
                    foreach (DictionaryEntry elem in reader)
                    {
                        if ((string)elem.Key == "username")
                        {
                            username = (string)elem.Value;
                        }
                        else if ((string)elem.Key == "password")
                        {
                            password = (string)elem.Value;
                        }

                    }

                    username_field.Text = username;
                    password_field.Text = password;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        private void FilledTagNames()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourcePath = "AssetsMatrix.TagNames.txt";

            string result = string.Empty;
            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }

            List<string> listString = GetSplitString(result);

            foreach (var elem in listString)
            {
                _TagNamesList.Add(new TagNames(elem));   
            }
        }

        private List<String> GetSplitString(string value)
        {
            List<String> returnedString = new List<string>();

            string returnValue = value;
            string[] stringArray = returnValue.Split('\n');
            foreach (string s in stringArray)
            {
                s.Replace("\r\n", "");
                if(s.Length>0)
                {
                    returnedString.Add(AssetMatrixStaticFunction.ExtractStringFromPath(s));
                }
            }
            return returnedString;
        }

        private void SetLoading(bool value)
        {
            if (value)
            {
                ajax1_loader_gif.IsEnabled = true;
                ajax1_loader_gif.Visibility = Visibility.Visible;

                this.Cursor = Cursors.Wait;

                //button
                button_Login.IsEnabled = false;

                //field
                username_field.IsEnabled = false;
                password_field.IsEnabled = false;
                textBox.IsEnabled = false;

                //table


            }
            else
            {
                ajax1_loader_gif.IsEnabled = false;
                ajax1_loader_gif.Visibility = Visibility.Hidden;
                this.Cursor = Cursors.AppStarting;
                textBox.TextChanged += new TextChangedEventHandler(textBox_TextChanged);

                //button
                button_Login.IsEnabled = true;

                //field
                username_field.IsEnabled = true;
                password_field.IsEnabled = true;
                textBox.IsEnabled = true;

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
                Console.Write(e.ToString());
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

            if (_Client == null)
            {
                return;
            }

            SaveCredentials(username, password);

            SetLoading(true);
            FethcingDataFromGithub();
        }

        private void SaveCredentials(string username, string password)
        {
            //TODO : write to file the username and password

            using (ResourceWriter writer = new ResourceWriter(CREDENTIAL_FILEPATH))
            {
                if (writer == null)
                {
                    return;
                }

                writer.AddResource("username", username);
                writer.AddResource("password", password);
                writer.Close();
            }
        }

        private void FethcingDataFromGithub()
        {
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
          
        private void OnMouseClick_RefreshSimulationListData(object sender, RoutedEventArgs e)
        {

            GithubFetchingDataForSimulation();
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
            listBoxSearch.Visibility = Visibility.Visible;
            string inputText = textBox.Text;
            if (string.IsNullOrEmpty(inputText))
            {
                return;
            }

            string lowerCaseInputText = inputText.ToLower();

            listBoxSearch.Items.Clear();
            listBoxSearch.Height = HEIGHT;
            foreach (var elem in _TagNamesList)
            {
                string lowerCaseName = elem.Name.ToLower();
                if (lowerCaseName.Contains(lowerCaseInputText))
                {
                    listBoxSearch.Items.Add(elem.Name);
                    listBoxSearch.Height += HEIGHT;
                }
            }

           
        }

        private void TextBoxChanged()
        {
            _SimulationListObjects.Clear();
            
            if (string.IsNullOrEmpty(textBox.Text))
                return;

            int simcounter = 0;
            
            XElement lowerCaseTextBox = CreateXElementFromSearchedText();
            if (lowerCaseTextBox == null)
            {
                return;
            }
            foreach (SimulationItemData simulation in _SimulationItemData)
            {
                int counterSearch = simulation.SearchInXML(lowerCaseTextBox);

                if (counterSearch == 0)
                {
                    continue;
                }
                string simulationName = simulation.GetSimulationName.ToLower();

                _SimulationListObjects.Add(new SimulationListObject(simulationName, "total item found is :: "+counterSearch.ToString()));
                simcounter++;
            }

            totalSimFound.Text = simcounter.ToString();
        }

        private XElement CreateXElementFromSearchedText()
        {
            string lowerCaseTextBox = textBox.Text;

            if (!lowerCaseTextBox.Contains("<"))
            {
                lowerCaseTextBox = "<" + lowerCaseTextBox;
            }

            if (!lowerCaseTextBox.Contains("/>"))
            {
                lowerCaseTextBox = lowerCaseTextBox + " />";
            }

            try
            {
                XDocument doc = XDocument.Parse(lowerCaseTextBox);
                XElement returnedValue = doc.Root;

                return returnedValue;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;

        }

        private void InputBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                TextBoxChanged();
                listBoxSearch.Visibility = Visibility.Hidden;
            }
           
        }

        private void textBox_GotFocus_1(object sender, RoutedEventArgs e)
        {
            if (_bIsSelected == false)
            {
                textBox.Text = "";
            }
            
            listBoxSearch.Visibility = Visibility.Visible;
        }

        private void SimulationList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }
        
        private void ListBoxSearch_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox combo = (ListBox)sender;
            if (combo.SelectedItem != null)
            {
                string cleanedUpString = combo.SelectedItem.ToString().Replace("\r","");
                textBox.Text = cleanedUpString;
                listBoxSearch.Visibility = Visibility.Hidden;
                TextBoxChanged();
                _bIsSelected = true;
            }
        }
    }
}
