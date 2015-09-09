using puttgamesWP10.Common;
using puttgamesWP10.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using Windows.Data.Json;
using Windows.Storage;

// The Pivot Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641

namespace puttgamesWP10
{
    /// <summary>
    /// A page that displays details for a single item within a group.
    /// </summary>
    public sealed partial class HighScoresPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private string ResultGroupName = "FirstGroup";
        private Dictionary<string, string> gameModes = new Dictionary<string, string>();
        private RatingCalculator ratingCalculator = new RatingCalculator();
        private DataSaver dataSaver = new DataSaver();
        const string JSON_FILENAME = "Data.json";
        private const int RETRY_COUNT = 20;
        private string navParameter;

        public HighScoresPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            this.Loaded += HighScoresPage_Loaded;

            gameModes.Add("0", "1025");
            gameModes.Add("1", "100*10");
            gameModes.Add("2", "ÅBO");
            gameModes.Add("3", "JYLY");
        }

        async void HighScoresPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!DefaultViewModel.ContainsKey(ResultGroupName) || DefaultViewModel[ResultGroupName] == null)
            {
                GameResultsGroup resultGroup = null;
                int counter = 0;
                while (resultGroup == null && counter < RETRY_COUNT)
                {
                    resultGroup = await SampleDataSource.GetResultGroupAsync(navParameter);
                    counter++;
                }
                Debug.WriteLine("HiScores Loaded: counter: " + counter);
            }
            Debug.WriteLine("HSP_Loaded ");
        } 

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>.
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            Debug.WriteLine("HS: load");
            // navigation parameter is gameModeId as string
            GameResultsGroup resultGroup = null;
            int counter = 0;
            while (resultGroup == null && counter < RETRY_COUNT)
            {
                resultGroup = await SampleDataSource.GetResultGroupAsync(e.NavigationParameter.ToString());
                counter++;
            }
            Debug.WriteLine("HiScores: counter: " + counter);

            this.DefaultViewModel[ResultGroupName] = resultGroup;

            Header.Text = gameModes[e.NavigationParameter.ToString()];
            navParameter = e.NavigationParameter.ToString();
            //var item = await SampleDataSource.GetItemAsync((string)e.NavigationParameter);
            //this.DefaultViewModel["Item"] = item;
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/>.</param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {

            // TODO: Save the unique state of the page here.
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void SelectionModeAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (scoresListView.SelectionMode == ListViewSelectionMode.None)
            {
                scoresListView.SelectionMode = ListViewSelectionMode.Multiple;
            }
            else
            {
                scoresListView.SelectionMode = ListViewSelectionMode.None;
                DeleteAppBarButton.IsEnabled = false;
            }
        }

        private void DeleteAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var results = this.DefaultViewModel[ResultGroupName] as GameResultsGroup;
            var toRemove = scoresListView.SelectedItems;

            List<string> affectedPlayers = new List<string>();
            while(toRemove.Count > 0)
            {
                Result resultToRemove = toRemove.ElementAt(0) as Result;
                results.Results.Remove(resultToRemove);
                
                string playerNameToRemove = resultToRemove.ResultPlayerName;
                if(!affectedPlayers.Contains(playerNameToRemove))
                {
                    Debug.WriteLine("playerNameToRemove: " + playerNameToRemove);
                    affectedPlayers.Add(playerNameToRemove);
                }
            }
            RecalculateRatings(affectedPlayers);

            Debug.WriteLine("Starting to save all data to json");
            dataSaver.SaveAllDataToJson();
        }

        //calculates new putt rating for all players whose names are in the parameter List
        private async void RecalculateRatings(List<string> playerNames)
        {
            // Get results for all game modes
            var playerGroup = await SampleDataSource.GetPlayerGroupAsync("0");

            List<Player> affectedPlayerObjects = new List<Player>();
            for (int i = 0; i < playerGroup.Players.Count; ++i)
            {
                if (playerNames.Contains(playerGroup.Players.ElementAt(i).PlayerName))
                {
                    affectedPlayerObjects.Add(playerGroup.Players.ElementAt(i));
                    //Debug.WriteLine("playerObject.Name: " + playerGroup.Players.ElementAt(i).PlayerName);
                }
            }
            foreach (Player player in affectedPlayerObjects)
            {
                player.PuttRating = await ratingCalculator.PlayerRating(player.PlayerName);
                player.GamesPlayed = await ratingCalculator.GamesPlayed(player.PlayerName, true);
            }
            
            Debug.WriteLine("RecalculateRatings Done");
        }
        
        /*
        private void OrderPlayerGroup(ref PlayerGroup playerDataGroup)
        {
            Player player = playerDataGroup.Players.Last();
            // order the added player to its place
            //order list with ptt ratings!
            // if there is at least two players, try to sort
            bool moved = false;
            if (playerDataGroup.Players.Count > 1)
            {
                // sort by moving the item upwards as long as necessary
                for (int i = 0; i < playerDataGroup.Players.Count; ++i)
                {
                    // move one upwards, if 
                    if (playerDataGroup.Players.Count > i - 1 &&
                        playerDataGroup.Players.ElementAt(playerDataGroup.Players.Count - 2 - i).PuttRating < player.PuttRating)
                    {
                        //Debug.WriteLine("i:" + i + " group.results.count: " + group.Results.Count + "newScore: " + newResult.Score + "other score: " + group.Results.ElementAt(group.Results.Count - 2 - i).Score);

                        playerDataGroup.Players.Move(playerDataGroup.Players.Count - 1 - i, playerDataGroup.Players.Count - 2 - i);
                        moved = true;

                        if (playerDataGroup.Players.Count - 2 - i == 0)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            // if there is only one item, or the item have not been moved, remove and add to refresh the view
            else if (playerDataGroup.Players.Count == 1 || !moved)
            {
                Player p = playerDataGroup.Players.Last();
                playerDataGroup.Players.Remove(p);
                playerDataGroup.Players.Add(p);
            }
        }*/

        private void scoresListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (scoresListView.SelectedItems.Count == 0)
            {
                DeleteAppBarButton.IsEnabled = false;
            }
            else
            {
                DeleteAppBarButton.IsEnabled = true;
            }
        }
        
        private void MenuFlyoutDelete_Click(object sender, RoutedEventArgs e)
        {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            Result result = datacontext as Result;
            var group = this.DefaultViewModel[ResultGroupName] as GameResultsGroup;
            group.Results.Remove(result);
            
            // recalculate rating for the player whose result was deleted
            List<string> affectedPlayerNames = new List<string>();
            affectedPlayerNames.Add(result.ResultPlayerName);
            RecalculateRatings(affectedPlayerNames);

            dataSaver.SaveAllDataToJson();
        }

        private void scoresListView_Holding(object sender, HoldingRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);

            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            Result result = datacontext as Result;

            if (result != null)
            {
                flyoutBase.ShowAt(e.OriginalSource as FrameworkElement);
            }
            e.Handled = true;
        }
    }
}