namespace Pokédex.Models;

public class PokemonViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Height { get; set; }
    public int Weight { get; set; }
    public List<string> Types { get; set; } = new();
}