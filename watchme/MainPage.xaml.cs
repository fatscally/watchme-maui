namespace watchme;

public partial class MainPage : ContentPage
{
    private WatchConnectivityViewModel Vm => (WatchConnectivityViewModel)BindingContext;

    public MainPage()
    {
        InitializeComponent();
        BindingContext = new WatchConnectivityViewModel();
    }

    private void OnToggled(object sender, ToggledEventArgs e)
    {
        Vm.SetPower(e.Value);
    }
}