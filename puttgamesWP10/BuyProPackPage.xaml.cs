using puttgamesWP10.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Store;
using System.Diagnostics;
using Windows.Storage;
using System.Threading.Tasks;

// Page to show ProPack offer and button to go to the Store and buy ProPack

namespace puttgamesWP10
{
    public sealed partial class BuyProPackPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private LicenseInformation licenseInformation;
        private const string PRO_PACK = "ProPack";
        private const string STATUS_PURCHASE_SUCCESSFUL = "ProPack successfully purchased. Enjoy!";
        private const string STATUS_ALREADY_PURCHASED = "Earlier ProPack purchase restored.";
        private const string STATUS_NOT_PURCHASED = "ProPack was not purchased.";
        private const string STATUS_ERROR = "Something went wrong, ProPack was not purchased. Please try again later.";

        //debug
        //private const string XML_NOK = "in-app-purchase_nok.xml";
        //private const string XML_OK = "in-app-purchase_ok.xml";

        public BuyProPackPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            //licenseInformation = CurrentAppSimulator.LicenseInformation;
            //
            licenseInformation = CurrentApp.LicenseInformation;

        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            //DEBUG only, using debug interface for buying
            #region debug
            /*
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            string xml_filename = XML_NOK;
            if (localSettings.Values.ContainsKey("XML"))
            {
                xml_filename = (string)localSettings.Values["XML"];
            }
            
            Debug.WriteLine("xml_filename: " + xml_filename);

            if (xml_filename == XML_OK)
            {
                buyBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                purchasedLbl.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            LoadInAppPurchaseProxyFileAsync(xml_filename);
            */
            #endregion

            
            // check if propack is purchased and show the purchase button accordingly
            if (Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey("ProPackPurchased") || licenseInformation.ProductLicenses[PRO_PACK].IsActive)
            {
                buyBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                purchasedLbl.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }

            //fetch price from store and set it to priceLbl
            ListingInformation listingInfo = await CurrentApp.LoadListingInformationAsync();
            if(listingInfo.ProductListings.Count > 0)
            {
                ProductListing proPackListing = listingInfo.ProductListings.Values.ElementAt(0);
                priceLbl.Text = proPackListing.FormattedPrice;
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void Buy_Button_Click(object sender, RoutedEventArgs e)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (!licenseInformation.ProductLicenses[PRO_PACK].IsActive)
            {
                try
                {
                    // The customer doesn't own this feature, so 
                    // show the purchase dialog.
                    Debug.WriteLine("Try to purchase");
                    PurchaseResults x = await CurrentApp.RequestProductPurchaseAsync(PRO_PACK);
                    Debug.WriteLine("x.status: " + x.Status.ToString());

                    if (x.Status == ProductPurchaseStatus.Succeeded)
                    {
                        purchasedLbl.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        buyBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        
                        //debug only:
                        /*
                        LoadInAppPurchaseProxyFileAsync(XML_OK);
                        var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                        localSettings.Values["XML"] = XML_OK;
                        */
                        
                        statusLbl.Text = STATUS_PURCHASE_SUCCESSFUL;
                        statusLbl.Foreground = new SolidColorBrush(Windows.UI.Colors.LimeGreen);
                        statusLbl.Visibility = Windows.UI.Xaml.Visibility.Visible;

                        localSettings.Values["ProPackPurchased"] = true;

                    }
                    else if (x.Status == ProductPurchaseStatus.AlreadyPurchased)
                    {
                        Debug.WriteLine("Customer owns this already.");
                        statusLbl.Text = STATUS_ALREADY_PURCHASED;
                        statusLbl.Foreground = new SolidColorBrush(Windows.UI.Colors.LimeGreen);

                        purchasedLbl.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        buyBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        statusLbl.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        localSettings.Values["ProPackPurchased"] = true;
                    }
                    else if (x.Status == ProductPurchaseStatus.NotPurchased)
                    {
                        Debug.WriteLine("Not purchased.");
                        statusLbl.Text = STATUS_NOT_PURCHASED;
                        statusLbl.Foreground = new SolidColorBrush(Windows.UI.Colors.Orange);

                        purchasedLbl.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        buyBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        statusLbl.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    }
                    //Check the license state to determine if the in-app purchase was successful.
                }
                catch (Exception)
                {
                    // The in-app purchase was not completed because 
                    // an error occurred.
                    Debug.WriteLine("Exception occurred in buying.");
                    statusLbl.Text = STATUS_ERROR;
                    statusLbl.Foreground = new SolidColorBrush(Windows.UI.Colors.Red); 
                    statusLbl.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
            }
            else
            {
                // The customer already owns this feature.
                Debug.WriteLine("Customer owns this already.");
                localSettings.Values["ProPackPurchased"] = true;
                statusLbl.Text = STATUS_ALREADY_PURCHASED;
            }
        }

        /* Debug code for dummy store purchase

        private async void LoadInAppPurchaseProxyFileAsync(string filename)
        {
            //Debug.WriteLine("D1");

            StorageFile proxyFile = 
                await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///DataModel/" + filename));
            //StorageFolder proxyDataFolder = await Windows.Storage.ApplicationData.Current. (");
            //StorageFile proxyFile = await proxyDataFolder.GetFileAsync("in-app-purchase.xml");
            //var licenseChangeHandler = new LicenseChangedEventHandler(InAppPurchaseRefreshScenario);
            //CurrentAppSimulator.LicenseInformation.LicenseChanged += licenseChangeHandler;
            //Debug.WriteLine("D2");
            await CurrentAppSimulator.ReloadSimulatorAsync(proxyFile);
            Debug.WriteLine("Load done");

            /*
            // setup application upsell message 
            try
            {
                ListingInformation listing = await CurrentAppSimulator.LoadListingInformationAsync();
                var product1 = listing.ProductListings["product1"];
                var product2 = listing.ProductListings["product2"];
                Product1SellMessage.Text = "You can buy " + product1.Name + " for: " + product1.FormattedPrice + ".";
                Product2SellMessage.Text = "You can buy " + product2.Name + " for: " + product2.FormattedPrice + ".";
            }
            catch (Exception)
            {
                rootPage.NotifyUser("LoadListingInformationAsync API call failed", NotifyType.ErrorMessage);
            } /
        }*/

        /* Debug only, to unbuy

        private void Unbuy_Button_Click(object sender, RoutedEventArgs e)
        {
            LoadInAppPurchaseProxyFileAsync(XML_NOK); 
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["XML"] = XML_NOK;
            buyBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
            purchasedLbl.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            statusLbl.Text = "";
        } */
    }
}
