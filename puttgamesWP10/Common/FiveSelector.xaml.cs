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
using puttgamesWP10;

// FiveSelector is used JYLY game mode to get input from 0-5

namespace puttgamesWP10
{
    public sealed partial class FiveSelector : UserControl
    {
        private List<Button> buttons = new List<Button>();
        private int currentlySelected = 0;
        public event EventHandler<RoutedEventArgs> SelectionChanged;

        private SolidColorBrush disabledColorBrush = (Windows.UI.Xaml.Media.SolidColorBrush)(Application.Current.Resources["SystemControlDisabledBaseLowBrush"]);
        private SolidColorBrush selectedColorBrush = (Windows.UI.Xaml.Media.SolidColorBrush)(Application.Current.Resources["SystemControlBackgroundAccentBrush"]);
        private SolidColorBrush selectedBorderBrush = (Windows.UI.Xaml.Media.SolidColorBrush)(Application.Current.Resources["SystemControlBackgroundAccentBrush"]);
        private SolidColorBrush notSelectedColorBrush = (Windows.UI.Xaml.Media.SolidColorBrush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"];
        private SolidColorBrush notSelectedBorderBrush = (Windows.UI.Xaml.Media.SolidColorBrush)Application.Current.Resources["ApplicationForegroundThemeBrush"];
        
        public FiveSelector()
        {
            this.InitializeComponent();
            buttons.Add(zero); 
            buttons.Add(i);
            buttons.Add(ii);
            buttons.Add(iii);
            buttons.Add(iv);
            buttons.Add(v);

            setSelection(0);
        }
        public int selection()
        {
            return currentlySelected;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button s = sender as Button;
    
            s.Background = selectedColorBrush;
            //s.BorderBrush = selectedBorderBrush;
            currentlySelected = Convert.ToInt32(s.Content.ToString());
            
            if (SelectionChanged != null)
            {
                SelectionChanged(this, new RoutedEventArgs());
            }
        }

        // five selector selected item can be set with this, 0 = nothing is selected
        public void setSelection(int selected)
        {
            if (selected == 0)
            {
                currentlySelected = 0;

                foreach (Button b in buttons)
                {
                    if (b.IsEnabled)
                    {
                        b.Background = notSelectedColorBrush;
                        b.BorderBrush = notSelectedBorderBrush;
                    }
                    else
                    {
                        b.Background = disabledColorBrush;
                        b.BorderBrush = notSelectedBorderBrush;
                    }
                }
            }
            else
            {
                Button btn = buttons.ElementAt(selected - 1);
                Button_Click(btn, new RoutedEventArgs());
            }
        }
        public void Enable()
        {
            foreach (Button b in buttons)
            {
                if (!b.IsEnabled)
                {
                    b.IsEnabled = true;
                    b.Background = notSelectedColorBrush;
                    b.BorderBrush = notSelectedBorderBrush;
                }
            }
        }
        public void Disable()
        {
            foreach (Button b in buttons)
            {
                b.IsEnabled = false;
            }
        }
    }
}