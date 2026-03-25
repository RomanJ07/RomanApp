using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RomanApp.Models;
using RomanApp.Services;

namespace RomanApp.ViewModels;

public partial class AddPokemonViewModel : ObservableObject
{
    private readonly ICapturedPokemonService _capturedPokemonService;

    public AddPokemonViewModel(ICapturedPokemonService capturedPokemonService)
    {
        _capturedPokemonService = capturedPokemonService;
        CapturedPokemons = new ObservableCollection<CapturedPokemon>();
    }

    public ObservableCollection<CapturedPokemon> CapturedPokemons { get; }

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private string photoPath = string.Empty;

    [ObservableProperty]
    private byte[]? photoData;

    [ObservableProperty]
    private bool isPhotoTaken;

    [RelayCommand]
    public async Task TakePhoto()
    {
        try
        {
            // Demander les permissions de caméra
            var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (cameraStatus != PermissionStatus.Granted)
            {
                cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
            }

            if (cameraStatus != PermissionStatus.Granted)
            {
                await Application.Current!.MainPage!.DisplayAlertAsync("Erreur", "Permission d'accès à la caméra refusée", "OK");
                return;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync();

            if (photo != null)
            {
                PhotoPath = photo.FullPath;
                IsPhotoTaken = true;

                // Lire les données de la photo
                using var stream = await photo.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                PhotoData = memoryStream.ToArray();
            }
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlertAsync("Erreur", $"Impossible de prendre une photo: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task AddPokemon()
    {
        if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Description))
        {
            await Application.Current!.MainPage!.DisplayAlertAsync("Erreur", "Veuillez remplir le titre et la description", "OK");
            return;
        }

        if (!IsPhotoTaken || PhotoData == null)
        {
            await Application.Current!.MainPage!.DisplayAlertAsync("Erreur", "Veuillez prendre une photo", "OK");
            return;
        }

        var capturedPokemon = new CapturedPokemon
        {
            Title = Title,
            Description = Description,
            PhotoData = PhotoData,
            PhotoPath = PhotoPath,
            CaptureDate = DateTime.Now
        };

        CapturedPokemons.Add(capturedPokemon);

        // Sauvegarder dans le fichier
        await _capturedPokemonService.SaveCapturedPokemonAsync(capturedPokemon);

        // Réinitialiser les champs
        Title = string.Empty;
        Description = string.Empty;
        PhotoPath = string.Empty;
        PhotoData = null;
        IsPhotoTaken = false;

        await Application.Current!.MainPage!.DisplayAlertAsync("Succès", "Pokémon ajouté au Pokédex!", "OK");
    }

    [RelayCommand]
    public void ResetForm()
    {
        Title = string.Empty;
        Description = string.Empty;
        PhotoPath = string.Empty;
        PhotoData = null;
        IsPhotoTaken = false;
    }
}

