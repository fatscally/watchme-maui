using Foundation;
using UIKit;

namespace watchme;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    //public override void OnActivated(UIApplication application)
    //{
    //    base.OnActivated(application);
    //    Main run loop is running here — safe to init WCSession

    //   WatchConnectivityManager.Instance.Initialize();
    //}

}




