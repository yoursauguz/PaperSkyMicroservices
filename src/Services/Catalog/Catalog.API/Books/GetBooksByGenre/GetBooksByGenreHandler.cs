namespace Catalog.API.Books.CreateBook;

public record GetBooksByGenreQuery(string Genre) : IQuery<GetBooksByGenreResult>;
public record GetBooksByGenreResult(IEnumerable<Book> Books);
public class GetBooksByGenreQueryHandler(IDocumentSession session) : IQueryHandler<GetBooksByGenreQuery, GetBooksByGenreResult>
{
    public async Task<GetBooksByGenreResult> Handle(GetBooksByGenreQuery query, CancellationToken cancellationToken)
    {
        var books = await session.Query<Book>()
            .Where(book => book.Genres.Contains(query.Genre))
            .ToListAsync(cancellationToken);

        return new GetBooksByGenreResult(books);
    }
}
