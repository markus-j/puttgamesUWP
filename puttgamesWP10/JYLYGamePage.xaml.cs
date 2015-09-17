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

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace puttgamesWP10
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class JYLYGamePage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        private const string EXIT_CONFIRMATION_TEXT = "Results are not saved.";
        private const string EXIT_CONFIRMATION_TITLE = "Exit game?";
        private const string SAVE_CONFIRMATION_TEXT = "Results will be saved.";
        private const string SAVE_CONFIRMATION_TITLE = "Finish game?";
        private const string FINISH = "finish";
        private const string CANCEL = "cancel";
        private const string EXIT = "exit";
        private const string resultGroupName = "resultGroup";
        private const string DONE = "ok";

        private const string EXIT_CONFIRMATION_TEXT_2 = "Unfinished games will be lost. Finished games will be saved.";
        private const string SAVE_UNCOMPLETED_CONFIRMATION_TEXT = "Unfinished games will be lost. Finished games will be saved.";
        private const string SAVE_UNCOMPLETED_CONFIRMATION_TITLE = "All players not finished";
        private RelayCommand _checkedGoBackCommand;

        public JYLYGamePage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            // add check before going back
            _checkedGoBackCommand = new RelayCommand(
                                    () => this.showExitConfirmation(),
                                    () => this.CanCheckGoBack()
                                );

            navigationHelper.GoBackCommand = _checkedGoBackCommand;
            pivot.SelectionChanged += pivot_SelectionChanged;
        }

        private bool CanCheckGoBack()
        {
            return true;
        }
        private async void showExitConfirmation()
        {
            var msg = new MessageDialog(EXIT_CONFIRMATION_TEXT, EXIT_CONFIRMATION_TITLE);
            var okBtn = new UICommand(EXIT, new UICommandInvokedHandler(ConfirmationCommandHandler));
            var cancelBtn = new UICommand(CANCEL, new UICommandInvokedHandler(ConfirmationCommandHandler));
            msg.Commands.Add(okBtn);
            msg.Commands.Add(cancelBtn);
            IUICommand result = await msg.ShowAsync();
        }
        
        // if there are uncompleted results, show warning
        private async void showUncompletedResultsWarning()
        {
            var msg = new MessageDialog(SAVE_UNCOMPLETED_CONFIRMATION_TEXT, SAVE_UNCOMPLETED_CONFIRMATION_TITLE);
            var okBtn = new UICommand(FINISH, new UICommandInvokedHandler(SaveConfirmationCommandHandler));
            var cancelBtn = new UICommand(CANCEL, new UICommandInvokedHandler(SaveConfirmationCommandHandler));
            msg.Commands.Add(okBtn);
            msg.Commands.Add(cancelBtn);
            IUICommand result = await msg.ShowAsync();
        }

        //handle save confirmation handler
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
        // Handle the back confirmation selection
        public void ConfirmationCommandHandler(IUICommand commandLabel)
        {
            var Action = commandLabel.Label;
            switch (Action)
            {
                case EXIT:
                    if (Frame.CanGoBack)
                    {
                        this.Frame.BackStack.RemoveAt(1);
                        this.navigationHelper.GoBack();
                    }
                    break;

                case CANCEL:
                    break;
            }
        }
        // enable or disable save button according to the completion stage
        void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PivotItem item = pivot.Items[pivot.SelectedIndex] as PivotItem;
            JYLYPivotItem JYLYitem = item.Content as JYLYPivotItem;

            if (JYLYitem.isCompleted())
            {
                SaveAppBarButton.IsEnabled = true;
            }
            else
            {
                SaveAppBarButton.IsEnabled = false;
            }
        }

        // save button handler
        private void SaveAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            bool showWarning = false;
            // check if there is incompleted games, if yes, warning note
            for (int i = 0; i < pivot.Items.Count; ++i)
            {
                PivotItem item = pivot.Items[i] as PivotItem;
                JYLYPivotItem JYLYitem = item.Content as JYLYPivotItem;
                if (JYLYitem != null)
                {
                    if (!JYLYitem.isCompleted())
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

        // saves both the scores and the full JYLY scores for further use in future (export, graphs, statisctics, etc.)
        private void getAndSaveResults()
        {
            JsonArray newResults = new JsonArray();
            JsonArray newJYLYResults = new JsonArray();

            for (int i = 0; i < pivot.Items.Count; ++i)
            {
                PivotItem item = pivot.Items[i] as PivotItem;
                JYLYPivotItem pivotItem = item.Content as JYLYPivotItem;

                int currentPlayerScore = pivotItem.getScore();

                JsonObject newResult = new JsonObject();
                newResult.Add("ResultId", JsonValue.CreateStringValue(DateTime.Now.ToString()));
                newResult.Add("ResultPlayerName", JsonValue.CreateStringValue(item.Header.ToString()));
                newResult.Add("ResultGameModeId", JsonValue.CreateStringValue("3"));
                newResult.Add("Score", JsonValue.CreateNumberValue(currentPlayerScore));
                newResult.Add("ResultDateTime", JsonValue.CreateStringValue(DateTime.Now.ToString("G")));
                newResults.Add(newResult);

                // Add JYLY results to a separate json object
                JsonObject newJYLYResult = new JsonObject();
                newJYLYResult.Add("Serie", JsonValue.CreateStringValue(pivotItem.getState()));
                newJYLYResult.Add("PlayerName", JsonValue.CreateStringValue(item.Header.ToString()));
                newJYLYResult.Add("Score", JsonValue.CreateNumberValue(currentPlayerScore));
                newJYLYResult.Add("DateTime", JsonValue.CreateStringValue(DateTime.Now.ToString("G")));
                newJYLYResults.Add(newJYLYResult);
            }
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["NewResults"] = newResults.Stringify();
            localSettings.Values["NewJYLYResults"] = newJYLYResults.Stringify();
            
            Debug.WriteLine(newResults.Stringify());
            Debug.WriteLine(newJYLYResults.Stringify());

            this.Frame.BackStack.RemoveAt(1);
            this.navigationHelper.GoBack();
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
            string players = "";

            if (e.PageState == null)
            {
                players = e.NavigationParameter.ToString();
            }
            else if (e.PageState.ContainsKey("Players"))
            {
                players = (string)e.PageState["Players"];
            }
            else
            {
                // we have screwed up smthing
            }

            List<string> selectedPlayers = new List<string>();

            parseParametersToList(players, ref selectedPlayers);

            // create pivot items (came pages) for each player
            for (int i = 0; i < selectedPlayers.Count; ++i)
            {
                PivotItem pivotItem = new PivotItem();
                JYLYPivotItem JYLYItem = new JYLYPivotItem();
                
                JYLYItem.PlayerCompletedGame += JYLYItem_PlayerCompletedGame;
                JYLYItem.PlayerUncompletedGame += JYLYItem_PlayerUncompletedGame;

                pivotItem.Content = JYLYItem;
                pivotItem.Header = selectedPlayers[i];
                this.pivot.Items.Add(pivotItem);
            }

            // if there was a saved state, set counters to their values

            if (e.PageState != null)
            {
                for (int i = 0; i < pivot.Items.Count; ++i)
                {
                    //Debug.WriteLine("i: " + i);
                    PivotItem item = pivot.Items[i] as PivotItem;
                    JYLYPivotItem pivotItem = item.Content as JYLYPivotItem;
                    pivotItem.setState((string)e.PageState[i.ToString()]);
                }
                if (e.PageState.ContainsKey("SelectedIndex"))
                {
                    pivot.SelectedIndex = (int)e.PageState["SelectedIndex"];
                }
            }
        }
        void JYLYItem_PlayerCompletedGame(object sender, RoutedEventArgs e)
        {
            SaveAppBarButton.IsEnabled = true;
        }
        void JYLYItem_PlayerUncompletedGame(object sender, RoutedEventArgs e)
        {
            SaveAppBarButton.IsEnabled = false;
        }
       
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            string players = "";
            for (int i = 0; i < pivot.Items.Count; ++i)
            {
                PivotItem item = pivot.Items[i] as PivotItem;
                JYLYPivotItem JYLYitem = item.Content as JYLYPivotItem;
                players += item.Header.ToString() + ";";
                e.PageState.Add(i.ToString(), JYLYitem.getState() );
            }
            Debug.WriteLine("saved players: " + players);
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
        }

        #endregion
    }
}
