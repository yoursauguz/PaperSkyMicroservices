namespace Catalog.API.Books.CreateBook;

//public record GetBooksByGenreRequest();
public record GetBooksByGenreResponse(IEnumerable<Book> Books);

public class GetBooksByGenreEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/catalogs/genre/{genre}", async (string genre, ISender sender) =>
        {
            var result = await sender.Send(new GetBooksByGenreQuery(genre));
            var response = result.Adapt<GetBooksByGenreResponse>();
            return Results.Ok(response);

        }).WithName("GetBooksByGenre")
            .Produces<GetBooksByGenreResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get Books By Genre")
            .WithDescription("Get Books By Genre");
    }
}
