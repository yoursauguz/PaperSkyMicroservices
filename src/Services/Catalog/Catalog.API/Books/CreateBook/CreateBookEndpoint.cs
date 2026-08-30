namespace Catalog.API.Books.CreateBook;

public record CreateBookRequest(string Name, List<string> Genres, string? Description, string? ImagePath, decimal Price, int NumberOfPages, List<string> Authors, double AverageRating);
public record CreateBookResponse(Guid Id);

public class CreateBookEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/catalogs", async (CreateBookRequest request, ISender sender) =>
        {
            var command = request.Adapt<CreateBookCommand>();
            var result = await sender.Send(command);
            var response = result.Adapt<CreateBookResponse>();
            return Results.Created($"/api/v1/catalogs/{response.Id}", response);

        }).WithName("CreateBook")
            .Produces<CreateBookResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Create a Book")
            .WithDescription("Create a Book");
    }
}
