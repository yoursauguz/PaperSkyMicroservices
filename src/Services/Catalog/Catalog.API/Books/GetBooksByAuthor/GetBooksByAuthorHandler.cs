namespace Catalog.API.Books.CreateBook;

public record GetBooksByAuthorQuery(string Author) : IQuery<GetBooksByAuthorResult>;
public record GetBooksByAuthorResult(IEnumerable<Book> Books);
public class GetBooksByAuthorQueryHandler(IDocumentSession session) : IQueryHandler<GetBooksByAuthorQuery, GetBooksByAuthorResult>
{
    public async Task<GetBooksByAuthorResult> Handle(GetBooksByAuthorQuery query, CancellationToken cancellationToken)
    {
        var books = await session.Query<Book>()
            .Where(book => book.Authors.Contains(query.Author))
            .ToListAsync(cancellationToken);

        return new GetBooksByAuthorResult(books);
    }
}
