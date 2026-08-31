namespace Catalog.API.Books.UpdateBook;

public record UpdateBookRequest(Guid Id,string Name, List<string> Genres, string? Description, string? ImagePath, decimal Price, int NumberOfPages, List<string> Authors, double AverageRating);
public record UpdateBookResponse(bool IsSuccess);

public class UpdateBookEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/v1/catalogs", async (UpdateBookRequest request, ISender sender) =>
        {
            var command = request.Adapt<UpdateBookCommand>();
            var result = await sender.Send(command);
            var response = result.Adapt<UpdateBookResponse>();
            return Results.Ok(response);

        }).WithName("UpdateBook")
            .Produces<UpdateBookResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Update a Book")
            .WithDescription("Update a Book");
    }
}
