using puttgamesWP10.Common;
using puttgamesWP10.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
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


namespace puttgamesWP10
{
    public sealed partial class PlayersPage : Page
    {
        private List<string> gameModes_ = new List<string>();
        private int selectedGameMode_ = 0;
         
        private const string PlayerGroupName = "PlayerGroup";
        private const string NEXT_PLAYER_ID = "nextPlayerId";
        private const int RETRY_COUNT = 20;

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private AddPlayerDialog addPlayerDialog = new AddPlayerDialog();
        private RatingCalculator ratingCalculator = new RatingCalculator();
        private DataSaver dataSaver = new DataSaver();

        public PlayersPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            this.playersListView.SelectionChanged += playersListView_SelectionChanged;

            this.Loaded += PlayersPage_Loaded;

            gameModes_.Add("1025");
            gameModes_.Add("100*10");
            gameModes_.Add("Åbo");
            gameModes_.Add("JYLY");
        }

        void PlayersPage_Loaded(object sender, RoutedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            
            // sort players by ID
            PlayerGroup playerGroup = defaultViewModel[PlayerGroupName] as PlayerGroup;
           

            List<Player> playersList = new List<Player>();
            var players = playerGroup.Players;

            // 1: add players to list
            foreach (Player p in players)
            {
                playersList.Add(p);
            }
            
            // 2: sort the list
            playersList.Sort((x, y) => Convert.ToInt32(x.PlayerId).CompareTo(Convert.ToInt32(y.PlayerId)));

            string selectedPlayers = "";
            if (localSettings.Values.ContainsKey("SelectedPlayers"))
            {
                selectedPlayers = (string)localSettings.Values["SelectedPlayers"];
            }

            // 3: add players back to the playerGroup
            playerGroup.Players.Clear();
            foreach (Player pl in playersList)
            {
                playerGroup.Players.Add(pl);
            }
            localSettings.Values["SelectedPlayers"] = selectedPlayers;

            List<string> playerNamesList = new List<string>();
            parseNamesToList(selectedPlayers, ref playerNamesList);

            // PLAYER SELECTION
            // go through all the selected players
            for (int i = 0; i < playerNamesList.Count; ++i)
            {
                IList<Object> items = playersListView.Items;

                // go through all items and check the item that name matches with the player name
                for (int j = 0; j < items.Count; ++j)
                {
                    Player player = (Player)items.ElementAt(j);
                    if (player.PlayerName == playerNamesList.ElementAt(i))
                    {
                        playersListView.SelectedItems.Add(items.ElementAt(j));
                    }
                }
            }
        }

        void playersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // save selected players to localSettings
            string selectedPlayers = checkSelectedPlayers();
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["SelectedPlayers"] = selectedPlayers;
            Debug.WriteLine("selection changed, selectedPlayers: " + selectedPlayers);

            // if no players are selected, disable the start button
            if (playersListView.SelectedItems.Count == 0)
            {
                StartAppBarButton.IsEnabled = false;
            }
            else
            {
                StartAppBarButton.IsEnabled = true;
            }
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // if players page is reached from a game page, go back to the game main page
            selectedGameMode_ = (int)e.NavigationParameter;
            gameModeName.Text = " " + gameModes_[selectedGameMode_].ToString();

            int counter = 0;
            PlayerGroup playerGroup = null;
            while (playerGroup == null && counter < RETRY_COUNT)
            {
                playerGroup = await SampleDataSource.GetPlayerGroupOne();
                counter++;
            }
            this.DefaultViewModel[PlayerGroupName] = playerGroup;
        }

        private void parseNamesToList(string navigationParameters, ref List<string> parameterList)
        {
            while (navigationParameters.Length > 0)
            {
                parameterList.Add(navigationParameters.Substring(0, navigationParameters.IndexOf(";")));

                //if there is still something after the next ;
                if (navigationParameters.IndexOf(";") + 1 < navigationParameters.Length)
                {
                    navigationParameters = navigationParameters.Substring(navigationParameters.IndexOf(";") + 1);
                    //Debug.WriteLine("navigationParameters: " + navigationParameters);
                }
                // if the name was the last in the list, empty the string so the loop ends
                else
                {
                    navigationParameters = "";
                }
            }
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // save selected players
            string selectedPlayers = checkSelectedPlayers();
            Debug.WriteLine("SaveState SelectedPlayers: " + selectedPlayers);
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["SelectedPlayers"] = selectedPlayers;
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


        private void StartAppBarButton_Click(object sender, RoutedEventArgs e)
        {

            // check checked names and put them to a string separated with ";" (";" also after the last name)
            string selectedPlayers = checkSelectedPlayers();
            
            // check game mode and navigate to a page of that mode
            switch (selectedGameMode_)
            {
                case 0:
                    // 1025
                    Frame.Navigate(typeof(MXXVGamePage), selectedPlayers);  
                    break;
                case 1:
                    // 100*10
                    Frame.Navigate(typeof(CXGamePage), selectedPlayers);
                    break;
                case 2:
                    // Åbo
                    Frame.Navigate(typeof(AboGamePage), selectedPlayers);
                    break;

                case 3:
                    // JYLY
                    Frame.Navigate(typeof(JYLYGamePage), selectedPlayers);
                    break;
                
                default:
                    break;
            }
        }

        private string checkSelectedPlayers()
        {
            string selectedPlayers = "";
            
            // go through the selected items in the list view
            IList<Object> items = playersListView.SelectedItems;
            
            // create a list containing all Players
            List<Player> players = new List<Player>();
            foreach (Object o in items)
            {
                Player p = o as Player;
                players.Add(p);
            }

            // sort the player list based on id
            players.Sort( (x,y) => Convert.ToInt32(x.PlayerId).CompareTo(Convert.ToInt32(y.PlayerId)) );

            // add player names to the string to be returned
            for (int i = 0; i < players.Count; ++i)
            {
                selectedPlayers += players.ElementAt(i).PlayerName;
                selectedPlayers += ";";
            }
            return selectedPlayers;
        }

        private async void AddPlayerAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            // show dialog where new players are added
            ContentDialogResult result = await addPlayerDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                // add new player to the view model
                PlayerGroup group = this.DefaultViewModel[PlayerGroupName] as PlayerGroup;
                
                Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

                int nextPlayerId = 0;
                // check next player id from settings and increment it
                if (localSettings.Values.ContainsKey(NEXT_PLAYER_ID))
                {
                    nextPlayerId = (int)localSettings.Values[NEXT_PLAYER_ID];
                    localSettings.Values[NEXT_PLAYER_ID] = (int)localSettings.Values[NEXT_PLAYER_ID] + 1;
                }
                else
                {
                    localSettings.Values[NEXT_PLAYER_ID] = 0;
                }

                int puttRating = await ratingCalculator.PlayerRating(addPlayerDialog.GetPlayerName());
                int totalGamesPlayed = await ratingCalculator.GamesPlayed(addPlayerDialog.GetPlayerName(), true);

                Debug.WriteLine("PuttRating: " + puttRating + " gamesPlayed: " + totalGamesPlayed);

                // create new player and add it to the view model
                Player newPlayer = new Player(nextPlayerId.ToString(), addPlayerDialog.GetPlayerName(), 
                                              totalGamesPlayed, puttRating); 
                group.Players.Add(newPlayer);

                //select new player by default
                playersListView.SelectedItems.Add(newPlayer);

                // Scroll the new item into view.
                playersListView.ScrollIntoView(newPlayer);
                dataSaver.SaveAllDataToJson();                
            }
            else
            {
                // User pressed Cancel or the back arrow, do nothing
            }
        }

        // long tap opens delete option
        private void playersListView_Holding(object sender, HoldingRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            Player player = datacontext as Player;

            if (player != null)
            {
                flyoutBase.ShowAt(e.OriginalSource as FrameworkElement);
            }
            e.Handled = true;
        }

        // delete click handler
        private void MenuFlyoutDelete_Click(object sender, RoutedEventArgs e)
        {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            Player player = datacontext as Player;
            var group = this.DefaultViewModel[PlayerGroupName] as PlayerGroup;
            group.Players.Remove(player);
            dataSaver.SaveAllDataToJson();
        }
    }
}
