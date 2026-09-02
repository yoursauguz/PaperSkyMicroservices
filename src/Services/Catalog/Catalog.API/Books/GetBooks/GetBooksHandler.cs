
using Marten.Pagination;

namespace Catalog.API.Books.CreateBook;

public record GetBooksQuery(int? PageNumber = 1, int? PageSize = 10) : IQuery<GetBooksResult>;
public record GetBooksResult(IEnumerable<Book> Books);

internal class GetBooksQueryHandler(IDocumentSession session) : IQueryHandler<GetBooksQuery, GetBooksResult>
{
    public async Task<GetBooksResult> Handle(GetBooksQuery query, CancellationToken cancellationToken)
    {
        var books = await session.Query<Book>().ToPagedListAsync(query.PageNumber ?? 1, query.PageSize ?? 10, cancellationToken);
        return new GetBooksResult(books);
    }
}
