using System;
using Foundation;
using ObjCRuntime;
using WatchConnectivity;

namespace watchme;

public class WatchConnectivityManager : NSObject, IWCSessionDelegate
{
    public static readonly WatchConnectivityManager Instance = new();

    public event Action<bool, string>? StateChanged;

    private WCSession? session;
    private bool _isOn;

    private readonly NSDateFormatter timeFormatter = new()
    {
        DateFormat = "HH:mm:ss",
        TimeZone = NSTimeZone.LocalTimeZone
    };

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
        _isOn = on;

        var timeStr = timeFormatter.ToString(NSDate.Now);

        var payload = new NSDictionary<NSString, NSObject>(
            new[] { new NSString("power"), new NSString("time") },
            new NSObject[] { new NSNumber(on), new NSString(timeStr) }
        );

        if (session?.Reachable == true)
        {
            try
            {
                session.SendMessage(payload, null, error =>
                {
                    Console.WriteLine($"SendMessage failed: {error?.LocalizedDescription} – falling back to context");
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
        try
        {
            session.UpdateApplicationContext(context, out var error);
            if (error != null)
                Console.WriteLine($"UpdateApplicationContext failed: {error.LocalizedDescription}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UpdateApplicationContext exception: {ex.Message}");
        }
    }

    // --- WCSessionDelegate ---

    [Export("session:activationDidCompleteWithState:error:")]
    public void ActivationDidComplete(WCSession session, WCSessionActivationState activationState, NSError? error)
    {
        Console.WriteLine($"Session activation: {activationState}, error: {error?.LocalizedDescription ?? "none"}");
        if (activationState == WCSessionActivationState.Activated)
            RequestCurrentStateIfNeeded();
    }

    [Export("sessionReachabilityDidChange:")]
    public void ReachabilityDidChange(WCSession session)
    {
        Console.WriteLine($"Reachability changed: {session.Reachable}");
        if (session.Reachable)
            RequestCurrentStateIfNeeded();
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
            SendPowerState(_isOn);
            return;
        }

        if (dict["power"] is NSNumber powerNum && dict["time"] is NSString timeStr)
        {
            var on = powerNum.BoolValue;
            var time = timeStr.ToString();
            _isOn = on;
            MainThread.BeginInvokeOnMainThread(() => StateChanged?.Invoke(on, time));
        }
    }

    private void RequestCurrentStateIfNeeded()
    {
        if (session?.Reachable != true) return;

        var msg = new NSDictionary<NSString, NSObject>(
            new NSString[] { new NSString("request") },
            new NSObject[] { new NSString("power") });

        session.SendMessage(msg, null, err =>
            Console.WriteLine($"RequestCurrentState failed: {err?.LocalizedDescription ?? "unknown"}"));
    }

    [Export("sessionDidBecomeInactive:")]
    public void SessionDidBecomeInactive(WCSession session) { }

    [Export("sessionDidDeactivate:")]
    public void SessionDidDeactivate(WCSession session) =>
        WCSession.DefaultSession.ActivateSession();
}