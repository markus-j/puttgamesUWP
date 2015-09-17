using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace puttgamesWP10
{
    public class PlaceholderConverter : IValueConverter
    {

        #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        int? itemCount = value as int?;
        if (itemCount.HasValue)
        {
            return itemCount > 0 ? Windows.UI.Xaml.Visibility.Collapsed : Windows.UI.Xaml.Visibility.Visible;
        }

        return Windows.UI.Xaml.Visibility.Collapsed;
    }


        // ConvertBack is not implemented for a OneWay binding. 
        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    } 
}
