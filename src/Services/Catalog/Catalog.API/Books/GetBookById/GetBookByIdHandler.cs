namespace Catalog.API.Books.CreateBook;

public record GetBookByIdQuery(Guid Id) : IQuery<GetBookByIdResult>;
public record GetBookByIdResult(Book Book);
public class GetBookByIdQueryHandler(IDocumentSession session) : IQueryHandler<GetBookByIdQuery, GetBookByIdResult>
{
    public async Task<GetBookByIdResult> Handle(GetBookByIdQuery query, CancellationToken cancellationToken)
    {
        var book = await session.LoadAsync<Book>(query.Id, cancellationToken);

        if (book == null)
            throw new BookNotFoundException(query.Id);

        return new GetBookByIdResult(book);

    }
}
