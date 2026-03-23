using Foundation;
using WatchConnectivity;

namespace watchme;

public sealed class PowerStateChangedEventArgs : EventArgs
{
    public bool IsOn { get; }
    public string? LastEventTime { get; }

    public PowerStateChangedEventArgs(bool isOn, string? lastEventTime)
    {
        IsOn = isOn;
        LastEventTime = lastEventTime;
    }
}

public sealed class WatchConnectivityManager : NSObject, IWCSessionDelegate
{
    public static WatchConnectivityManager Instance { get; } = new();

    private WCSession? session;

    private readonly NSDateFormatter timeFormatter = new()
    {
        DateFormat = "HH:mm:ss",
        TimeZone = NSTimeZone.LocalTimeZone
    };

    public bool IsOn { get; private set; }
    public string? LastEventTime { get; private set; }

    public event EventHandler<PowerStateChangedEventArgs>? PowerStateChanged;

    private WatchConnectivityManager()
    {
        if (!WCSession.IsSupported)
        {
            Console.WriteLine("WCSession not supported on this device.");
            return;
        }

        session = WCSession.DefaultSession;
        session.Delegate = this;
        session.ActivateSession();

        Console.WriteLine($"Paired: {session.Paired}");
        Console.WriteLine($"Watch installed: {session.WatchAppInstalled}");
        Console.WriteLine($"Reachable: {session.Reachable}");

        Console.WriteLine("WCSession activated.");
    }

    public void SendPowerState(bool on)
    {
        IsOn = on;

        var now = NSDate.Now;
        var timeStr = timeFormatter.ToString(now);

        var payload = new NSDictionary<NSString, NSObject>(
            new[]
            {
            new NSString("power"),
            new NSString("time")
            },
            new NSObject[]
            {
            new NSNumber(on),
            new NSString(timeStr)
            });

        if (session == null)
            return;

        Console.WriteLine($"Paired: {session.Paired}, WatchAppInstalled: {session.WatchAppInstalled}, Reachable: {session.Reachable}");

        if (!session.Paired || !session.WatchAppInstalled)
        {
            Console.WriteLine("Cannot send state because the watch companion app is unavailable.");
            return;
        }

        if (session.Reachable)
        {
            try
            {
                session.SendMessage(payload, null, error =>
                {
                    Console.WriteLine($"sendMessage failed: {error?.LocalizedDescription ?? "unknown"}; falling back to context");
                    UpdateApplicationContext(payload);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendMessage exception: {ex.Message}");
                UpdateApplicationContext(payload);
            }
        }
        else
        {
            UpdateApplicationContext(payload);
        }
    }

    private void UpdateApplicationContext(NSDictionary<NSString, NSObject> context)
    {
        if (session == null) return;

        if (!session.Paired)
        {
            Console.WriteLine("No Apple Watch is paired.");
            return;
        }

        if (!session.WatchAppInstalled)
        {
            Console.WriteLine("Watch app is not installed.");
            return;
        }

        try
        {
            session.UpdateApplicationContext(context, out var error);
            if (error != null)
            {
                Console.WriteLine($"updateApplicationContext failed: {error.LocalizedDescription}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UpdateApplicationContext exception: {ex.Message}");
        }
    }



    [Export("session:activationDidCompleteWithState:error:")]
    public void ActivationDidComplete(WCSession session, WCSessionActivationState activationState, NSError? error)
    {
        Console.WriteLine($"Session activation: {activationState}, error: {error?.LocalizedDescription ?? "none"}");

        if (activationState == WCSessionActivationState.Activated)
        {
            RequestCurrentStateIfNeeded();
        }
    }

    [Export("sessionReachabilityDidChange:")]
    public void ReachabilityDidChange(WCSession session)
    {
        Console.WriteLine($"Reachability changed: {session.Reachable}");

        if (session.Reachable)
        {
            RequestCurrentStateIfNeeded();
        }
    }

    [Export("session:didReceiveMessage:")]
    public void DidReceiveMessage(WCSession session, NSDictionary<NSString, NSObject> message)
    {
        Console.WriteLine("didReceiveMessage");
        HandleIncoming(message);
    }

    [Export("session:didReceiveApplicationContext:")]
    public void DidReceiveApplicationContext(WCSession session, NSDictionary<NSString, NSObject> applicationContext)
    {
        Console.WriteLine("didReceiveApplicationContext");
        HandleIncoming(applicationContext);
    }

    private void HandleIncoming(NSDictionary<NSString, NSObject> dict)
    {
        if (dict["request"] is NSString req && req == "power")
        {
            SendPowerState(IsOn);
            return;
        }

        if (dict["power"] is NSNumber powerNum && dict["time"] is NSString timeStr)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                IsOn = powerNum.BoolValue;
                LastEventTime = timeStr.ToString();

                Console.WriteLine($"Updated state -> {IsOn} at {LastEventTime}");
                RaisePowerStateChanged();
            });
        }
    }

    private void RequestCurrentStateIfNeeded()
    {
        if (session?.Reachable != true) return;

        var msg = new NSDictionary<NSString, NSObject>(
            new NSString[] { new NSString("request") },
            new NSObject[] { new NSString("power") });

        session.SendMessage(msg, null, err =>
        {
            Console.WriteLine($"Request current state failed: {err?.LocalizedDescription ?? "unknown"}");
        });
    }

    private void RaisePowerStateChanged()
    {
        PowerStateChanged?.Invoke(this, new PowerStateChangedEventArgs(IsOn, LastEventTime));
    }

    [Export("sessionDidBecomeInactive:")]
    public void SessionDidBecomeInactive(WCSession session)
    {
    }

    [Export("sessionDidDeactivate:")]
    public void SessionDidDeactivate(WCSession session)
    {
        WCSession.DefaultSession.ActivateSession();
    }








}