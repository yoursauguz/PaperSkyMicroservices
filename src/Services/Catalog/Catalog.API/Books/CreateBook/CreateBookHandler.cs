
namespace Catalog.API.Books.CreateBook;

public record CreateBookCommand(string Name,List<string> Genres,string? Description,string? ImagePath,decimal Price,int NumberOfPages,List<string> Authors,double AverageRating)
    :ICommand<CreateProductResult>;

public record CreateProductResult(Guid Id);
internal class CreateBookCommandHandler(IDocumentSession session) : ICommandHandler<CreateBookCommand, CreateProductResult>
{
    public async Task<CreateProductResult> Handle(CreateBookCommand command, CancellationToken cancellationToken)
    {

        var book = new Book
        {
            Name = command.Name,
            Genres = command.Genres,
            Description = command.Description,
            ImagePath = command.ImagePath,
            Price = command.Price,
            NumberOfPages = command.NumberOfPages,
            Authors = command.Authors,
            AverageRating = command.AverageRating
        };

        session.Store(book);
        await session.SaveChangesAsync(cancellationToken);

        return new CreateProductResult(book.Id);
    }
}
