namespace Catalog.API.Books.CreateBook;

//public record GetBooksRequest();
public record GetBooksResponse(IEnumerable<Book> Books);

public class GetBooksEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/catalogs", async (ISender sender) =>
        {
            var result = await sender.Send(new GetBooksQuery());
            var response = result.Adapt<GetBooksResponse>();
            return Results.Ok(response);

        }).WithName("GetBooks")
            .Produces<GetBooksResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get All Books")
            .WithDescription("Get All Books");
    }
}
