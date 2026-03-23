using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace watchme;

public partial class WatchConnectivityViewModel : ObservableObject
{
    private readonly WatchConnectivityManager manager;

    [ObservableProperty]
    private bool isOn;

    [ObservableProperty]
    private string? lastEventTime;

    public string StatusText => IsOn ? "Power is ON" : "Power is OFF";

    public string LastEventDisplay =>
        string.IsNullOrWhiteSpace(LastEventTime)
            ? "No events yet"
            : $"Last change: {LastEventTime}";

    public WatchConnectivityViewModel()
    {
        manager = WatchConnectivityManager.Instance;

        IsOn = manager.IsOn;
        LastEventTime = manager.LastEventTime;

        manager.PowerStateChanged += OnPowerStateChanged;
    }

    private void OnPowerStateChanged(object? sender, PowerStateChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsOn = e.IsOn;
            LastEventTime = e.LastEventTime;
        });
    }

    partial void OnIsOnChanged(bool value)
    {
        OnPropertyChanged(nameof(StatusText));
    }

    partial void OnLastEventTimeChanged(string? value)
    {
        OnPropertyChanged(nameof(LastEventDisplay));
    }

    [RelayCommand]
    private void TurnOn()
    {
        manager.SendPowerState(true);
    }

    [RelayCommand]
    private void TurnOff()
    {
        manager.SendPowerState(false);
    }

    public void SetPower(bool value)
    {
        manager.SendPowerState(value);
    }


    


}