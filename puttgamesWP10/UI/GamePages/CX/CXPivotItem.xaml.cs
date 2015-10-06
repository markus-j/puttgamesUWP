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
using Windows.Phone.UI.Input;

// User Control representing player's page in game mode 100*10

namespace puttgamesWP10
{
    public sealed partial class CXPivotItem : UserControl
    {
        public event EventHandler<RoutedEventArgs> PlayerCompletedGame;
        public event EventHandler<RoutedEventArgs> PlayerUncompletedGame;
        
        public bool isCompleted()
        {
            if (LblTotalNr.Text == "100")
            {
                return true;
            }
            return false;
        }

        public int getThrowsIn()
        {
            return Convert.ToInt32(LblIn.Text);
        }
        public int getThrowsOut()
        {
            return Convert.ToInt32(LblOut.Text);
        }
        public void setThrows(string throwsIn, string throwsOut)
        {
            LblIn.Text = throwsIn;
            LblOut.Text = throwsOut;
            LblTotalNr.Text = (Convert.ToInt32(throwsIn) + Convert.ToInt32(throwsOut)).ToString();
            LblLeftNr.Text = (100 - Convert.ToInt32(LblTotalNr.Text)).ToString();
            checkIfGameCompleted();
        }

        public CXPivotItem()
        {
            this.InitializeComponent();
        }
        
        private void btnIn_Click(object sender, RoutedEventArgs e)
        {
            //update labels
            LblIn.Text = (Convert.ToInt32(LblIn.Text) + 1).ToString();
            updateCommonLabels();
            checkIfGameCompleted();    
        }

        private void btnOut_Click(object sender, RoutedEventArgs e)
        {
            //update labels
            LblOut.Text = (Convert.ToInt32(LblOut.Text) + 1).ToString();
            updateCommonLabels();
            checkIfGameCompleted();
        }
        
        private void checkIfGameCompleted()
        {
            if (LblLeftNr.Text == "0")
            {
                btnIn.IsEnabled = false;
                btnOut.IsEnabled = false;

                if (PlayerCompletedGame != null)
                {
                    PlayerCompletedGame(this, new RoutedEventArgs());
                }
            }
            else
            {
                btnIn.IsEnabled = true;
                btnOut.IsEnabled = true;
                
                if (PlayerCompletedGame != null)
                {
                    PlayerUncompletedGame(this, new RoutedEventArgs());
                }
            }
        }

        private void updateCommonLabels()
        {
            LblLeftNr.Text = (Convert.ToInt32(LblLeftNr.Text) - 1).ToString();
            LblTotalNr.Text = (Convert.ToInt32(LblTotalNr.Text) + 1).ToString();
        }

        private void btnOutCorrect_Click(object sender, RoutedEventArgs e)
        {
            if(Convert.ToInt32(LblOut.Text) > 0)
            { 
                LblOut.Text = (Convert.ToInt32(LblOut.Text) - 1).ToString();
                LblTotalNr.Text = (Convert.ToInt32(LblTotalNr.Text) - 1).ToString();
                LblLeftNr.Text = (Convert.ToInt32(LblLeftNr.Text) + 1).ToString();
                checkIfGameCompleted();
            }
        }

        private void btnInCorrect_Click(object sender, RoutedEventArgs e)
        {
            if (Convert.ToInt32(LblIn.Text) > 0)
            {
                LblIn.Text = (Convert.ToInt32(LblIn.Text) - 1).ToString();
                LblTotalNr.Text = (Convert.ToInt32(LblTotalNr.Text) - 1).ToString();
                LblLeftNr.Text = (Convert.ToInt32(LblLeftNr.Text) + 1).ToString();
                checkIfGameCompleted();
            }
        }
    }
}
