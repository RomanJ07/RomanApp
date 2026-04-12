using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using RomanThurianApp.Models;
using RomanThurianApp.Services;

namespace RomanThurianApp.ViewModels;

public partial class AddPokemonViewModel : ObservableObject
{
    private readonly ICapturedPokemonService _capturedPokemonService;
    private string _title = string.Empty;
    private string _description = string.Empty;
    private string _photoPath = string.Empty;
    private byte[]? _photoData;
    private bool _isPhotoTaken;

    public AddPokemonViewModel(ICapturedPokemonService capturedPokemonService)
    {
        _capturedPokemonService = capturedPokemonService;
        CapturedPokemons = new ObservableCollection<CapturedPokemon>();
    }

    public ObservableCollection<CapturedPokemon> CapturedPokemons { get; }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string PhotoPath
    {
        get => _photoPath;
        set => SetProperty(ref _photoPath, value);
    }

    public byte[]? PhotoData
    {
        get => _photoData;
        set => SetProperty(ref _photoData, value);
    }

    public bool IsPhotoTaken
    {
        get => _isPhotoTaken;
        set => SetProperty(ref _isPhotoTaken, value);
    }

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
                await ShowAlertAsync("Erreur", "Permission d'acces a la camera refusee", "OK");
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
            await ShowAlertAsync("Erreur", $"Impossible de prendre une photo: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task AddPokemon()
    {
        if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Description))
        {
            await ShowAlertAsync("Erreur", "Veuillez remplir le titre et la description", "OK");
            return;
        }

        if (!IsPhotoTaken || PhotoData == null)
        {
            await ShowAlertAsync("Erreur", "Veuillez prendre une photo", "OK");
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

        await ShowAlertAsync("Succes", "Pokemon ajoute au Pokedex!", "OK");
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

    private static Task ShowAlertAsync(string title, string message, string cancel)
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is null)
        {
            return Task.CompletedTask;
        }

        return MainThread.InvokeOnMainThreadAsync(() => page.DisplayAlertAsync(title, message, cancel));
    }
}

