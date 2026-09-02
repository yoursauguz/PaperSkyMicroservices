namespace Catalog.API.Books.CreateBook;

public record GetBooksRequest(int? PageNumber = 1, int? PageSize = 10);
public record GetBooksResponse(IEnumerable<Book> Books);

public class GetBooksEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/catalogs", async ([AsParameters] GetBooksRequest request,ISender sender) =>
        {
            var query = request.Adapt<GetBooksQuery>();
            var result = await sender.Send(query);
            var response = result.Adapt<GetBooksResponse>();
            return Results.Ok(response);

        }).WithName("GetBooks")
            .Produces<GetBooksResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get All Books")
            .WithDescription("Get All Books");
    }
}
