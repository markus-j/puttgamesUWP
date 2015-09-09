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
using puttgamesWP10.Data;
using puttgamesWP10.Common;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace puttgamesWP10
{
    public sealed partial class MainPagePivotItemPlayers : UserControl
    {
        public event EventHandler<RoutedEventArgs> BuyProPackClicked;
        public event EventHandler<RoutedEventArgs> PlayerTapped;
        public event EventHandler<RoutedEventArgs> DeletePlayerClicked;

        public MainPagePivotItemPlayers()
        {
            this.InitializeComponent();
        }
        public void showProPackOffer()
        {
            buyProPackText.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }
        public void hideProPackOffer()
        {
            buyProPackText.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }
        private void buyProPack_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            if (BuyProPackClicked != null)
            {
                BuyProPackClicked(this, new RoutedEventArgs());
            }
        }
        public void PlayersListView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            Player player = datacontext as Player;

            if (player != null)
            {
                PlayerTappedEventArgs args = new PlayerTappedEventArgs();
                args.Player = player;
                if (PlayerTapped != null)
                {
                    PlayerTapped(this,args);
                }
            }
        }

        public void PlayersListView_Holding(object sender, HoldingRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);

            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            Player player = datacontext as Player;

            if (player != null)
            {
                flyoutBase.ShowAt(e.OriginalSource as FrameworkElement);
            }
            e.Handled = true;
        }

        public void MenuFlyoutDelete_Click(object sender, RoutedEventArgs e)
        {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            Player player = datacontext as Player;

            if (player != null)
            {
                PlayerTappedEventArgs args = new PlayerTappedEventArgs();
                args.Player = player;
                if (DeletePlayerClicked != null)
                {
                    DeletePlayerClicked(this, args);
                }
            }
        }
        public void scrollIntoNewPlayer(Player player)
        {
            PlayersListView.ScrollIntoView(player);
        }
    }
}
