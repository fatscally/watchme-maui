using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace watchme;

public partial class WatchConnectivityViewModel : ObservableObject
{
    private readonly WatchConnectivityManager _manager;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    private bool isOn;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LastEventDisplay))]
    private string? lastEventTime;

    public string StatusText => IsOn ? "On" : "Off";
    public string LastEventDisplay => LastEventTime is null ? "" : $"Last changed at {LastEventTime}";

    public WatchConnectivityViewModel()
    {
        _manager = WatchConnectivityManager.Instance;

        // Receive state pushed from the watch
        _manager.StateChanged += (on, time) =>
        {
            IsOn = on;
            LastEventTime = time;
        };
    }

    public void SetPower(bool on)
    {
        IsOn = on;
        _manager.SendPowerState(on);
    }

    [RelayCommand]
    private void TurnOn() => SetPower(true);

    [RelayCommand]
    private void TurnOff() => SetPower(false);
}