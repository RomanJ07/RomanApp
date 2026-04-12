using RomanThurianApp.ViewModels;

namespace RomanThurianApp;

public partial class TrainerPage
{
    private readonly TrainerViewModel _viewModel;
    private bool _isInitialized;

    public TrainerPage(TrainerViewModel viewModel)
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

