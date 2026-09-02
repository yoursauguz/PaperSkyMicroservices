using Marten.Schema;

namespace Catalog.API.Data;

public class CatalogInitialData : IInitialData
{
    public async Task Populate(IDocumentStore store, CancellationToken cancellation)
    {
        using var session = store.LightweightSession();

        if (await session.Query<Book>().AnyAsync())
            return;

        session.Store<Book>(GetPreConfiguredBooks());
        await session.SaveChangesAsync();
    }

    private IEnumerable<Book> GetPreConfiguredBooks()
    {
        return new List<Book>
        {
            new()
            {
                Id = new Guid("01a04f14-a789-7d61-81ce-3ab2ddd3d441"),
                Name = "Clean Code",
                Genres = new List<string> { "Technology" },
                Price = 40.00m,
                NumberOfPages = 464,
                Authors = new List<string> { "Robert C. Martin" },
                AverageRating = 4.8
            },
            new()
            {
                Id = new Guid("01a04f14-a789-7d61-81ce-3ae2ddd3d441"),
                Name = "Domain-Driven Design",
                Genres = new List<string> { "Architecture" },
                Price = 55.00m,
                NumberOfPages = 560,
                Authors = new List<string> { "Eric Evans" },
                AverageRating = 4.7
            }
        };
    }
}
