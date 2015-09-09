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


// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

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
            // Ensure that the check box is unchecked each time the dialog opens.
            playerName.Text = "";
            IsPrimaryButtonEnabled = false;
            playerName.Focus(Windows.UI.Xaml.FocusState.Programmatic);
        }
        public string GetPlayerName()
        {
            return playerName.Text;
        }

        private async void playerName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (playerName.Text.Contains(";"))
            {
                playerName.Text = playerName.Text.Remove(playerName.Text.Length - 1);
            }
            var player = await SampleDataSource.GetPlayerAsync(playerName.Text); 

            if (playerName.Text.Length == 0)
            {
                IsPrimaryButtonEnabled = false;
                infoText.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else if (player != null)
            {
                IsPrimaryButtonEnabled = false;
                infoText.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                IsPrimaryButtonEnabled = true;
                infoText.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }
    }
}
