namespace Catalog.API.Models;

public class Book
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public List<string> Genres { get; set; } = new();
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
    public decimal Price { get; set; }
    public int NumberOfPages { get; set; }
    public List<string> Authors { get; set; } = new();
    public double AverageRating { get; set; }
}
