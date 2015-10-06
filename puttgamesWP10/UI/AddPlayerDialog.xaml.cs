using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers.Provider;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using puttgamesWP10.Data;


namespace puttgamesWP10
{
    public sealed partial class AddPlayerDialog : ContentDialog
    {
        public AddPlayerDialog()
        {
            this.InitializeComponent();
            this.Opened += AddPlayerDialog_Opened;
        }

        private void AddPlayerDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            // Ensure that the OK button is disabled and player name field empty each time the dialog opens.
            playerName.Text = "";
            IsPrimaryButtonEnabled = false;

            // try to put the cursor to the name field (does not necessarily work every time)
            playerName.Focus(Windows.UI.Xaml.FocusState.Programmatic);
        }
        public string GetPlayerName()
        {
            return playerName.Text;
        }

        private async void playerName_TextChanged(object sender, TextChangedEventArgs e)
        {
            // ";[]{}" are forbidden in name because of json format, clear the latest character if it is any of those
            if (playerName.Text.Contains(";") || playerName.Text.Contains("[") || playerName.Text.Contains("]") || 
                playerName.Text.Contains("{") || playerName.Text.Contains("}"))
            {
                playerName.Text = playerName.Text.Remove(playerName.Text.Length - 1);
            }

            // when player name field is cleared, disable button and do nothing else
            if (playerName.Text.Length == 0)
            {
                IsPrimaryButtonEnabled = false;
                infoText.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                return;
            }
            
            // try to get existing player from database
            var player = await SampleDataSource.GetPlayerAsync(playerName.Text); 

            // if there is existing player with the name, disable button and show info text
            if (player != null)
            {
                IsPrimaryButtonEnabled = false;
                infoText.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            // no players found from database, free to create one
            else
            {
                IsPrimaryButtonEnabled = true;
                infoText.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }
    }
}
