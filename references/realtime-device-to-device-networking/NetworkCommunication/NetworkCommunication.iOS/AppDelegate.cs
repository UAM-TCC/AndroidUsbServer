﻿using Foundation;
using UIKit;

namespace NetworkCommunication.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        public override UIWindow Window { get; set;}

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            Window = new UIWindow (UIScreen.MainScreen.Bounds);

            var storyboard = UIStoryboard.FromName ("Main", null);

            Window.RootViewController = storyboard.InstantiateViewController ("NavigationController");

            Window.MakeKeyAndVisible ();

            return true;
        }

        public override void OnResignActivation(UIApplication application)
        {

        }

        public override void DidEnterBackground(UIApplication application)
        {

        }

        public override void WillEnterForeground(UIApplication application)
        {

        }

        public override void OnActivated(UIApplication application)
        {

        }

        public override void WillTerminate(UIApplication application)
        {

        }
    }
}


