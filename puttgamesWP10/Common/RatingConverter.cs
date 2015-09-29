using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.ApplicationModel.Store;


// Rating converter hides rating partially if proPack is not purchased
namespace puttgamesWP10
{
    public class RatingConverter : IValueConverter
    {
        private LicenseInformation licenseInformation = CurrentApp.LicenseInformation;
        private const string PRO_PACK = "ProPack";
        #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        // check licence here?
        if (!(Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey("ProPackPurchased") || licenseInformation.ProductLicenses[PRO_PACK].IsActive))
        {
            string orig = value.ToString();
            int len = orig.Length;
            if (len == 3)
            {
                orig = orig.Substring(0, 1) + "XX";
            }
            else if (len > 3)
            {
                orig = orig.Substring(0, 2) + "XX";
            }
            return orig;
        }
        else
        {
            return value.ToString();
        }
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
