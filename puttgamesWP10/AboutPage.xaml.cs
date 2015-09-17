using puttgamesWP10.Common;
using puttgamesWP10;
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


namespace puttgamesWP10
{
    public sealed partial class AboutPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private const string DEVELOPER_EMAIL_ADDRESS = "discapps.wp@gmail.com";
        private string message_body = "";
        private const string MESSAGE_SUBJECT = "Feedback on Putt Games";

        public AboutPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            
            string appVersion = string.Format("{0}.{1}.{2}.{3}",
                                        Windows.ApplicationModel.Package.Current.Id.Version.Major,
                                        Windows.ApplicationModel.Package.Current.Id.Version.Minor,
                                        Windows.ApplicationModel.Package.Current.Id.Version.Build,
                                        Windows.ApplicationModel.Package.Current.Id.Version.Revision);
            
            versionLbl.Text = "Version: " + appVersion;
            message_body = "\n\nFeedback on Putt Games version " + appVersion;
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
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
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

        // TODO reviewapp -link does not work in win 10 store
        private async void SendLove_Button_Click(object sender, RoutedEventArgs e)
        {
            Uri linkUri = Windows.ApplicationModel.Store.CurrentApp.LinkUri;
            //Guid appId = Windows.ApplicationModel.Store.CurrentApp.AppId;
            //await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + appId));
            await Windows.System.Launcher.LaunchUriAsync(linkUri);
        }

        private void SendFeedback_Button_Click(object sender, RoutedEventArgs e)
        {

            //send mail to the developer
            ComposeEmail(DEVELOPER_EMAIL_ADDRESS, message_body, MESSAGE_SUBJECT);
        }

        // create an email message from email address, message body and subject
        // then call async emailManager to show the new email UI
        private async void ComposeEmail(string emailAddress, string messageBody, string messageSubject)
        {
            var emailMessage = new Windows.ApplicationModel.Email.EmailMessage();
            emailMessage.Body = messageBody;
            emailMessage.Subject = messageSubject;

            var emailRecipient = new Windows.ApplicationModel.Email.EmailRecipient(emailAddress);
            emailMessage.To.Add(emailRecipient);

            await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(emailMessage);
        }

        private void buyProPack_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            Frame.Navigate(typeof(BuyProPackPage));
        }
    }
}
