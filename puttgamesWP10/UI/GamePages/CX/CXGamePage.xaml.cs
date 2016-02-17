using puttgamesWP10.Common;
using puttgamesWP10.Data;
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

using System.Globalization;


namespace puttgamesWP10
{
    public sealed partial class CXGamePage : Page
    {
        private const string EXIT_CONFIRMATION_TEXT = "Unfinished games will be lost. Finished games will be saved.";
        private const string EXIT_CONFIRMATION_TITLE = "Exit game?";
        private const string SAVE_UNCOMPLETED_CONFIRMATION_TEXT = "Unfinished games will be lost. Finished games will be saved.";
        private const string SAVE_UNCOMPLETED_CONFIRMATION_TITLE = "All players not finished";
        private const string resultGroupName = "resultGroup";

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();


        public CXGamePage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            
            pivot.SelectionChanged += pivot_SelectionChanged;
            
            // new back button handling
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

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

        // return false if no check is needed
        private bool CanCheckGoBack()
        {
            return true;
        }
        private async void showExitConfirmation()
        {
            var msg = new MessageDialog(EXIT_CONFIRMATION_TEXT, EXIT_CONFIRMATION_TITLE);
            var okBtn = new UICommand("Exit", new UICommandInvokedHandler(ConfirmationCommandHandler));
            var cancelBtn = new UICommand("Cancel", new UICommandInvokedHandler(ConfirmationCommandHandler));
            msg.Commands.Add(okBtn);
            msg.Commands.Add(cancelBtn);
            IUICommand result = await msg.ShowAsync();
        }

        // Handle the confirmation selection
        public void ConfirmationCommandHandler(IUICommand commandLabel)
        {
            var Action = commandLabel.Label;
            switch (Action)
            {
                case "Exit":
                    if (Frame.CanGoBack)
                    {
                        getAndSaveResults();
                    }
                    break;

                case "Cancel":
                    break;
            }
        }

        // enable or disable the save button according to the status of 
        // completion of the game of the particular player
        void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PivotItem item = pivot.Items[pivot.SelectedIndex] as PivotItem;
            CXPivotItem CXitem = item.Content as CXPivotItem;

            if ( CXitem.isCompleted())
            {
                SaveAppBarButton.IsEnabled = true;
            }
            else
            {
                SaveAppBarButton.IsEnabled = false;
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


        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            }

            // set results to the viewmodel

            //create pivot items based on the selected players
            //parse selected players from navigation parameter string or local settings to a list

            string players = "";

            if (e.PageState == null)
            {
                players = e.NavigationParameter.ToString();
            }
            else
            {
                players = (string)e.PageState["Players"];
            }

            List<string> selectedPlayers = new List<string>();

            parseParametersToList(players, ref selectedPlayers);

            // create pivot items (came pages) for each player
            for (int i = 0; i < selectedPlayers.Count; ++i)
            {
                PivotItem pivotItem = new PivotItem();
                CXPivotItem cxItem = new CXPivotItem();
                
                cxItem.PlayerCompletedGame += cxItem_PlayerCompletedGame;
                cxItem.PlayerUncompletedGame += cxItem_PlayerUncompletedGame;
                pivotItem.Content = cxItem;
                pivotItem.Header = selectedPlayers[i];
                this.pivot.Items.Add(pivotItem);
            }

            // if there was a saved state, set counters to their values
            // format of the saved string is "PlayerName;ThrowsIn;ThrowsOut"
            if (e.PageState != null)
            {
                for (int i = 0; i < pivot.Items.Count; ++i)
                {
                    PivotItem item = pivot.Items[i] as PivotItem;
                    CXPivotItem CXitem = item.Content as CXPivotItem;
                    
                    string throws = (string)e.PageState[i.ToString()];
                    string throwsIn = throws.Substring(0, throws.IndexOf(";"));
                    string throwsOut = throws.Substring(throws.IndexOf(";") + 1);
                    CXitem.setThrows(throwsIn,throwsOut);
                }
                if (e.PageState.ContainsKey("SelectedIndex"))
                {
                    pivot.SelectedIndex = (int)e.PageState["SelectedIndex"];
                }
            }
        }
        
        // enable or disable the save button when player completes of uncompletes their game
        void cxItem_PlayerCompletedGame(object sender, RoutedEventArgs e)
        {
            SaveAppBarButton.IsEnabled = true;
        }
        void cxItem_PlayerUncompletedGame(object sender, RoutedEventArgs e)
        {
            SaveAppBarButton.IsEnabled = false;
        }

        private void parseParametersToList(string navigationParameters, ref List<string> parameterList)
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
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
            }

            string players = "";
            for (int i = 0; i < pivot.Items.Count; ++i)
            {
                PivotItem item = pivot.Items[i] as PivotItem;
                CXPivotItem CXitem = item.Content as CXPivotItem;
                players += item.Header.ToString() + ";";
                e.PageState.Add(i.ToString(), CXitem.getThrowsIn() + ";" + CXitem.getThrowsOut() );
            }
            Debug.WriteLine("saved players: " + players);
            e.PageState.Add("Players", players);
            e.PageState.Add("SelectedIndex", pivot.SelectedIndex);
        }

        #region NavigationHelper registration

      
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

        private void SaveAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            bool showWarning = false;
            // check if there is incompleted games, if yes, warning note
            for (int i = 0; i < pivot.Items.Count; ++i)
            {
                PivotItem item = pivot.Items[i] as PivotItem;
                CXPivotItem CXitem = item.Content as CXPivotItem;
                if (CXitem != null)
                {
                    if (!CXitem.isCompleted())
                    {
                        showWarning = true;
                        break;
                    }
                }
            }
            if (showWarning)
            {
                //show warning
                showUncompletedResultsWarning();
            }
            else
            {
                getAndSaveResults();
            }
        }

        private void getAndSaveResults()
        {
            // get all results to a List<List<int>>, both completed and uncompleted
            JsonArray newResults = new JsonArray();

            for (int i = 0; i < pivot.Items.Count; ++i)
            {
                //Debug.WriteLine("pivot.Items.Count: " + pivot.Items.Count + "i: " + i);
                
                PivotItem item = pivot.Items[i] as PivotItem;
                CXPivotItem CXitem = item.Content as CXPivotItem;
                
                int currentPlayerIn = CXitem.getThrowsIn();
                int currentPlayerOut = CXitem.getThrowsOut();
                
                if (currentPlayerIn + currentPlayerOut == 100)
                {
                    // player has completed the game
                    JsonObject newResult = new JsonObject();
                    newResult.Add("ResultId", JsonValue.CreateStringValue(DateTime.Now.ToString()));
                    newResult.Add("ResultPlayerName", JsonValue.CreateStringValue(item.Header.ToString()));
                    newResult.Add("ResultGameModeId", JsonValue.CreateStringValue("1"));
                    newResult.Add("Score", JsonValue.CreateNumberValue(currentPlayerIn));
                    newResult.Add("ResultDateTime", JsonValue.CreateStringValue(DateTime.Now.ToString("G")));
                    newResults.Add(newResult);
                    
                    //Debug.WriteLine("New result added for: " + item.Header.ToString());
                }
            }
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["NewResults"] = newResults.Stringify();

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
            }
            this.Frame.BackStack.RemoveAt(1);
            this.navigationHelper.GoBack();
        }

        private async void showUncompletedResultsWarning()
        {
            var msg = new MessageDialog(SAVE_UNCOMPLETED_CONFIRMATION_TEXT, SAVE_UNCOMPLETED_CONFIRMATION_TITLE);
            var okBtn = new UICommand("ok", new UICommandInvokedHandler(SaveConfirmationCommandHandler));
            var cancelBtn = new UICommand("cancel", new UICommandInvokedHandler(SaveConfirmationCommandHandler));
            msg.Commands.Add(okBtn);
            msg.Commands.Add(cancelBtn);
            IUICommand result = await msg.ShowAsync();
        }
        public void SaveConfirmationCommandHandler(IUICommand commandLabel)
        {
            var Action = commandLabel.Label;
            switch (Action)
            {
                case "ok":
                    getAndSaveResults();
                    break;

                case "cancel":
                    break;
            }
        }

    }
}
