// this class converts DateTime objects to localized string format

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using System.Globalization;

namespace puttgamesWP10
{
    public class DateToStringConverter : IValueConverter
    {

        #region IValueConverter Members

        // Define the Convert method to change a DateTime object to  
        // a month string. 
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // The value parameter is the data from the source object. 
            DateTime thedate = new DateTime();
            bool parseSuccess = DateTime.TryParse((string)value, out thedate);
            
            string date = "";
            //date = date.Substring(0, 8);
            
            if (parseSuccess)
            {
                date = thedate.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern); 
            }
            else
            {
                date = DateTime.Today.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
            }

            return date;
        }

        // ConvertBack is not implemented for a OneWay binding. 
        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            return "";
            //throw new NotImplementedException();
        }

        #endregion
    } 
}
