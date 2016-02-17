using puttgamesWP10.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using Windows.Phone.UI.Input;
using Windows.UI.Popups;
using Windows.Data.Json;
using Windows.UI.Core;

namespace puttgamesWP10
{
    /// <summary>
    /// Abo game page includes one or more AboPivotItems, one for each player.
    /// </summary>
    
    public sealed partial class AboGamePage : Page
    {
        private const string EXIT_CONFIRMATION_TEXT = "Results are not saved.";
        private const string EXIT_CONFIRMATION_TITLE = "Exit game?";
        private const string SAVE_CONFIRMATION_TEXT = "Results will be saved.";
        private const string SAVE_CONFIRMATION_TITLE = "Finish game?";
        private const string FINISH = "finish";
        private const string CANCEL = "cancel";
        private const string EXIT = "exit";
        private const string resultGroupName = "resultGroup";
        private const string DONE = "ok";
        private const string GAME_MODE_ID = "2";

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public AboGamePage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            // new back button handling
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            }
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            e.Handled = true;
            showExitConfirmation();
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            showExitConfirmation();
        }

        // shows exit confirmation popup
        // TODO popup is not currently shown because of new back button handling
        private async void showExitConfirmation()
        {
            var msg = new MessageDialog(EXIT_CONFIRMATION_TEXT, EXIT_CONFIRMATION_TITLE);
            var okBtn = new UICommand(EXIT, new UICommandInvokedHandler(ConfirmationCommandHandler));
            var cancelBtn = new UICommand(CANCEL, new UICommandInvokedHandler(ConfirmationCommandHandler));
            msg.Commands.Add(okBtn);
            msg.Commands.Add(cancelBtn);
            IUICommand result = await msg.ShowAsync();
        }

        //Save confirmation is shown always to prevent erroneus clicks
        private async void showSaveConfirmation()
        {
            var msg = new MessageDialog(SAVE_CONFIRMATION_TEXT, SAVE_CONFIRMATION_TITLE);
            var okBtn = new UICommand(FINISH, new UICommandInvokedHandler(SaveConfirmationCommandHandler));
            var cancelBtn = new UICommand(CANCEL, new UICommandInvokedHandler(SaveConfirmationCommandHandler));
            msg.Commands.Add(okBtn);
            msg.Commands.Add(cancelBtn);
            IUICommand result = await msg.ShowAsync();
        }
        public void SaveConfirmationCommandHandler(IUICommand commandLabel)
        {
            var Action = commandLabel.Label;
            switch (Action)
            {
                case FINISH:
                    if (Frame.CanGoBack)
                    {
                        getAndSaveResults();
                    }
                    break;

                case CANCEL:
                    break;
            }
        }

        // Handle the confirmation selection
        public void ConfirmationCommandHandler(IUICommand commandLabel)
        {
            var Action = commandLabel.Label;
            switch (Action)
            {
                case EXIT:
                    if (Frame.CanGoBack)
                    {
                        if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
                        {
                            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
                        }

                        this.Frame.BackStack.RemoveAt(1);
                        this.navigationHelper.GoBack();
                    }
                    break;

                case CANCEL:
                    break;
            }
        }

        private void SaveAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            showSaveConfirmation();
        }

        //gets results for all players, puts them on temp storage to localSettings for mainpage to save
        private void getAndSaveResults()
        {
            // get all results to a List<List<int>>, both completed and uncompleted
            JsonArray newResults = new JsonArray();

            for (int i = 0; i < pivot.Items.Count; ++i)
            {
                PivotItem item = pivot.Items[i] as PivotItem;
                AboPivotItem pivotItem = item.Content as AboPivotItem;

                int currentPlayerScore = pivotItem.getScore();

                JsonObject newResult = new JsonObject();
                newResult.Add("ResultId", JsonValue.CreateStringValue(DateTime.Now.ToString()));
                newResult.Add("ResultPlayerName", JsonValue.CreateStringValue(item.Header.ToString()));
                newResult.Add("ResultGameModeId", JsonValue.CreateStringValue(GAME_MODE_ID));
                newResult.Add("Score", JsonValue.CreateNumberValue(currentPlayerScore));
                newResult.Add("ResultDateTime", JsonValue.CreateStringValue(DateTime.Now.ToString("G")));
                newResults.Add(newResult);
            }
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["NewResults"] = newResults.Stringify();

            Debug.WriteLine(newResults.Stringify());

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
            }
            this.Frame.BackStack.RemoveAt(1);
            this.navigationHelper.GoBack();
        }
        
        // Navigation parameters parsed to a list
        private void parseParametersToList(string navigationParameters, ref List<string> parameterList)
        {
            while (navigationParameters.Length > 0)
            {
                parameterList.Add(navigationParameters.Substring(0, navigationParameters.IndexOf(";")));

                //if there is still something after the next ;
                if (navigationParameters.IndexOf(";") + 1 < navigationParameters.Length)
                {
                    navigationParameters = navigationParameters.Substring(navigationParameters.IndexOf(";") + 1);
                }
                // if the name was the last in the list, empty the string so the loop ends
                else
                {
                    navigationParameters = "";
                }
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

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // set results to the viewmodel

            //create pivot items based on the selected players
            //parse selected players from navigation parameter string or local settings to a list

            string players = "";

            // first navigation to the page
            if (e.PageState == null)
            {
                players = e.NavigationParameter.ToString();
            }
            // there is a saved state, resume
            else if (e.PageState.ContainsKey("Players"))
            {
                players = (string)e.PageState["Players"];
            }
            
            List<string> selectedPlayers = new List<string>();
            parseParametersToList(players, ref selectedPlayers);

            // create pivot items (game pages) for each player
            for (int i = 0; i < selectedPlayers.Count; ++i)
            {
                PivotItem pivotItem = new PivotItem();
                AboPivotItem cxItem = new AboPivotItem();
                pivotItem.Content = cxItem;
                pivotItem.Header = selectedPlayers[i];
                this.pivot.Items.Add(pivotItem);
            }

            // if there was a saved state, set counters to their values
            if (e.PageState != null)
            {
                for (int i = 0; i < pivot.Items.Count; ++i)
                {
                    PivotItem item = pivot.Items[i] as PivotItem;
                    AboPivotItem pivotItem = item.Content as AboPivotItem;
                    pivotItem.setState((string)e.PageState[i.ToString()]);
                }
                if (e.PageState.ContainsKey("SelectedIndex"))
                {
                    pivot.SelectedIndex = (int)e.PageState["SelectedIndex"];
                }
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // save each player's game state
            string players = "";
            for (int i = 0; i < pivot.Items.Count; ++i)
            {
                PivotItem item = pivot.Items[i] as PivotItem;
                AboPivotItem pivotItem = item.Content as AboPivotItem;
                players += item.Header.ToString() + ";";
                e.PageState.Add(i.ToString(), pivotItem.getState());
                
                Debug.WriteLine("pagestate.add: i:" + i.ToString() + " " + pivotItem.getState());
            }
            e.PageState.Add("Players", players);
            e.PageState.Add("SelectedIndex", pivot.SelectedIndex);
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
            SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
        }

        #endregion
    }
}