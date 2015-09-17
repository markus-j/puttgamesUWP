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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace puttgamesWP10
{
    public sealed partial class AboPivotItem : UserControl
    {
        private int firstScore = 0;
        private int secondScore = 0;
        private int thirdScore = 0;
        private TenSelector firstSelector;
        private TenSelector secondSelector;
        private TenSelector thirdSelector;

        public AboPivotItem()
        {
            this.InitializeComponent();

            firstSelector = new TenSelector();
            firstSelector.SetValue(Grid.RowProperty, 0);
            firstSelector.SetValue(Grid.ColumnProperty, 1);
            firstSelector.SelectionChanged += selector_SelectionChanged;
            root.Children.Add(firstSelector);

            secondSelector = new TenSelector();
            secondSelector.SetValue(Grid.RowProperty, 1);
            secondSelector.SetValue(Grid.ColumnProperty, 1);
            secondSelector.SelectionChanged += selector_SelectionChanged;
            root.Children.Add(secondSelector);

            thirdSelector = new TenSelector();
            thirdSelector.SetValue(Grid.RowProperty, 2);
            thirdSelector.SetValue(Grid.ColumnProperty, 1);
            thirdSelector.SelectionChanged += selector_SelectionChanged; 
            root.Children.Add(thirdSelector);
        }

        // handle user clicks to the TenSelectors, add points accordingly
        private void selector_SelectionChanged(object sender, RoutedEventArgs e)
        {
            TenSelector s = sender as TenSelector;
            if (s != null)
            {
                if (s == firstSelector)
                {
                    firstScore = firstSelector.selection();
                }
                else if (s == secondSelector)
                {
                    secondScore = secondSelector.selection() * 2;
                }
                else if (s == thirdSelector)
                {
                    thirdScore = thirdSelector.selection() * 3;
                }
                score.Text = (firstScore + secondScore + thirdScore).ToString();
            }
        }
        // get total score
        public int getScore()
        {
            return (firstScore + secondScore + thirdScore);
        }

        // get state as string e.g "0;0;0"
        public string getState()
        {
            string state =  (firstSelector.selection()).ToString() + ";" + 
                            (secondSelector.selection()).ToString() + ";" +
                            (thirdSelector.selection()).ToString();
            
            return state;
        }

        // set state from a string formatted like "0;0;0"
        public void setState(string state)
        {
            string score1 = state.Substring(0, (state.IndexOf(";")));
            state = state.Substring(state.IndexOf(";") + 1);
            
            string score2 = state.Substring(0, (state.IndexOf(";")));
            state = state.Substring(state.IndexOf(";") + 1);
            
            string score3 = state;
            Debug.WriteLine("score1: " + score1 + " score2: " + score2 + " score3: " + score3);

            firstSelector.setSelection(Convert.ToInt32(score1));
            secondSelector.setSelection(Convert.ToInt32(score2));
            thirdSelector.setSelection(Convert.ToInt32(score3));
        }
    }
}
