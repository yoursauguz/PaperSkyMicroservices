namespace Catalog.API.Books.CreateBook;

public record CreateBookCommand(string Name, List<string> Genres, string? Description, string? ImagePath, decimal Price, int NumberOfPages, List<string> Authors, double AverageRating)
    : ICommand<CreateBookResult>;

public record CreateBookResult(Guid Id);

public class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
{
    public CreateBookCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Book name cannot be empty");
        RuleFor(x => x.Genres).NotEmpty().WithMessage("Genres cannot be empty");
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than 0");
        RuleFor(x => x.NumberOfPages).GreaterThan(0).WithMessage("Number of pages must be greater than 0");
        RuleFor(x => x.Authors).NotEmpty().WithMessage("Authors cannot be empty");
        RuleFor(x => x.AverageRating)
                    .InclusiveBetween(1, 5)
                    .WithMessage("Average rating must be between 1 and 5");
    }
}

internal class CreateBookCommandHandler(IDocumentSession session, ILogger<CreateBookCommandHandler> logger) : ICommandHandler<CreateBookCommand, CreateBookResult>
{
    public async Task<CreateBookResult> Handle(CreateBookCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("CreateBookCommandHandler.Handle called with {@command}", command);

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

        return new CreateBookResult(book.Id);
    }
}
