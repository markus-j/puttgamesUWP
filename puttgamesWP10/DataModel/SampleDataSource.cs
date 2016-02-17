using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Diagnostics;
using System.Runtime.Serialization.Json;
using System.IO;

// The data model defined by this file serves as a representative example of a strongly-typed
// model.  The property names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs. If using this model, you might improve app 
// responsiveness by initiating the data loading task in the code behind for App.xaml when the app 
// is first launched.

namespace puttgamesWP10.Data
{
    // Result data model.
    public class JYLYResult
    {
        public JYLYResult(String serie, String playerName, double score, String dateTime)
        {
            this.Serie = serie;
            this.PlayerName = playerName;
            this.Score = score;
            this.DateTime = dateTime;
        }

        public string Serie { get; private set; }
        public string PlayerName { get; private set; }
        public double Score { get; private set; }
        public string DateTime { get; private set; }

        public override string ToString()
        {
            return this.Serie;
        }
    }

    // Player data model.
    public class Player
    {
        public Player(String playerId, String playerName, int gamesPlayed, int puttRating = 0 )
        {
            this.PlayerId = playerId;
            this.PlayerName = playerName;
            this.GamesPlayed = gamesPlayed;
            this.PuttRating = puttRating;
        }

        public string PlayerId { get; private set; }
        public string PlayerName { get; private set; }
        public int GamesPlayed { get; set; }
        public int PuttRating { get; set; }

        public override string ToString()
        {
            return this.PlayerName;
        }
    }

    // Result data model.
    public class Result
    {
        public Result(String resultId, String resultPlayerName, String resultGameModeId, double score, String resultDateTime)
        {
            this.ResultId = resultId;
            this.ResultPlayerName = resultPlayerName;
            this.ResultGameModeId = resultGameModeId;
            this.Score = score;
            this.ResultDateTime = resultDateTime;
        }

        public string ResultId { get; private set; }
        public string ResultPlayerName { get; private set; }
        public string ResultGameModeId { get; private set; }
        public double Score { get; private set; }
        public string ResultDateTime { get; private set; }

        public override string ToString()
        {
            return this.Score.ToString();
        }
    }

    // Player group data model.
    public class PlayerGroup
    {
        public PlayerGroup(String playerGroupId)
        {
            this.PlayerGroupId = playerGroupId;
            this.Players = new ObservableCollection<Player>();
        }

        public string PlayerGroupId { get; private set; }
        public ObservableCollection<Player> Players { get; set; }

        public override string ToString()
        {
            return this.PlayerGroupId;
        }
    }
    
    // Result group data model.
    public class GameResultsGroup
    {
        public GameResultsGroup(String gameResultsGroupId)
        {
            this.GameResultsGroupId = gameResultsGroupId;
            this.Results = new ObservableCollection<Result>();
        }

        public string GameResultsGroupId { get; private set; }
        public ObservableCollection<Result> Results { get; set; }

        public override string ToString()
        {
            return this.GameResultsGroupId;
        }
    }


    public sealed class SampleDataSource
    {
        const string JSON_FILENAME = "data.txt";        
        const string JSON_FILEPATH_LOCAL_FOLDER = "ms-appdata:///local/";
        const string JSON_FILEPATH_INSTALLATION_FOLDER = "ms-appx:///DataModel";
        const string UPDATE_DONE = "update1.2.0.0_done";
        const string UPDATE_ABO_DONE = "update_abo_done";
        const string UPDATE_JYLY_DONE = "update_JYLY_done";

        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<JYLYResult> _JYLYresults = new ObservableCollection<JYLYResult>();
        public ObservableCollection<JYLYResult> JYLYResults
        {
            get { return this._JYLYresults; }
        }

        private ObservableCollection<GameResultsGroup> _results = new ObservableCollection<GameResultsGroup>();
        public ObservableCollection<GameResultsGroup> Results
        {
            get { return this._results; }
        }

        private ObservableCollection<PlayerGroup> _players = new ObservableCollection<PlayerGroup>();
        public ObservableCollection<PlayerGroup> Players
        {
            get { return this._players; }
        }

        public static async Task<IEnumerable<JYLYResult>> GetJYLYResultsAsync()
        {
            await _sampleDataSource.GetSampleDataAsyncC();

            return _sampleDataSource.JYLYResults;
        }

        public static async Task<IEnumerable<GameResultsGroup>> GetResultsAsync()
        {
            await _sampleDataSource.GetSampleDataAsyncC();

            return _sampleDataSource.Results;
        }
        public static async Task<IEnumerable<PlayerGroup>> GetPlayersAsync()
        {
            Debug.WriteLine("SDC1.2");
            await _sampleDataSource.GetSampleDataAsyncC();

            return _sampleDataSource.Players;
        }
       
        // gets the results for game mode gameResultDataGroupId
        public static async Task<GameResultsGroup> GetResultGroupAsync(string gameResultGroupId)
        {
            await _sampleDataSource.GetSampleDataAsyncC();

            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.Results.Where((result) => result.GameResultsGroupId.Equals(gameResultGroupId));
            if (matches.Count() >= 1) return matches.First();
            return null;
        }
        // gets the players for playerGroupId
        public static async Task<PlayerGroup> GetPlayerGroupAsync(string playerGroupId)
        {
            await _sampleDataSource.GetSampleDataAsyncC();
            
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.Players.Where((result) => result.PlayerGroupId.Equals(playerGroupId));
            if (matches.Count() >= 1) return matches.First();
            return null;
        }
        
        // gets the playergroup "0"
        public static async Task<PlayerGroup> GetPlayerGroupOne()
        {
            await _sampleDataSource.GetSampleDataAsyncC();
            return _sampleDataSource.Players.ElementAt(0);
        }

        // gets the players for playerGroupId
        public static async Task<Player> GetPlayerAsync(string playerName)
        {
            //var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            await _sampleDataSource.GetSampleDataAsyncC();
            
            // Simple linear search is acceptable for small data sets
            var playerGroup = _sampleDataSource.Players[0];
            var matches = playerGroup.Players.Where((result) => result.PlayerName.Equals(playerName));
            if (matches.Count() >= 1) return matches.First();
            return null;
        }

        private object dataLock = new object();
        private bool isDataLocked = false;
        private async Task GetSampleDataAsyncC()
        {
            bool wait = false;
            lock (dataLock)
            {
                if (!isDataLocked)
                {
                    isDataLocked = true;
                }
                else
                {
                    wait = true;
                }
            }
            if (wait)
            {
                while (isDataLocked)
                {
                    await Task.Delay(100);
                }
                return;
            }
            
            if (_results.Count != 0 || _players.Count != 0 || _JYLYresults.Count != 0)
            {
                lock (dataLock)
                {
                    isDataLocked = false;
                }
                return;
            }

            Uri localUri = new Uri(JSON_FILEPATH_LOCAL_FOLDER + JSON_FILENAME);
            Uri installationUri = new Uri(JSON_FILEPATH_INSTALLATION_FOLDER + JSON_FILENAME);
            
            StorageFile file = null;
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            
            if (!localSettings.Values.ContainsKey("FileCreated") )
            {
                Debug.WriteLine("File does not exist, return preloaded data");
                Windows.Storage.StorageFolder installedLocation = Windows.ApplicationModel.Package.Current.InstalledLocation;
                file = await installedLocation.GetFileAsync("data.txt");
            }
            else
            {
                Debug.WriteLine("File exists, return local data");
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                IReadOnlyList<StorageFile> fileList = await localFolder.GetFilesAsync();
                foreach(var Sfile in fileList)
                {
                    if (Sfile.Name.Contains("data") || Sfile.Name.Contains("Data"))
                    {
                        try
                        {
                            file = await localFolder.GetFileAsync(Sfile.Name);
                            Debug.WriteLine("File read successfully.");
                            break;
                        }
                        catch (System.IO.FileNotFoundException)
                        {
                            Debug.WriteLine("File not found exception. Return preloaded data.");
                            StorageFolder installedLocation = Windows.ApplicationModel.Package.Current.InstalledLocation;
                            file = await installedLocation.GetFileAsync("data.txt");
                        }
                    }
                }
            }

            
            string jsonText = "";
            try
            {
                if (file != null)
                {
                    jsonText = await FileIO.ReadTextAsync(file);
                }
            }
            catch (FileNotFoundException)
            {
                // Handle file not found
                Debug.WriteLine("json FileNotFound Exception");
            }
            catch (KeyNotFoundException)
            {
                Debug.WriteLine("json KeyNotFound Exception");
            }
            catch (System.Exception)
            {
                Debug.WriteLine("System.Exception");
            }

            JsonObject jsonObject;
            if (jsonText != null && jsonText != "")
            {
                jsonObject = JsonObject.Parse(jsonText);
            }
            else
            {
                Debug.WriteLine("jsonText was null, return");
                return;
            }


            JsonArray jsonArray_results = jsonObject["ResultGroups"].GetArray();
            foreach (JsonValue groupValue in jsonArray_results)
            {
                JsonObject groupObject = groupValue.GetObject();
                GameResultsGroup resultGroup = new GameResultsGroup(groupObject["GameResultsGroupId"].GetString());
                foreach (JsonValue resultValue in groupObject["Results"].GetArray())
                {
                    JsonObject resultObject = resultValue.GetObject();

                    resultGroup.Results.Add(new Result(resultObject["ResultId"].GetString(),
                                                       resultObject["ResultPlayerName"].GetString(),
                                                       resultObject["ResultGameModeId"].GetString(),
                                                       resultObject["Score"].GetNumber(),
                                                       resultObject["ResultDateTime"].GetString()));
                }
                this.Results.Add(resultGroup);
            }



            if (!localSettings.Values.ContainsKey(UPDATE_ABO_DONE) || this.Results.Count == 2 )
            {
                GameResultsGroup resultGroup = new GameResultsGroup("2");
                this.Results.Add(resultGroup);
                localSettings.Values[UPDATE_ABO_DONE] = true;
                localSettings.Values["SaveAboGroup"] = true;
            }
            if (!localSettings.Values.ContainsKey(UPDATE_JYLY_DONE) || this.Results.Count == 3)
            {
                GameResultsGroup resultGroup = new GameResultsGroup("3");
                this.Results.Add(resultGroup);
                localSettings.Values[UPDATE_JYLY_DONE] = true;
                localSettings.Values["SaveJYLYGroup"] = true;
            }


            JsonArray jsonArray_players = jsonObject["PlayerGroups"].GetArray();
            foreach (JsonValue playerGroupValue in jsonArray_players)
            {
                JsonObject playerGroupObject = playerGroupValue.GetObject();
                PlayerGroup playerGroup = new PlayerGroup(playerGroupObject["PlayerGroupId"].GetString());
                foreach (JsonValue resultValue in playerGroupObject["Players"].GetArray())
                {
                    JsonObject resultObject = resultValue.GetObject();

                    if (localSettings.Values.ContainsKey(UPDATE_DONE))
                    {
                        Debug.WriteLine("GetPlayerWithRating");
                        playerGroup.Players.Add(new Player(resultObject["PlayerId"].GetString(),
                                                           resultObject["PlayerName"].GetString(),
                                                           (int)resultObject["GamesPlayed"].GetNumber(),
                                                           (int)resultObject["PuttRating"].GetNumber()));
                    }
                    else
                    {
                        Debug.WriteLine("get players Without Rating");
                        playerGroup.Players.Add(new Player(resultObject["PlayerId"].GetString(),
                                                           resultObject["PlayerName"].GetString(),
                                                           (int)resultObject["GamesPlayed"].GetNumber()));
                    }
                }
                this.Players.Add(playerGroup);
            }
            
            // get JYLY results
            Debug.WriteLine("Get jylyresults");

            JsonArray jsonArray_JYLYresults = null;
            if (jsonObject.ContainsKey("JYLYResults"))
            {
                jsonArray_JYLYresults = jsonObject["JYLYResults"].GetArray();
                
                foreach (JsonValue JYLYResultValue in jsonArray_JYLYresults)
                {
                    JsonObject JYLYResultObject = JYLYResultValue.GetObject();
                    JYLYResult jyly_result = new JYLYResult(JYLYResultObject["Serie"].GetString(),
                                                            JYLYResultObject["PlayerName"].GetString(),
                                                            JYLYResultObject["Score"].GetNumber(),
                                                            JYLYResultObject["DateTime"].GetString());
                    this.JYLYResults.Add(jyly_result);
                }
                
            }

            Debug.WriteLine("GetData Done");
            lock (dataLock)
            {
                isDataLocked = false;
            }       
        }
    }
}