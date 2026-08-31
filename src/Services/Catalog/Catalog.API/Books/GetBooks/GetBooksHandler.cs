
namespace Catalog.API.Books.CreateBook;

public record GetBooksQuery() : IQuery<GetBooksResult>;
public record GetBooksResult(IEnumerable<Book> Books);

internal class GetBooksQueryHandler(IDocumentSession session, ILogger<GetBooksQueryHandler> logger) : IQueryHandler<GetBooksQuery, GetBooksResult>
{
    public async Task<GetBooksResult> Handle(GetBooksQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("GetBooksQueryHandler.Handle called with {@query}", query);

        var books = await session.Query<Book>().ToListAsync(cancellationToken);

        return new GetBooksResult(books);

    }
}
