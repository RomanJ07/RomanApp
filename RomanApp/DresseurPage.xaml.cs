using RomanApp.ViewModels;

namespace RomanApp;

public partial class DresseurPage : ContentPage
{
    private readonly DresseurViewModel _viewModel;
    private bool _isInitialized;

    
    public DresseurPage(DresseurViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        await _viewModel.LoadSavedTeamCommand.ExecuteAsync(null);
    }
}

