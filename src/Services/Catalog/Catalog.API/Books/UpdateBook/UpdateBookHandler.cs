namespace Catalog.API.Books.UpdateBook;

public record UpdateBookCommand(Guid Id,string Name, List<string> Genres, string? Description, string? ImagePath, decimal Price, int NumberOfPages, List<string> Authors, double AverageRating)
: ICommand<UpdateBookResult>;

public record UpdateBookResult(bool IsSuccess);

public class UpdateBookCommandValidator : AbstractValidator<UpdateBookCommand>
{
    public UpdateBookCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Book id is required");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Book name cannot be empty");
        RuleFor(x => x.Genres).NotEmpty().WithMessage("Genres cannot be empty");
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than 0");
        RuleFor(x => x.NumberOfPages).GreaterThan(0).WithMessage("Number of pages must be greater than 0");
        RuleFor(x => x.Authors).NotEmpty().WithMessage("Genres cannot be empty");
        RuleFor(x => x.AverageRating).GreaterThan(0).WithMessage("Average rating must be between 1 and 5");
        RuleFor(x => x.AverageRating).LessThanOrEqualTo(5).WithMessage("Average rating must be between 1 and 5");
    }
}

public class UpdateBookCommandHandler(IDocumentSession session, ILogger<UpdateBookCommandHandler> logger) : ICommandHandler<UpdateBookCommand, UpdateBookResult>
{
    public async Task<UpdateBookResult> Handle(UpdateBookCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("UpdateBookCommandHandler.Handle called with {@command}", command);

        var book = await session.LoadAsync<Book>(command.Id, cancellationToken);

        if (book is null)
            throw new BookNotFoundException(command.Id);

        book.Name = command.Name;
        book.Genres = command.Genres;
        book.Description = command.Description;
        book.ImagePath = command.ImagePath;
        book.Price = command.Price;
        book.NumberOfPages = command.NumberOfPages;
        book.Authors = command.Authors;
        book.AverageRating = command.AverageRating;

        session.Update(book);
        await session.SaveChangesAsync(cancellationToken);

        return new UpdateBookResult(true);
    }
}
