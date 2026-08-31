namespace Catalog.API.Books.DeleteBook;

//public record DeleteBookByIdRequest(Guid Id);
public record DeleteBookByIdResponse(bool IsSuccess);

public class DeleteBookByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/v1/catalogs/{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteBookByIdCommand(id));
            var response = result.Adapt<DeleteBookByIdResponse>();
            return Results.Ok(response);

        }).WithName("DeleteBookById")
           .Produces<DeleteBookByIdResponse>(StatusCodes.Status200OK)
           .ProducesProblem(StatusCodes.Status400BadRequest)
           .WithSummary("Delete Book By Id")
           .WithDescription("Delete Book By Id");
    }
}
