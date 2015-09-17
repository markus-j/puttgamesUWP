using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using puttgamesWP10.Data;
using System.Diagnostics;

namespace puttgamesWP10
{
    class RatingCalculator
    {
        //private GameResultsGroup MXXVresults;
        //private GameResultsGroup CXresults;
        private string lastPlayerName = "";
        private int totalGamesPlayed = 0;
        private List<double> FACTORS_TWO = new List<double>();
        private List<double> FACTORS_THREE = new List<double>();
        private List<double> FACTORS_FOUR = new List<double>();
        private const int RETRY_COUNT = 20;

        public RatingCalculator()
        {
            FACTORS_TWO.Add(0.55);
            FACTORS_TWO.Add(0.45);

            FACTORS_THREE.Add(0.5);
            FACTORS_THREE.Add(0.3);
            FACTORS_THREE.Add(0.2);

            FACTORS_FOUR.Add(0.4);
            FACTORS_FOUR.Add(0.3);
            FACTORS_FOUR.Add(0.2);
            FACTORS_FOUR.Add(0.1);
        }

        public async Task<int> PlayerRating(string playerName)
        {
            // Get results for all game modes
            IEnumerable<GameResultsGroup> resultGroups = null;
            int c = 0;
            while (resultGroups == null && c < RETRY_COUNT)
            {
                resultGroups = await SampleDataSource.GetResultsAsync();
                c++;
            }

            Debug.WriteLine("c:" + c);
            Debug.WriteLine("resultgroups.count:" + resultGroups.Count<GameResultsGroup>());

            List<GameResultsGroup> allResultGroups = new List<GameResultsGroup>();
            allResultGroups.Add(resultGroups.ElementAt(0));
            allResultGroups.Add(resultGroups.ElementAt(1));
            if (resultGroups.Count<GameResultsGroup>() >= 3)
            {
                allResultGroups.Add(resultGroups.ElementAt(2));
                Debug.WriteLine("Rating: ABO mukaan");
            }
            if (resultGroups.Count<GameResultsGroup>() >= 4)
            {
                allResultGroups.Add(resultGroups.ElementAt(3));
                Debug.WriteLine("Rating: JYLY messiin");
            }
            totalGamesPlayed = 0;

            List<Result> allResultsForPlayer = new List<Result>();
            // go through the result groups
            for (int k = 0; k < allResultGroups.Count; ++k)
            {
                // go through the results in one result group
                foreach (Result result in allResultGroups.ElementAt(k).Results)
                {
                    if (result.ResultPlayerName == playerName)
                    {
                        allResultsForPlayer.Add(result);
                        Debug.WriteLine("result for player was found");
                        //resultRatings.Add(CalculateResultRating(result.Score, k));
                        ++totalGamesPlayed;
                    }
                }
            }
            for(int i = 0; i < allResultsForPlayer.Count; ++i)
            {
                Debug.WriteLine("i: " + i + " " + allResultsForPlayer.ElementAt(i).ResultDateTime);
            }
            try
            {
                allResultsForPlayer.Sort((x, y) => DateTime.Parse(x.ResultDateTime).CompareTo(DateTime.Parse(y.ResultDateTime)));
            }
            catch (System.ArgumentNullException)
            {
                Debug.WriteLine("RatingCalculator: ArgumentNullException");
                return 0;
            }
            catch (System.FormatException)
            {
                Debug.WriteLine("RatingCalculator: FormatException");
                return 0;
            }

            for (int i = 0; i < allResultsForPlayer.Count; ++i)
            {
                Debug.WriteLine("i: " + i + " " + allResultsForPlayer.ElementAt(i).ResultDateTime);
            }

            lastPlayerName = playerName;
            int counter = 1;

            List<double> resultRatingsOne = new List<double>();
            List<double> resultRatingsTwo = new List<double>();
            List<double> resultRatingsThree = new List<double>();
            List<double> resultRatingsFour = new List<double>();

            if(allResultsForPlayer.Count > 0)
            {
                for (int i = allResultsForPlayer.Count - 1; i >= 0; --i)
                {
                    double resultRating = CalculateResultRating(allResultsForPlayer.ElementAt(i).Score, 
                                             Convert.ToInt32(allResultsForPlayer.ElementAt(i).ResultGameModeId));
                    if (counter <= 3)
                    {
                        resultRatingsOne.Add(resultRating);
                    }
                    else if (counter > 3 && counter <= 7)
                    {
                        resultRatingsTwo.Add(resultRating);
                    }
                    else if (counter > 7 && counter <= 18)
                    {
                        resultRatingsThree.Add(resultRating);
                    }
                    else
                    {
                        resultRatingsFour.Add(resultRating);
                    }
                    counter++;
                }

                double averageOne = 0;
                double averageTwo = 0;
                double averageThree = 0;
                double averageFour = 0;
                bool useOne = false;
                bool useTwo = false;
                bool useThree = false;
                bool useFour = false;

                if (resultRatingsOne.Count > 0)
                {
                    useOne = true;
                    averageOne = resultRatingsOne.Average();
                }
                if (resultRatingsTwo.Count > 0)
                {
                    useTwo = true; 
                    averageTwo = resultRatingsTwo.Average();
                }
                if (resultRatingsThree.Count > 0)
                {
                    useThree = true;
                    averageThree = resultRatingsThree.Average();
                }
                if (resultRatingsFour.Count > 0)
                {
                    useFour = true;
                    averageFour = resultRatingsFour.Average();
                }

                double puttRating = 0;

                if (useFour)
                {
                    puttRating = averageOne * FACTORS_FOUR[0] + averageTwo * FACTORS_FOUR[1] + averageThree * FACTORS_FOUR[2] + averageFour * FACTORS_FOUR[3];
                }
                else if (useThree)
                {
                    puttRating = averageOne * FACTORS_THREE[0] + averageTwo * FACTORS_THREE[1] + averageThree * FACTORS_THREE[2];
                }
                else if (useTwo)
                {
                    puttRating = averageOne * FACTORS_TWO[0] + averageTwo * FACTORS_TWO[1];
                }
                else if (useOne)
                {
                    puttRating = averageOne;
                }
                
                return (int)Math.Round(puttRating);
            }
            else
            {
                Debug.WriteLine("no results found for player:" + playerName);
                return 0;
            }
        }

        public async Task<int> BasicPlayerRating(string playerName)
        {
            // Get results for all game modes

            IEnumerable<GameResultsGroup> resultGroups = null;
            int c = 0;
            while (resultGroups == null && c < RETRY_COUNT)
            {
                resultGroups = await SampleDataSource.GetResultsAsync();
                c++;
            }
           
            List<GameResultsGroup> allResultGroups = new List<GameResultsGroup>();
            allResultGroups.Add(resultGroups.ElementAt(0));
            allResultGroups.Add(resultGroups.ElementAt(1));

            List<double> resultRatings = new List<double>();
            totalGamesPlayed = 0;

            // go through the result groups
            for (int k = 0; k < allResultGroups.Count; ++k)
            {
                // go through the results in one result group
                foreach (Result result in allResultGroups.ElementAt(k).Results)
                {
                    if (result.ResultPlayerName == playerName)
                    {
                        Debug.WriteLine("result for player was found"); 
                        resultRatings.Add(CalculateResultRating(result.Score, k));
                        ++totalGamesPlayed;
                    }
                }
            }
            double puttRating = 0;

            if (resultRatings.Count > 0)
            {
                puttRating = resultRatings.Average();
            }
            
            // store playerName to whom rating and games played have been calculated
            lastPlayerName = playerName;
            return (int)Math.Round(puttRating);
        }

        public async Task<int> GamesPlayed(string playerName, bool useCache = false)
        {
            if (useCache && playerName == lastPlayerName)
            {
                return totalGamesPlayed;
            }
            else
            {
                // Get results for all game modes
                IEnumerable<GameResultsGroup> resultGroups = null;
                int c = 0;
                while (resultGroups == null && c < RETRY_COUNT)
                {
                    resultGroups = await SampleDataSource.GetResultsAsync();
                    c++;
                } 
                
                List<GameResultsGroup> allResultGroups = new List<GameResultsGroup>();
                allResultGroups.Add(resultGroups.ElementAt(0));
                allResultGroups.Add(resultGroups.ElementAt(1));
                
                if (resultGroups.Count<GameResultsGroup>() == 3)
                {
                    allResultGroups.Add(resultGroups.ElementAt(2));
                    Debug.WriteLine("Elementat(2) lisättiin");
                }

                totalGamesPlayed = 0;

                // go through the result groups
                for (int k = 0; k < allResultGroups.Count; ++k)
                {
                    // go through the results in one result group
                    foreach (Result result in allResultGroups.ElementAt(k).Results)
                    {
                        if (result.ResultPlayerName == playerName)
                        {
                            ++totalGamesPlayed;
                        }
                    }
                }
            }
            lastPlayerName = playerName;
            return totalGamesPlayed;
        }

        public double CalculateResultRating(double score, int gameMode)
        {
            Debug.WriteLine("CalculateResultRating, score: " + score + " gameMode: " + gameMode);
            if (score == 0)
            {
                return 0;
            }
            if (gameMode == 0)
            {
                if (score < 100)
                {
                    return score / 100 * 700;
                }
                return score * 4 / 10 + 700;
            }
            else if (gameMode == 1)
            {
                if (score < 10)
                {
                    return score / 10 * 700;
                }
                return score * 4 + 710;
            }
            else if (gameMode == 2)
            {
                Debug.WriteLine("ABO rating calculation");
                if (score <= 8)
                {
                    return score * 100;
                }
                else if (score <= 15)
                {
                    return 800 + (score - 8) * 14;
                }
                else if (score <= 30)
                {
                    return 900 + (score - 15) * 7;
                }
                else if (score <= 40)
                {
                    return 1005 + (score - 30) * 10;
                }
                else
                {
                    return 1105 + (score - 40) * 10; 
                }
            }
            else if (gameMode == 3)
            {
                Debug.WriteLine("JYLY rating calculation");

                if(score <= 200)
                {
                    return score * 4;
                }
                else if(score <= 400)
                {
                    return 800 + (score - 200) * 0.5;
                }
                else if (score <= 600)
                {
                    return 900 + (score - 400) * 0.4;
                }
                else
                {
                    return 980 + (score - 600) * 0.6;
                }
            }
            return 0;
        }
    }
}
