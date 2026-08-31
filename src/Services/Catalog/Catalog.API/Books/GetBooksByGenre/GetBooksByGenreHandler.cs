namespace Catalog.API.Books.CreateBook;

public record GetBooksByGenreQuery(string Genre) : IQuery<GetBooksByGenreResult>;
public record GetBooksByGenreResult(IEnumerable<Book> Books);
public class GetBooksByGenreQueryHandler(IDocumentSession session, ILogger<GetBooksByGenreQueryHandler> logger) : IQueryHandler<GetBooksByGenreQuery, GetBooksByGenreResult>
{
    public async Task<GetBooksByGenreResult> Handle(GetBooksByGenreQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("GetBooksByGenreQueryHandler.Handle called with {@query}", query);

        var books = await session.Query<Book>()
            .Where(book => book.Genres.Contains(query.Genre))
            .ToListAsync(cancellationToken);

        return new GetBooksByGenreResult(books);
    }
}
