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
    private bool _isTakingPhoto;
    private string _photoStatusMessage = "Aucune photo prise";

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

    public bool IsTakingPhoto
    {
        get => _isTakingPhoto;
        set => SetProperty(ref _isTakingPhoto, value);
    }

    public string PhotoStatusMessage
    {
        get => _photoStatusMessage;
        set => SetProperty(ref _photoStatusMessage, value);
    }

    [RelayCommand]
    public async Task TakePhoto()
    {
        if (IsTakingPhoto)
        {
            return;
        }

        try
        {
            IsTakingPhoto = true;

            if (!MediaPicker.Default.IsCaptureSupported)
            {
                PhotoStatusMessage = "La camera n'est pas disponible sur cet appareil.";
                await ShowAlertAsync("Camera indisponible", PhotoStatusMessage, "OK");
                return;
            }

            var cameraStatus = await EnsureCameraPermissionAsync();
            if (cameraStatus != PermissionStatus.Granted)
            {
                PhotoStatusMessage = "Autorisation camera refusee. Active-la dans les reglages.";
                await ShowAlertAsync("Autorisation requise", PhotoStatusMessage, "OK");
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
                PhotoStatusMessage = "Photo capturee avec succes.";
            }
            else
            {
                PhotoStatusMessage = "Capture annulee.";
            }
        }
        catch (FeatureNotSupportedException)
        {
            PhotoStatusMessage = "La capture photo n'est pas supportee sur cet appareil.";
            await ShowAlertAsync("Fonction non supportee", PhotoStatusMessage, "OK");
        }
        catch (PermissionException)
        {
            PhotoStatusMessage = "Permission camera manquante.";
            await ShowAlertAsync("Autorisation requise", PhotoStatusMessage, "OK");
        }
        catch (OperationCanceledException)
        {
            PhotoStatusMessage = "Capture annulee.";
        }
        catch (Exception ex)
        {
            PhotoStatusMessage = "Erreur pendant la prise de photo.";
            await ShowAlertAsync("Erreur", $"Impossible de prendre une photo: {ex.Message}", "OK");
        }
        finally
        {
            IsTakingPhoto = false;
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
        PhotoStatusMessage = "Aucune photo prise";

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
        IsTakingPhoto = false;
        PhotoStatusMessage = "Aucune photo prise";
    }

    private static async Task<PermissionStatus> EnsureCameraPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status == PermissionStatus.Granted)
        {
            return status;
        }

        return await Permissions.RequestAsync<Permissions.Camera>();
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

