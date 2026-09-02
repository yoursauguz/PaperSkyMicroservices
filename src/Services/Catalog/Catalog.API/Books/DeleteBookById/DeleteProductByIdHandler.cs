namespace Catalog.API.Books.DeleteBook;
public record DeleteBookByIdCommand(Guid Id) : ICommand<DeleteBookByIdResult>;
public record DeleteBookByIdResult(bool IsSuccess);

public class DeleteBookCommandValidator : AbstractValidator<DeleteBookByIdCommand>
{
    public DeleteBookCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Book id is required");
    }
}

public class DeleteBookByIdCommandHandler(IDocumentSession session) : ICommandHandler<DeleteBookByIdCommand, DeleteBookByIdResult>
{
    public async Task<DeleteBookByIdResult> Handle(DeleteBookByIdCommand command, CancellationToken cancellationToken)
    {
        session.Delete<Book>(command.Id);
        await session.SaveChangesAsync(cancellationToken);
        return new DeleteBookByIdResult(true);
    }
}