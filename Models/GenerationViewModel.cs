namespace Pokédex.Models;

public class GenerationViewModel
{
     public List<SimplePokemonViewModel> PokemonData { get; set; } = new();
     public int Generation { get; set; }
}