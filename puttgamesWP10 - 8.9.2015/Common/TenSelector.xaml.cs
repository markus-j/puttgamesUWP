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
    public sealed partial class TenSelector : UserControl
    {
        private List<Button> buttons = new List<Button>();
        private int currentlySelected = 0;
        public event EventHandler<RoutedEventArgs> SelectionChanged;
        private SolidColorBrush selectedColorBrush = (Windows.UI.Xaml.Media.SolidColorBrush)(Application.Current.Resources["SystemControlBackgroundAccentBrush"]);
        private SolidColorBrush selectedBorderBrush = (Windows.UI.Xaml.Media.SolidColorBrush)(Application.Current.Resources["SystemControlBackgroundAccentBrush"]);
        private SolidColorBrush notSelectedColorBrush = (Windows.UI.Xaml.Media.SolidColorBrush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"];
        private SolidColorBrush notSelectedBorderBrush = (Windows.UI.Xaml.Media.SolidColorBrush)Application.Current.Resources["ApplicationForegroundThemeBrush"];
        
        public TenSelector()
        {
            this.InitializeComponent();

            buttons.Add(i);
            buttons.Add(ii);
            buttons.Add(iii);
            buttons.Add(iv);
            buttons.Add(v);
            buttons.Add(vi);
            buttons.Add(vii);
            buttons.Add(iix);
            buttons.Add(ix);
            buttons.Add(x);

            setSelection(0);
        }
        public int selection()
        {
            return currentlySelected;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button s = sender as Button;
           
            foreach (Button b in buttons)
            {
                if (b != s)
                {
                    b.Background = notSelectedColorBrush;
                    //b.BorderThickness = (Thickness)(Application.Current.Resources["PhoneBorderThickness"]);
                    b.BorderBrush = notSelectedBorderBrush;
                }
                else
                {
                    if (b.Background != selectedColorBrush)
                    {
                        b.Background = selectedColorBrush;
                        //b.BorderThickness = new Thickness(5);
                        //b.BorderBrush = selectedBorderBrush;
                        currentlySelected = Convert.ToInt32(b.Content.ToString());
                    }
                    else
                    {
                        b.Background = notSelectedColorBrush;
                        //b.BorderThickness = (Thickness)(Application.Current.Resources["PhoneBorderThickness"]);
                        b.BorderBrush = notSelectedBorderBrush; 
                        currentlySelected = 0;
                    }
                }
            }
            if (SelectionChanged != null)
            {
                SelectionChanged(this, new RoutedEventArgs());
            }
        }

        public void setSelection(int selected)
        {
            if (selected == 0)
            {
                foreach (Button btn in buttons)
                {
                    btn.Background = notSelectedColorBrush;
                    btn.BorderBrush = notSelectedBorderBrush;
                }
            }
            else
            {
                Button b = buttons.ElementAt(selected - 1);
                Button_Click(b, new RoutedEventArgs());
            }
        }
    }
}
