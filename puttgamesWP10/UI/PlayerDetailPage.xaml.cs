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
using Windows.ApplicationModel.Store;
using Windows.Storage;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace puttgamesWP10
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlayerDetailPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private const string NO_SKILL_LEVEL = "n/a";
        private List<TextBlock> gamesPlayedTextBlocks = new List<TextBlock>();
        private List<TextBlock> bestScoreTextBlocks = new List<TextBlock>();
        private List<TextBlock> avgScoreTextBlocks = new List<TextBlock>();
        private List<TextBlock> skillLevelTextBlocks = new List<TextBlock>();
        
        private const string SKILL_LEVEL_BEGINNER = "Beginner";
        private const string SKILL_LEVEL_INTERMEDIATE = "Intermediate";
        private const string SKILL_LEVEL_ADVANCED = "Advanced";
        private const string SKILL_LEVEL_PRO = "Pro";
        private const string SKILL_LEVEL_EXCEPTIONAL = "Exceptional";
        private const string SKILL_LEVEL_AMAZING = "Amazing";

        private const string ABO_SKILL_LEVEL_BEGINNER = "'Raisio'";
        private const string ABO_SKILL_LEVEL_INTERMEDIATE = "'Kaarina'";
        private const string ABO_SKILL_LEVEL_ADVANCED = "'Masku'";
        private const string ABO_SKILL_LEVEL_PRO = "'Lauste'";
        private const string ABO_SKILL_LEVEL_AMAZING = "'Move to Åbo!'";
        
        private const string PlayerGroupName = "PlayerGroup";
        private LicenseInformation licenseInformation;
        private const string PRO_PACK = "ProPack";
        private const string BUY_PRO_PACK_TEXT = "in ProPack only";
        private const int RETRY_COUNT = 20;

        //debug 
        //private const string XML_NOK = "in-app-purchase_nok.xml";
        //private const string XML_OK = "in-app-purchase_ok.xml";
        

        public PlayerDetailPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            // 
            licenseInformation = CurrentApp.LicenseInformation;
            //licenseInformation = CurrentAppSimulator.LicenseInformation;


            gamesPlayedTextBlocks.Add(mmxvGamesPlayed);
            gamesPlayedTextBlocks.Add(cxGamesPlayed);
            gamesPlayedTextBlocks.Add(AboGamesPlayed);
            gamesPlayedTextBlocks.Add(JYLYGamesPlayed);

            bestScoreTextBlocks.Add(mmxvBestScore);
            bestScoreTextBlocks.Add(cxBestScore);
            bestScoreTextBlocks.Add(AboBestScore);
            bestScoreTextBlocks.Add(JYLYBestScore);

            avgScoreTextBlocks.Add(mmxvAvgScore);
            avgScoreTextBlocks.Add(cxAvgScore);
            avgScoreTextBlocks.Add(AboAvgScore);
            avgScoreTextBlocks.Add(JYLYAvgScore);

            skillLevelTextBlocks.Add(mmxvSkillLevel);
            skillLevelTextBlocks.Add(cxSkillLevel);
            skillLevelTextBlocks.Add(AboSkillLevel);
            skillLevelTextBlocks.Add(JYLYSkillLevel);
        }

        /*
        private async void LoadInAppPurchaseProxyFileAsync(string filename)
        {
            StorageFile proxyFile =
                await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///DataModel/" + filename));
            await CurrentAppSimulator.ReloadSimulatorAsync(proxyFile);
        }*/

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
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
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            //DEBUG
            #region debug
            /*
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            string xml_filename = XML_NOK;
            if (localSettings.Values.ContainsKey("XML"))
            {
                xml_filename = (string)localSettings.Values["XML"];
            }

            Debug.WriteLine("xml_filename: " + xml_filename);

            LoadInAppPurchaseProxyFileAsync(xml_filename);
            */
            #endregion
            
            // get player name and set it to the header
            string currentPlayerName = (string)e.NavigationParameter;

            Player player = null;
            int counter = 0;
            while (player == null && counter < RETRY_COUNT)
            {
                player = await SampleDataSource.GetPlayerAsync(currentPlayerName);
                counter++;
            }
            playerName.Text = currentPlayerName;

            

            if(Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey("ProPackPurchased") || licenseInformation.ProductLicenses[PRO_PACK].IsActive)
            {
                rating.Text = player.PuttRating.ToString();
                rating.Visibility = Windows.UI.Xaml.Visibility.Visible;
                buyProPackText.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                rating.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                buyProPackText.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }

            // Get results for all game modes
            IEnumerable<GameResultsGroup> resultGroups = null;
            int counter2 = 0;
            while (resultGroups == null && counter2 < 20)
            {
                resultGroups = await SampleDataSource.GetResultsAsync();
                counter2++;
            }

            
            if (resultGroups == null)
            {
                Debug.WriteLine("groups NULL");
            }

            List<GameResultsGroup> allResultGroups = new List<GameResultsGroup>();
            allResultGroups.Add(resultGroups.ElementAt(0));
            allResultGroups.Add(resultGroups.ElementAt(1));
            allResultGroups.Add(resultGroups.ElementAt(2));
            allResultGroups.Add(resultGroups.ElementAt(3));

            // count game mode games played for player
            // get highest score
            // calculate avg score
            
            int totalGamesPlayedNr = 0;
            
            // go through the result groups
            for(int i = 0; i < allResultGroups.Count; ++i)
            {
                double highestScore = 0;
                List<Result> gameModeResultsForPlayer = new List<Result>();
                // go through the results in one result group
                foreach (Result result in allResultGroups.ElementAt(i).Results)
                {
                    if (result.ResultPlayerName == currentPlayerName)
                    {
                        
                        gameModeResultsForPlayer.Add(result);
                        totalGamesPlayedNr++;
            
                        if (result.Score > highestScore)
                        {
                            highestScore = result.Score;
                        }
                    }
                }
                //playerResultsInAllModes.Add(gameModeResultsForPlayer);
                //gameModeAvgRatings.Add(singleResultRatings.Average());
                //Debug.WriteLine("singleresultratings.average: " + singleResultRatings.Average());

                bestScoreTextBlocks.ElementAt(i).Text = highestScore.ToString();

                double avgScore = 0;
                if (gameModeResultsForPlayer.Count > 0)
                {
                    avgScore = Math.Round(gameModeResultsForPlayer.Sum(a => a.Score) / gameModeResultsForPlayer.Count);
                }
                avgScoreTextBlocks.ElementAt(i).Text = avgScore.ToString();
                gamesPlayedTextBlocks.ElementAt(i).Text = gameModeResultsForPlayer.Count.ToString();
                skillLevelTextBlocks.ElementAt(i).Text = GetSkillLevelString(avgScore, i);
            }
            
            // update summary labels
            totalGamesPlayed.Text = totalGamesPlayedNr.ToString();
            
        }
        
        private string GetOverallSkillLevelString(double rating)
        {
            if (rating < 850)
            {
                return SKILL_LEVEL_BEGINNER;
            }
            else if (rating < 900)
            {
                return SKILL_LEVEL_INTERMEDIATE;
            }
            else if (rating < 935)
            {
                return SKILL_LEVEL_ADVANCED;
            }
            else if (rating < 950)
            {
                return SKILL_LEVEL_EXCEPTIONAL;
            }
            else if (rating < 1000)
            {
                return SKILL_LEVEL_PRO;
            }
            else if (rating >= 1000)
            {
                return SKILL_LEVEL_AMAZING;
            }
            else
            {
                return "n/a";
            }
        }

        // returns skill level for a score in some game mode
        private string GetSkillLevelString(double score, int gameMode)
        {
            if (gameMode == 0)
            {
                if (score < 400)
                {
                    return SKILL_LEVEL_BEGINNER;
                }
                else if (score < 600)
                {
                    return SKILL_LEVEL_INTERMEDIATE;
                }
                else if (score < 800)
                {
                    return SKILL_LEVEL_ADVANCED;
                }
                else if (score < 945)
                {
                    return SKILL_LEVEL_PRO;
                }
                else if (score >= 945)
                {
                    return SKILL_LEVEL_AMAZING;
                }
                else
                {
                    return "n/a";
                }
            }
            else if (gameMode == 1)
            {
                if (score < 20)
                {
                    return SKILL_LEVEL_BEGINNER;
                }
                else if (score < 40)
                {
                    return SKILL_LEVEL_INTERMEDIATE;
                }
                else if (score < 60)
                {
                    return SKILL_LEVEL_ADVANCED;
                }
                else if (score < 80)
                {
                    return SKILL_LEVEL_EXCEPTIONAL;
                }
                else if (score < 99)
                {
                    return SKILL_LEVEL_PRO;
                }
                else if (score >= 99)
                {
                    return SKILL_LEVEL_AMAZING;
                }
                else
                {
                    return "n/a";
                }
            }
            else if (gameMode == 2)
            {
                if (score <= 8)
                {
                    return ABO_SKILL_LEVEL_BEGINNER;
                }
                else if (score <= 15)
                {
                    return ABO_SKILL_LEVEL_INTERMEDIATE;
                }
                else if (score <= 30)
                {
                    return ABO_SKILL_LEVEL_ADVANCED;
                }
                else if (score <= 40)
                {
                    return ABO_SKILL_LEVEL_PRO;
                }
                else if (score <= 60)
                {
                    return ABO_SKILL_LEVEL_AMAZING;
                }
                else
                {
                    return "n/a";
                }
            }
            else if (gameMode == 3)
            {
                if (score <= 200)
                {
                    return SKILL_LEVEL_BEGINNER;
                }
                else if (score <= 400)
                {
                    return SKILL_LEVEL_INTERMEDIATE;
                }
                else if (score <= 600)
                {
                    return SKILL_LEVEL_ADVANCED;
                }
                else if (score <= 800)
                {
                    return SKILL_LEVEL_PRO;
                }
                else if (score <= 1000)
                {
                    return SKILL_LEVEL_AMAZING;
                }
                else
                {
                    return "n/a";
                }
            }
            else
            {
                return "n/a";
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

        private void buyProPack_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            Frame.Navigate(typeof(BuyProPackPage));
        }
    }
}
