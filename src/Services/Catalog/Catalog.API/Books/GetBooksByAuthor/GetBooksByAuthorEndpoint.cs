namespace Catalog.API.Books.CreateBook;

//public record GetBooksByAuthorRequest();
public record GetBooksByAuthorResponse(IEnumerable<Book> Books);

public class GetBooksByAuthorEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/catalogs/author/{author}", async (string author, ISender sender) =>
        {
            var result = await sender.Send(new GetBooksByAuthorQuery(author));
            var response = result.Adapt<GetBooksByAuthorResponse>();
            return Results.Ok(response);

        }).WithName("GetBooksByAuthor")
            .Produces<GetBooksByAuthorResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get Books By Author")
            .WithDescription("Get Books By Author");
    }
}
