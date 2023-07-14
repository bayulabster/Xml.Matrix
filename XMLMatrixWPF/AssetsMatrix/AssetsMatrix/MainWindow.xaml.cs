using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AssetsMatrix.Core;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
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

            FilledTagNames();

            _bIsSelected = false;
            SetLoading(false);
            simulation_repo_textBox.Text = "master";
            SimulationList.DataContext = _SimulationListObjects;
            listBoxSearch.Visibility = Visibility.Hidden;
            
        }

        private void FilledTagNames()
        {
            _TagNamesList.Add(new TagNames("Placeholder"));
            _TagNamesList.Add(new TagNames("Element"));
            _TagNamesList.Add(new TagNames("Container"));
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

            SetLoading(true);
            FethcingDataFromGithub();
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
            
            string lowerCaseTextBox = textBox.Text;
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

            XDocument doc = XDocument.Parse(lowerCaseTextBox);
            XElement returnedValue = doc.Root;

            return returnedValue;
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
            throw new NotImplementedException();
        }
        
        private void ListBoxSearch_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox combo = (ListBox)sender;
            if (combo.SelectedItem != null)
            {
                textBox.Text = combo.SelectedValue.ToString();
                listBoxSearch.Visibility = Visibility.Hidden;
                TextBoxChanged();
                _bIsSelected = true;
            }
        }
    }
}
