using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using puttgamesWP10.Data;
using System.Diagnostics;
using Windows.Data.Json;
using Windows.Storage;


namespace puttgamesWP10
{
    class DebugStream
    {
        public void w(string s)
        {
            Debug.WriteLine(s);
        }
    }

    class DataSaver
    {
        private RatingCalculator ratingCalculator = new RatingCalculator();
        private const string JSON_FILENAME = "data.txt";
        private const int RETRY_COUNT = 20;
        private DebugStream d = new DebugStream();

        public async void SaveAllDataToJson()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            // JSON root object where all groups of groups are added: (PlayerGroups, ResultGroups)
            JsonObject root = new JsonObject();

            // PLAYERS
            //
            PlayerGroup PlGroup = null;
            int counter = 0;
            while (PlGroup == null && counter < RETRY_COUNT)
            {
                PlGroup = await SampleDataSource.GetPlayerGroupOne();
                counter++;
            }
            d.w("counter1: " + counter);

            JsonArray groupsArr = new JsonArray();
            JsonObject groupsObj = new JsonObject();

            JsonArray PlGroupArr = new JsonArray();

            for (int j = 0; j < PlGroup.Players.Count; j++)
            {
                Player player = PlGroup.Players[j];
                JsonObject playerObj = new JsonObject();
                playerObj.Add("PlayerId", JsonValue.CreateStringValue(player.PlayerId));
                playerObj.Add("PlayerName", JsonValue.CreateStringValue(player.PlayerName));
                playerObj.Add("GamesPlayed", JsonValue.CreateNumberValue(player.GamesPlayed));

                // if rating not yet in the database, calculate it 
                if (!localSettings.Values.ContainsKey("update1.2.0.0_done"))
                {
                    int puttRating = await ratingCalculator.PlayerRating(player.PlayerName);
                    playerObj.Add("PuttRating", JsonValue.CreateNumberValue(puttRating));
                    player.PuttRating = puttRating;
                }
                else
                {
                    //add putt rating to the player as a part of the update 1.2.0.0
                    playerObj.Add("PuttRating", JsonValue.CreateNumberValue((double)player.PuttRating));
                }
                PlGroupArr.Add(playerObj);
            }

            JsonObject setOfPlayers = new JsonObject();
            setOfPlayers.Add("PlayerGroupId", JsonValue.CreateStringValue("0"));
            setOfPlayers.Add("Players", PlGroupArr);

            groupsArr.Add(setOfPlayers);
            
            root.Add("PlayerGroups", groupsArr);

            if (!localSettings.Values.ContainsKey("update1.2.0.0_done"))
            {
                // mark to the settings that the update is done
                localSettings.Values.Add("update1.2.0.0_done", true);
            }

            // RESULTS
            //
            IEnumerable<GameResultsGroup> resultGroups = null;
            counter = 0;
            while (resultGroups == null && counter < RETRY_COUNT)
            {
                resultGroups = await SampleDataSource.GetResultsAsync();
                counter++;
            }
            d.w("counter2: " + counter);
            d.w("resultGroups.count: " + resultGroups.Count<GameResultsGroup>() );

            JsonArray RgroupsArr = new JsonArray();
            JsonObject RgroupsObj = new JsonObject();

            for (int i = 0; i < resultGroups.Count<GameResultsGroup>(); i++)
            {
                // prevent more than three result groups to be saved (0,1,2,3)
                if (i >= 4)
                {
                    break;
                }
                JsonArray groupArr = new JsonArray();
                GameResultsGroup group = resultGroups.ElementAt(i);

                for (int j = 0; j < group.Results.Count; j++)
                {
                    Result result = group.Results[j];
                    JsonObject resultObj = new JsonObject();
                    resultObj.Add("ResultId", JsonValue.CreateStringValue(result.ResultId));
                    resultObj.Add("ResultPlayerName", JsonValue.CreateStringValue(result.ResultPlayerName));
                    resultObj.Add("ResultGameModeId", JsonValue.CreateStringValue(result.ResultGameModeId));
                    resultObj.Add("Score", JsonValue.CreateNumberValue(result.Score));
                    resultObj.Add("ResultDateTime", JsonValue.CreateStringValue(result.ResultDateTime));
                    groupArr.Add(resultObj);
                }

                JsonObject setOfResults = new JsonObject();
                setOfResults.Add("GameResultsGroupId", JsonValue.CreateStringValue(i.ToString()));
                setOfResults.Add("Results", groupArr);

                RgroupsArr.Add(setOfResults);
                

            }
            root.Add("ResultGroups", RgroupsArr);

            Debug.WriteLine(root.Stringify());

            // JYLY RESULTS
            //
            IEnumerable<JYLYResult> JYLYResults = null;
            counter = 0;
            while (JYLYResults == null && counter < RETRY_COUNT)
            {
                JYLYResults = await SampleDataSource.GetJYLYResultsAsync();
                counter++;
            }
            d.w("counter2: " + counter);
            d.w("resultGroups.count: " + JYLYResults.Count<JYLYResult>());

            JsonArray JYLYarray = new JsonArray();
            JsonObject JYLYObj = new JsonObject();

            for (int i = 0; i < JYLYResults.Count<JYLYResult>(); i++)
            {
                JYLYResult result = JYLYResults.ElementAt(i);
                JsonObject resultObj = new JsonObject();
                resultObj.Add("Serie", JsonValue.CreateStringValue(result.Serie));
                resultObj.Add("PlayerName", JsonValue.CreateStringValue(result.PlayerName));
                resultObj.Add("Score", JsonValue.CreateNumberValue(result.Score));
                resultObj.Add("DateTime", JsonValue.CreateStringValue(result.DateTime));
                JYLYarray.Add(resultObj);
            }
            root.Add("JYLYResults", JYLYarray);

            Debug.WriteLine(root.Stringify());

            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFile newFile = await folder.CreateFileAsync(JSON_FILENAME, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(newFile, root.Stringify());
        }
    }
}
