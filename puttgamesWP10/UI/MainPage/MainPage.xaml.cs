using puttgamesWP10;
using puttgamesWP10.Common;
using puttgamesWP10.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Store;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace puttgamesWP10
{
    public sealed partial class MainPage : Page
    {
        private const string FirstGroupName = "FirstGroup";
        private const string SecondGroupName = "SecondGroup";
        private const string ThirdGroupName = "ThirdGroup";
        private const string FourthGroupName = "FourthGroup";
        private const string JYLYResultGroupName = "JYLYResultGroupName";

        private const string PlayerGroupName = "PlayerGroup";
        private const string NEXT_PLAYER_ID = "nextPlayerId";

        private const string PROPACK_MESSAGE_TEXT = "This game mode is available only in ProPack.";
        private const string PROPACK_MESSAGE_TITLE = "ProPack only";
        private const string BUY = "more info";
        private const string CANCEL = "cancel";

        private const string PIVOT_INDEX = "PivotIndex";

        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private AddPlayerDialog addPlayerDialog = new AddPlayerDialog();
        //private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");
        private RatingCalculator ratingCalculator = new RatingCalculator();
        private DataSaver dataSaver = new DataSaver();
        private LicenseInformation licenseInformation;
        private const string PRO_PACK = "ProPack";
        private const int RETRY_COUNT = 20;

        //debug 
        //private const string XML_NOK = "in-app-purchase_nok.xml";
        //private const string XML_OK = "in-app-purchase_ok.xml";

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            pivot.SelectionChanged += pivot_SelectionChanged;

            //licenseInformation = CurrentAppSimulator.LicenseInformation;

            licenseInformation = CurrentApp.LicenseInformation;

            pivot.PivotItemLoading += pivot_PivotItemLoading;
        }



        void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedIndex == 4)
            {
                AddPlayerAppBarButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                NewGameAppBarButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                AddPlayerAppBarButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                NewGameAppBarButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
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
        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            Application.Current.Exit();
        }
        /* DEBUG ONLY
        private async void LoadInAppPurchaseProxyFileAsync(string filename)
        {
            StorageFile proxyFile =
                await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///DataModel/" + filename));
            await CurrentAppSimulator.ReloadSimulatorAsync(proxyFile);
        }*/

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
        /// session. The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            //DEBUG

            #region debug
            /*
            string xml_filename = XML_NOK;
            if (localSettings.Values.ContainsKey("XML"))
            {
                xml_filename = (string)localSettings.Values["XML"];
            }

            Debug.WriteLine("xml_filename: " + xml_filename);

            LoadInAppPurchaseProxyFileAsync(xml_filename);
            Debug.WriteLine("xml loaded");
            //debug only
            if (xml_filename == XML_OK)
            {
                Debug.WriteLine("xml_OK");
                buyProPackText.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }*/
            #endregion

            // If a hardware Back button is present, hide the "soft" Back button
            // in the command bar, and register a handler for presses of the hardware
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            }

            IEnumerable<GameResultsGroup> resultGroups = null;
            int counter = 0;
            while (resultGroups == null && counter < RETRY_COUNT)
            {
                resultGroups = await SampleDataSource.GetResultsAsync();
                counter++;
            }

            this.DefaultViewModel[FirstGroupName] = resultGroups.ElementAt(0);
            this.DefaultViewModel[SecondGroupName] = resultGroups.ElementAt(1);
            this.DefaultViewModel[ThirdGroupName] = resultGroups.ElementAt(2);
            this.DefaultViewModel[FourthGroupName] = resultGroups.ElementAt(3);

            PlayerGroup playerDatGroup = null;
            counter = 0;
            while (playerDatGroup == null && counter < RETRY_COUNT)
            {
                playerDatGroup = await SampleDataSource.GetPlayerGroupOne();
                counter++;
            }
            this.DefaultViewModel[PlayerGroupName] = playerDatGroup;
            
            
            bool saveData = false;

            #region newResults
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey("NewResults"))
            {
                // parse new results into Json
                JsonArray jsonArray = JsonArray.Parse(localSettings.Values["NewResults"].ToString());

                // result group where to add new results
                GameResultsGroup group = null;

                // every new result is handled
                foreach (JsonValue jsonVal in jsonArray)
                {
                    JsonObject jsonObject = jsonVal.GetObject();
                    Result newResult = new Result(jsonObject["ResultId"].GetString(),
                                                    jsonObject["ResultPlayerName"].GetString(),
                                                    jsonObject["ResultGameModeId"].GetString(),
                                                    jsonObject["Score"].GetNumber(),
                                                    jsonObject["ResultDateTime"].GetString());

                    // variable 'group' is set to the result group where the new results are going to be added
                    if (jsonObject["ResultGameModeId"].GetString() == "0")
                    {
                        group = (GameResultsGroup)DefaultViewModel[FirstGroupName];
                    }
                    else if (jsonObject["ResultGameModeId"].GetString() == "1")
                    {
                        group = (GameResultsGroup)DefaultViewModel[SecondGroupName];
                    }
                    else if (jsonObject["ResultGameModeId"].GetString() == "2")
                    {
                        group = (GameResultsGroup)DefaultViewModel[ThirdGroupName];
                    }
                    else if (jsonObject["ResultGameModeId"].GetString() == "3")
                    {
                        group = (GameResultsGroup)DefaultViewModel[FourthGroupName];
                    }
                    else
                    {
                        // if a new result does not contain gamemode information, it is omitted and the next one is processed
                        continue;
                    }

                    // check if placeholder results are to be removed
                    // if this is the first time results are added to this group, clear it first
                    if (!localSettings.Values.ContainsKey("ResultsAddedForGroup" + jsonObject["ResultGameModeId"].GetString()))
                    {
                        group.Results.Clear();

                        // set flag that results have been added to this group
                        localSettings.Values["ResultsAddedForGroup" + jsonObject["ResultGameModeId"].GetString()] = true;
                    }

                    // add new result to the gameResultsGroup
                    group.Results.Add(newResult);

                    // if there is at least two results, try to sort
                    if (group.Results.Count > 1)
                    {
                        // sort by moving the item upwards as long as necessary
                        for (int i = 0; i < group.Results.Count; ++i)
                        {
                            // move one upwards, if 
                            if (group.Results.Count > i - 1 &&
                                group.Results.ElementAt(group.Results.Count - 2 - i).Score < newResult.Score)
                            {
                                group.Results.Move(group.Results.Count - 1 - i, group.Results.Count - 2 - i);

                                if (group.Results.Count - 2 - i == 0)
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
                    // modify the player's gamesplayed data
                    string resultPlayerName = jsonObject["ResultPlayerName"].GetString();

                    int counter2 = 0;
                    PlayerGroup playerDataGroup = null;
                    while (playerDataGroup == null && counter2 < RETRY_COUNT)
                    {
                        playerDataGroup = await SampleDataSource.GetPlayerGroupOne();

                        counter2++;
                    }


                    //get player from the defaultviewmodel's player group
                    Player player = null;

                    foreach (Player p in playerDataGroup.Players)
                    {
                        if (p.PlayerName == resultPlayerName)
                        {
                            player = p;
                            break;
                        }
                    }
                    if (player != null)
                    {
                        player.PuttRating = await ratingCalculator.PlayerRating(player.PlayerName);
                        Debug.WriteLine("player: " + player.PlayerName + " rating: " + player.PuttRating);
                        player.GamesPlayed += 1;
                    }
                }
                // new results read from memory, remove them and set up save flag
                localSettings.Values.Remove("NewResults");
                saveData = true;
            }
            // new JYLYresults
            if (localSettings.Values.ContainsKey("NewJYLYResults"))
            {
                IEnumerable<JYLYResult> IJYLYResultGroup = null;
                int counter3 = 0;
                while (IJYLYResultGroup == null && counter3 < RETRY_COUNT)
                {
                    IJYLYResultGroup = await SampleDataSource.GetJYLYResultsAsync();
                    counter3++;
                }
                ObservableCollection<JYLYResult> JYLYResultGroup = (ObservableCollection<JYLYResult>)IJYLYResultGroup;

                // parse new results into Json
                JsonArray jsonArray = JsonArray.Parse(localSettings.Values["NewJYLYResults"].ToString());

                // result group where to add new results

                // every new result is handled
                foreach (JsonValue jsonVal in jsonArray)
                {
                    JsonObject jsonObject = jsonVal.GetObject();
                    JYLYResult newJYLYResult = new JYLYResult(jsonObject["Serie"].GetString(),
                                                    jsonObject["PlayerName"].GetString(),
                                                    jsonObject["Score"].GetNumber(),
                                                    jsonObject["DateTime"].GetString());
                    JYLYResultGroup.Add(newJYLYResult);
                }
                // new JYLYresults read from memory, remove them and set up save flag
                localSettings.Values.Remove("NewJYLYResults");
                saveData = true;
            }
            #endregion

            Debug.WriteLine("DII: resultGroups.count: " + resultGroups.Count<GameResultsGroup>());

            // check if this is the first boot or is the app udated to Abo or JYLY versions to save all data
            if (saveData || !localSettings.Values.ContainsKey("FirstBootDone") ||
                !localSettings.Values.ContainsKey("update1.2.0.0_done") ||
                localSettings.Values.ContainsKey("SaveAboGroup") ||
                localSettings.Values.ContainsKey("SaveJYLYGroup"))
            {
                dataSaver.SaveAllDataToJson();

                localSettings.Values["FileCreated"] = true;
                localSettings.Values["FirstBootDone"] = true;

                if (localSettings.Values.ContainsKey("SaveAboGroup"))
                {
                    localSettings.Values.Remove("SaveAboGroup");
                }
                if (localSettings.Values.ContainsKey("SaveJYLYGroup"))
                {
                    localSettings.Values.Remove("SaveJYLYGroup");
                }
            }
            if (localSettings.Values.ContainsKey(PIVOT_INDEX))
            {
                this.pivot.SelectedIndex = (int)localSettings.Values[PIVOT_INDEX];
                localSettings.Values.Remove(PIVOT_INDEX);
            }

            Debug.WriteLine("MainPage LoadState done");
        }
        

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache. Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/>.</param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
            }
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[PIVOT_INDEX] = this.pivot.SelectedIndex;
        }

        private async void showProPackMessage()
        {
            var msg = new MessageDialog(PROPACK_MESSAGE_TEXT, PROPACK_MESSAGE_TITLE);
            var buyBtn = new UICommand(BUY, new UICommandInvokedHandler(ShowProPackMessageCommandHandler));
            var cancelBtn = new UICommand(CANCEL, new UICommandInvokedHandler(ShowProPackMessageCommandHandler));
            msg.Commands.Add(buyBtn);
            msg.Commands.Add(cancelBtn);
            IUICommand result = await msg.ShowAsync();
        }

        public void ShowProPackMessageCommandHandler(IUICommand commandLabel)
        {
            var Action = commandLabel.Label;
            switch (Action)
            {
                case BUY:
                    p_BuyProPackClicked(this, new RoutedEventArgs());
                    break;

                case CANCEL:
                    break;
            }
        }

        private void PlayAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            bool canPlay = true;
            if (pivot.SelectedIndex == 2 || pivot.SelectedIndex == 3)
            {
                if (!(Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey("ProPackPurchased") || 
                    licenseInformation.ProductLicenses[PRO_PACK].IsActive))
                {
                    //TODO
                    canPlay = false;
                }
            }

            if (canPlay)
            {
                Frame.Navigate(typeof(PlayersPage), this.pivot.SelectedIndex);
            }
            else
            {
                //show propack message
                showProPackMessage();
            }
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

        private void aboutAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AboutPage));
        }

        public void TopResultsListTapped_EventHandler(object sender, RoutedEventArgs e)
        {
            GameModeEventArgs m = e as GameModeEventArgs;
            this.Frame.Navigate(typeof(HighScoresPage), m.GameModeId);
        }

        void PlayerTappedEventHandler(object sender, RoutedEventArgs e)
        {
            PlayerTappedEventArgs p = e as PlayerTappedEventArgs;
            Frame.Navigate(typeof(PlayerDetailPage), p.Player.PlayerName);
        }
        private void DeletePlayerClickedEventHandler(object sender, RoutedEventArgs e)
        {
            PlayerTappedEventArgs p = e as PlayerTappedEventArgs;
            var group = this.DefaultViewModel[PlayerGroupName] as PlayerGroup;
            group.Players.Remove(p.Player);
            dataSaver.SaveAllDataToJson();
        }

        private async void AddPlayerAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            // show dialog where new players are added
            ContentDialogResult dialogResult = await addPlayerDialog.ShowAsync();
            if (dialogResult == ContentDialogResult.Primary)
            {
                // add new player to the view model
                PlayerGroup playerGroup = this.DefaultViewModel[PlayerGroupName] as PlayerGroup;
                int nextPlayerId = 0;
                // check next player id from settings and increment it
                if (localSettings.Values.ContainsKey(NEXT_PLAYER_ID))
                {
                    nextPlayerId = (int)localSettings.Values[NEXT_PLAYER_ID];
                }
                localSettings.Values[NEXT_PLAYER_ID] = (nextPlayerId + 1);

                int puttRating = await ratingCalculator.PlayerRating(addPlayerDialog.GetPlayerName());
                int totalGamesPlayed = await ratingCalculator.GamesPlayed(addPlayerDialog.GetPlayerName(), true);
                // create new player and add it to the view model
                Player newPlayer = new Player(nextPlayerId.ToString(), addPlayerDialog.GetPlayerName(),
                                              totalGamesPlayed, puttRating);
                playerGroup.Players.Add(newPlayer);

                // Scroll the new item into view.
                dataSaver.SaveAllDataToJson();
                
                // add player to selected players localSettings
                // save selected players
                string selectedPlayers = "";
                if (localSettings.Values.ContainsKey("SelectedPlayers"))
                {
                    selectedPlayers = (string)localSettings.Values["SelectedPlayers"];
                }
                selectedPlayers = selectedPlayers + newPlayer.PlayerName + ";";
                localSettings.Values["SelectedPlayers"] = selectedPlayers;


                Debug.WriteLine("Add3");
                // ask players listView to scroll down if needed
                MainPagePivotItemPlayers p = ((PivotItem)(pivot.Items.ElementAt(4))).Content as MainPagePivotItemPlayers;
                if (p != null)
                {
                    p.scrollIntoNewPlayer(newPlayer);
                }

                Debug.WriteLine("Add4");

            }
        }

        private async void PlayersPivotItem_Loaded(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine("PlayersPivotItem Loaded start");

            // sort players by puttRating
            PlayerGroup playerDataGroup = null;
            int counter = 0;
            while (playerDataGroup == null && counter < RETRY_COUNT)
            {
                playerDataGroup = await SampleDataSource.GetPlayerGroupOne();
                counter++;
            }
            this.DefaultViewModel[PlayerGroupName] = playerDataGroup;

            var playerGroup = playerDataGroup;

            List<Player> playersList = new List<Player>();
            var players = playerGroup.Players;

            // add players to list
            foreach (Player p in players)
            {
                playersList.Add(p);
            }

            // sort the list
            playersList.Sort((x, y) => x.PuttRating.CompareTo(y.PuttRating));

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            string selectedPlayers = "";

            if (localSettings.Values.ContainsKey("SelectedPlayers"))
            {
                selectedPlayers = (string)localSettings.Values["SelectedPlayers"];
            }

            // add players to the listview in rating order
            playerGroup.Players.Clear();

            for (int i = playersList.Count - 1; i >= 0; --i)
            {
                playerGroup.Players.Add(playersList.ElementAt(i));
            }
            var playersPivot = (pivot.Items[4] as PivotItem).Content as MainPagePivotItemPlayers;
            if (playersPivot != null)
            {
                playersPivot.UpdateListView();
            }

            localSettings.Values["SelectedPlayers"] = selectedPlayers;

            if (NeedToshowProPackOffer() && playersPivot != null)
            {
                playersPivot.showProPackOffer();
            }
            else if (playersPivot != null)
            {
                playersPivot.hideProPackOffer();
            }
        }

        // checks if there is a need to show proPack offer:
        // there is at least one player, with rating greater than 0 and propack is not purchased already
        // all pivot items need to be created as well
        private bool NeedToshowProPackOffer()
        {
            if (!defaultViewModel.ContainsKey(PlayerGroupName))
            {
                return false;
            }
            var playerGroup = defaultViewModel[PlayerGroupName] as PlayerGroup;

            bool showOffer = false;
            foreach (Player p in playerGroup.Players)
            {
                if (p.PuttRating > 0)
                {
                    showOffer = true;
                    break;
                }
            }

            if (playerGroup.Players.Count == 0)
            {
                if (pivot.Items.Count == 3)
                {
                    MainPagePivotItemPlayers p = (pivot.Items[2] as PivotItem).Content as MainPagePivotItemPlayers;
                    if (p != null)
                    {
                        return false;
                    }
                }
            }
            else if (!(Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey("ProPackPurchased") || licenseInformation.ProductLicenses[PRO_PACK].IsActive) && showOffer)
            {
                if (pivot.Items.Count >= 4)
                {
                    MainPagePivotItemPlayers p = (pivot.Items[pivot.Items.Count - 1] as PivotItem).Content as MainPagePivotItemPlayers;
                    if (p != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void buyProPackAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BuyProPackPage));
        }

        public void buyProPack_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            Frame.Navigate(typeof(BuyProPackPage));
        }

        void pivot_PivotItemLoading(Pivot sender, PivotItemEventArgs args)
        {
            if (args.Item.Content != null)
            {
                return;
            }

            // create new pivot item and set it to the view according to what view has been swiped to be visible
            Pivot pivot = (Pivot)sender;

            if (args.Item == pivot.Items[0])
            {
                MainPivotItem0 pivotItem = new MainPivotItem0();
                pivotItem.TopResultsListTapped += TopResultsListTapped_EventHandler;
                args.Item.Content = pivotItem;
            }
            else if (args.Item == pivot.Items[1])
            {
                MainPivotItem1 pivotItem = new MainPivotItem1();
                pivotItem.TopResultsListTapped += TopResultsListTapped_EventHandler;
                args.Item.Content = pivotItem;
            }
            else if (args.Item == pivot.Items[2])
            {
                MainPivotItem2 pivotItem = new MainPivotItem2();
                pivotItem.TopResultsListTapped += TopResultsListTapped_EventHandler;
                args.Item.Content = pivotItem;
            }
            else if (args.Item == pivot.Items[3])
            {
                MainPivotItem3 pivotItem = new MainPivotItem3();
                pivotItem.TopResultsListTapped += TopResultsListTapped_EventHandler;
                args.Item.Content = pivotItem;
            }
            else if (args.Item == pivot.Items[4])
            {
                MainPagePivotItemPlayers pivotItem = new MainPagePivotItemPlayers();
                pivotItem.BuyProPackClicked += p_BuyProPackClicked;
                pivotItem.PlayerTapped += PlayerTappedEventHandler;
                pivotItem.DeletePlayerClicked += DeletePlayerClickedEventHandler;
                args.Item.Content = pivotItem;

                //check propack offer display
                if (NeedToshowProPackOffer())
                {
                    pivotItem.showProPackOffer();
                }
            }
        }

        void p_BuyProPackClicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BuyProPackPage));
        }


        // scroll views up every time they are loaded, this applies better UX when coming back from the game views
        private void PivotItem0_Loaded(object sender, RoutedEventArgs e)
        {
            MainPivotItem0 p = ((pivot.Items[0] as PivotItem).Content) as MainPivotItem0;
            if (p != null)
            {
                p.ScrollViewUp();
            }
        }
        private void PivotItem1_Loaded(object sender, RoutedEventArgs e)
        {
            MainPivotItem1 p = ((pivot.Items[1] as PivotItem).Content) as MainPivotItem1;
            if (p != null)
            {
                p.ScrollViewUp();
            }
        }
        private void PivotItem2_Loaded(object sender, RoutedEventArgs e)
        {
            MainPivotItem2 p = ((pivot.Items[2] as PivotItem).Content) as MainPivotItem2;
            if (p != null)
            {
                p.ScrollViewUp();
            }
        }
        private void PivotItem3_Loaded(object sender, RoutedEventArgs e)
        {
            MainPivotItem3 p = ((pivot.Items[3] as PivotItem).Content) as MainPivotItem3;
            if (p != null)
            {
                p.ScrollViewUp();
            }
        }
    }
}