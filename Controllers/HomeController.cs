using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pokédex.Models;

namespace Pokédex.Controllers;

public class HomeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HomeController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index(int? generation = 1)
    {
        List<SimplePokemonViewModel> pokemonList;

        // Using the HttpClientFactory, we create a new HttpClient
        using (var client = _httpClientFactory.CreateClient("pokeapi"))
        {
            // We get the JSON data from the PokeAPI
            var jsonData = await client
                .GetAsync("generation/" + generation)
                .Result
                .Content
                .ReadAsStringAsync();

            // We parse the JSON data into a JObject
            var generationData = JObject
                .Parse(jsonData);

            // We get the list of Pokemon from the JObject
            var pokemonSpecies = generationData["pokemon_species"];

            // We create a new list of Pokemon, sorted by ID
            var sortedPokemonList = pokemonSpecies!.Select(entry =>
                {
                    // First we extract the ID from the URL
                    var id = entry["url"]!
                        .ToString()
                        .Replace("https://pokeapi.co/api/v2/pokemon-species/", "")
                        .Replace("/", "");

                    // Then we get the name of the Pokemon
                    var name = entry["name"]!
                        .ToString();

                    // Finally we return a new SimplePokemonViewModel
                    return new SimplePokemonViewModel
                    {
                        Id = int.Parse(id),
                        Name = string.Concat(name.First().ToString().ToUpper(), name.AsSpan(1))
                    };
                })
                .OrderBy(pokemon => pokemon.Id)
                .ToList();

            // We add the image URL to each Pokemon
            foreach (var pokemon in sortedPokemonList)
            {
                pokemon.ImageURL = $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{pokemon.Id}.png";
            }

            // And finally we set the list of Pokemon to the sorted list
            pokemonList = sortedPokemonList;
        }

        // Return the view with the list of Pokemon and the current generation, or 1 if none is specified (default)
        return View(new GenerationViewModel
        {
            PokemonData = pokemonList,
            Generation = generation ?? 1
        });
    }

    [HttpPost]
    public IActionResult ShowGeneration(string pokemonData, string? query)
    {
        // We deserialize the JSON data into a list of SimplePokemonViewModel
        var pokemonList = JsonConvert
            .DeserializeObject<List<SimplePokemonViewModel>>(pokemonData);

        // We filter the list of Pokemon by the query
        var newList = pokemonList?
            .Where(pokemon => pokemon.Name.Contains(query ?? "", StringComparison.OrdinalIgnoreCase))
            .ToList() ?? new List<SimplePokemonViewModel>();

        // Then we return the partial view with the filtered list
        return PartialView("_Generation", newList);
    }

    [HttpGet("/pokemon/{id:int}")]
    public async Task<IActionResult> Pokemon(int id)
    {
        PokemonViewModel pokemon;

        using (var client = _httpClientFactory.CreateClient("pokeapi"))
        {
            // Get our JSON data from the PokeAPI
            var jsonData = await client
                .GetAsync("pokemon/" + id)
                .Result
                .Content
                .ReadAsStringAsync();

            // Parse the JSON data into a JObject
            var pokemonData = JObject
                .Parse(jsonData);

            // Extract the data we need from the JObject
            
            // We get the name of the Pokemon
            var name = pokemonData["name"]!
                .ToString();

            // We get the height from the Pokemon
            var height = pokemonData["height"]!
                .ToObject<int>();

            // We get the weight from the Pokemon
            var weight = pokemonData["weight"]!
                .ToObject<int>();

            // We get the types from the Pokemon
            var types = pokemonData["types"]!
                .Select(entry => entry["type"]!["name"]!.ToString())
                .ToList();

            // Then we create a new PokemonViewModel with the data
            pokemon = new PokemonViewModel
            {
                Id = id,
                Name = string.Concat(name.First().ToString().ToUpper(), name.AsSpan(1)),
                Height = height,
                Weight = weight,
                Types = types
            };
        }

        // And finally we return the view with the Pokemon
        return View(pokemon);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}