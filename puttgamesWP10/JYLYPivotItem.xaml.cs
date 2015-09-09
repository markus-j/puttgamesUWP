using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using System.Threading.Tasks;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace puttgamesWP10
{
    public sealed partial class JYLYPivotItem : UserControl
    {
        private FiveSelector fiveSelector;
        private int score = 0;
        private string state = "";
        private List<int> distances = new List<int>();
        private int currentDistance = 10;
        private const int SELECTION_SLEEP_TIME_MS = 500;

        private List<TextBlock> resultTextBlocks = new List<TextBlock>();

        public event EventHandler<RoutedEventArgs> PlayerCompletedGame;
        public event EventHandler<RoutedEventArgs> PlayerUncompletedGame;

        private SolidColorBrush selectedColorBrush = (Windows.UI.Xaml.Media.SolidColorBrush)(Application.Current.Resources["SystemControlBackgroundAccentBrush"]);
        //private SolidColorBrush selectedColorBrush = new SolidColorBrush(Windows.UI.Colors.LimeGreen);
        private SolidColorBrush notSelectedBrush = (Windows.UI.Xaml.Media.SolidColorBrush)Application.Current.Resources["ApplicationForegroundThemeBrush"];

        public JYLYPivotItem()
        {
            this.InitializeComponent();
            
            distances.Add(5);
            distances.Add(6);
            distances.Add(7);
            distances.Add(8);
            distances.Add(9);
            distances.Add(10);

            resultTextBlocks.Add(i);
            resultTextBlocks.Add(ii);
            resultTextBlocks.Add(iii);
            resultTextBlocks.Add(iv);
            resultTextBlocks.Add(v);
            resultTextBlocks.Add(vi);
            resultTextBlocks.Add(vii);
            resultTextBlocks.Add(viii);
            resultTextBlocks.Add(ix);
            resultTextBlocks.Add(x);
            resultTextBlocks.Add(xi);
            resultTextBlocks.Add(xii);
            resultTextBlocks.Add(xiii);
            resultTextBlocks.Add(xiv);
            resultTextBlocks.Add(xv);
            resultTextBlocks.Add(xvi);
            resultTextBlocks.Add(xvii);
            resultTextBlocks.Add(xviii);
            resultTextBlocks.Add(xix);
            resultTextBlocks.Add(xx);

            fiveSelector = new FiveSelector();
            fiveSelector.SetValue(Grid.RowProperty, 4);
            fiveSelector.SelectionChanged += selector_SelectionChanged;
            root.Children.Add(fiveSelector);

        }
        private async void selector_SelectionChanged(object sender, RoutedEventArgs e)
        {
            FiveSelector s = sender as FiveSelector;
            if (s != null)
            {
                int successPutts = s.selection();
                state += successPutts.ToString();
                resultTextBlocks.ElementAt(Convert.ToInt32(round.Text) - 1).Text = successPutts.ToString();
                resultTextBlocks.ElementAt(Convert.ToInt32(round.Text) - 1).Foreground = selectedColorBrush;
                Debug.WriteLine("state: " + state);

                score += (currentDistance * successPutts);
                currentDistance = distances.ElementAt(successPutts);

                scoreLbl.Text = score.ToString();
                lie.Text = currentDistance.ToString();
                
                if (round.Text != "20")
                {
                    round.Text = (Convert.ToInt32(round.Text) + 1).ToString();
                }
            }

            checkIfGameCompleted();

            // wait a moment to visualize the selection, then clear it
            await System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(SELECTION_SLEEP_TIME_MS));

            s.setSelection(0);
            foreach (TextBlock t in resultTextBlocks)
            {
                t.Foreground = notSelectedBrush;
            }
            //resultTextBlocks.ElementAt(Convert.ToInt32(state.Length - 1)).Foreground = notSelectedBrush;
        }
        public int getScore()
        {
            return score;
        }
        public string getState()
        {
            return state;
        }
        public void setState(string stateString)
        {
            state = stateString;
            if (state.Length != 20)
            {
                round.Text = (state.Length + 1).ToString();
            }
            else
            {
                round.Text = "20";
            }

            // the last number in the state string defines the current lie (0->5 .. 5->10)
            if (state.Length > 0)
            {
                lie.Text = distances.ElementAt(Convert.ToInt32(state.Substring(state.Length - 1))).ToString();
            }
            else
            {
                lie.Text = "10";
            }

            //count current score
            int previous = 5;
            int sco = 0;
            int index = 0;

            while (stateString.Length > 0)
            {
                int current = Convert.ToInt32(stateString.Substring(0, 1));
                sco += current * distances.ElementAt(previous);
                resultTextBlocks.ElementAt(index).Text = current.ToString();
                index++;

                stateString = stateString.Remove(0, 1);
                previous = current;
                Debug.WriteLine("current: " + current + " previous: " + previous + " stateString: " + stateString);
            }
            score = sco;
            scoreLbl.Text = sco.ToString();

            checkIfGameCompleted();
        }

        public bool isCompleted()
        {
            if (state.Length == 20)
            {
                return true;
            }
            return false;
        }

        private void checkIfGameCompleted()
        {
            if (state.Length == 20)
            {
                fiveSelector.Disable();
                roundOngoing.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                roundFinished.Visibility = Windows.UI.Xaml.Visibility.Visible;
                
                //set Throw from text invisible, but not collapsed
                throwFromTxt.Text = "";
                lie.Text = "";
                throwFromUnit.Text = "";

                if (PlayerCompletedGame != null)
                {
                    PlayerCompletedGame(this, new RoutedEventArgs());
                }
            }
            else
            {
                fiveSelector.Enable();
                roundOngoing.Visibility = Windows.UI.Xaml.Visibility.Visible;
                roundFinished.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                //set Throw from text invisible, but not collapsed
                throwFromTxt.Text = "Throw from:";
                throwFromUnit.Text = "m";

                if (PlayerCompletedGame != null)
                {
                    PlayerUncompletedGame(this, new RoutedEventArgs());
                }
            }

            // undo is enabled if round is not the first one
            if (state.Length != 0)
            {
                undo.IsEnabled = true;
            }
            else
            {
                undo.IsEnabled = false;
            }
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            int previousDistance = 10;
            if(state.Length >= 2)
            {
                previousDistance = distances.ElementAt( Convert.ToInt32(state.Substring(state.Length - 2, 1)) );
            }
            Debug.WriteLine("previousDistance: " + previousDistance);

            score = score - (previousDistance * Convert.ToInt32(state.Substring(state.Length - 1, 1)));
            
            Debug.WriteLine("Convert.ToInt32(state.Substring(state.Length - 1, 1)): " + Convert.ToInt32(state.Substring(state.Length - 1, 1)));

            currentDistance = previousDistance;

            scoreLbl.Text = score.ToString();
            if (round.Text != "1" && state.Length != 20)
            {
                round.Text = (Convert.ToInt32(round.Text) - 1).ToString();
            }

            lie.Text = currentDistance.ToString();

            state = state.Substring(0, state.Length - 1); 
            resultTextBlocks.ElementAt(state.Length).Text = "";

            Debug.WriteLine("newState: " + state);

            checkIfGameCompleted();
        }
    }
}
