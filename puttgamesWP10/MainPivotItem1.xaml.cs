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
using puttgamesWP10.Common;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace puttgamesWP10
{
    public sealed partial class MainPivotItem1 : UserControl
    {
        public event EventHandler<RoutedEventArgs> TopResultsListTapped;

        public MainPivotItem1()
        {
            this.InitializeComponent();
        }

        private void TopResultsListView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (TopResultsListTapped != null)
            {
                GameModeEventArgs args = new GameModeEventArgs();
                args.GameModeId = "1";
                TopResultsListTapped(this, args);
            }
        }
        public void ScrollViewUp()
        {
            scrollViewerTwo.ChangeView(null, 0, null);
        }
    }
}
