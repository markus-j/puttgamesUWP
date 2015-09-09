using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using puttgamesWP10.Data;
using System.Diagnostics;

namespace puttgamesWP10.Common
{
    class PlayerTappedEventArgs : RoutedEventArgs
    {
        public Player Player { get; set; }
    }
    class GameModeEventArgs : RoutedEventArgs
    {
        public string GameModeId { get; set; }
    }
}
