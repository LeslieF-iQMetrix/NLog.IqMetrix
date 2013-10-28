using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace NLog.iOS.Test
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow _window;

		//
		// This method is invoked when the application has loaded and is ready to run. In this 
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// create a new window instance based on the screen size
			_window = new UIWindow (UIScreen.MainScreen.Bounds);

			// If you have defined a view, add it here:
			// window.RootViewController  = navigationController;

            var viewController = new MyViewController();
            _window.RootViewController = viewController;

		    if (options != null)
		    {
		        if (options.ContainsKey(UIApplication.LaunchOptionsRemoteNotificationKey))
		        {

		            var remoteNotification = options[UIApplication.LaunchOptionsRemoteNotificationKey] as NSDictionary;
		            if (remoteNotification != null)
		            {
		                //new UIAlertView(remoteNotification.AlertAction, remoteNotification.AlertBody, null, "OK", null).Show();
		            }
		        }
		    }

		    //==== register for remote notifications and get the device token
            // set what kind of notification types we want
            const UIRemoteNotificationType notificationTypes = UIRemoteNotificationType.Alert | UIRemoteNotificationType.Badge;
            // register for remote notifications
            UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(notificationTypes);

            // make the window visible
			_window.MakeKeyAndVisible ();

			return true;
		}


        public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
        {

        }

        /// <summary>
        /// The iOS will call the APNS in the background and issue a device token to the device. when that's
        /// accomplished, this method will be called.
        ///
        /// Note: the device token can change, so this needs to register with your server application everytime
        /// this method is invoked, or at a minimum, cache the last token and check for a change.
        /// </summary>
        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            deviceToken = deviceToken.ToString();
        }

        /// <summary>
        /// Registering for push notifications can fail, for instance, if the device doesn't have network access.
        ///
        /// In this case, this method will be called.
        /// </summary>
        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            new UIAlertView("Error registering push notifications", error.LocalizedDescription, null, "OK", null).Show();
        }
	}
}