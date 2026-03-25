using RomanApp.Models;
using System.Text.Json;

namespace RomanApp.Services;

public interface ICapturedPokemonService
{
    Task SaveCapturedPokemonAsync(CapturedPokemon pokemon);
    Task<List<CapturedPokemon>> LoadCapturedPokemonsAsync();
    Task DeleteCapturedPokemonAsync(string id);
}

public class CapturedPokemonService : ICapturedPokemonService
{
    private const string CapturedPokemonsFileName = "captured_pokemons.json";
    private readonly string _documentsPath;

    public CapturedPokemonService()
    {
        try
        {
            _documentsPath = FileSystem.AppDataDirectory;
            
            // Vérifier/créer le répertoire s'il n'existe pas
            if (!Directory.Exists(_documentsPath))
            {
                Directory.CreateDirectory(_documentsPath);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing CapturedPokemonService: {ex.Message}");
            _documentsPath = Path.GetTempPath();
        }
    }

    public async Task SaveCapturedPokemonAsync(CapturedPokemon pokemon)
    {
        try
        {
            var filePath = Path.Combine(_documentsPath, CapturedPokemonsFileName);
            var pokemons = await LoadCapturedPokemonsAsync();
            
            // Ajouter ou mettre à jour le Pokémon
            var existingPokemon = pokemons.FirstOrDefault(p => p.Id == pokemon.Id);
            if (existingPokemon != null)
            {
                pokemons.Remove(existingPokemon);
            }
            pokemons.Add(pokemon);

            var json = JsonSerializer.Serialize(pokemons);
            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            throw new Exception($"Erreur lors de la sauvegarde du Pokémon: {ex.Message}");
        }
    }

    public async Task<List<CapturedPokemon>> LoadCapturedPokemonsAsync()
    {
        try
        {
            var filePath = Path.Combine(_documentsPath, CapturedPokemonsFileName);
            
            if (!File.Exists(filePath))
            {
                return new List<CapturedPokemon>();
            }

            var json = await File.ReadAllTextAsync(filePath);
            var pokemons = JsonSerializer.Deserialize<List<CapturedPokemon>>(json) ?? new List<CapturedPokemon>();
            return pokemons;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erreur lors du chargement des Pokémons: {ex.Message}");
        }
    }

    public async Task DeleteCapturedPokemonAsync(string id)
    {
        try
        {
            var filePath = Path.Combine(_documentsPath, CapturedPokemonsFileName);
            var pokemons = await LoadCapturedPokemonsAsync();
            var pokemonToDelete = pokemons.FirstOrDefault(p => p.Id == id);
            
            if (pokemonToDelete != null)
            {
                pokemons.Remove(pokemonToDelete);
                var json = JsonSerializer.Serialize(pokemons);
                await File.WriteAllTextAsync(filePath, json);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Erreur lors de la suppression du Pokémon: {ex.Message}");
        }
    }
}

