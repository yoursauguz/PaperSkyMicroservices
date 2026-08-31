namespace Catalog.API.Books.CreateBook;

//public record GetBookByIdRequest();
public record GetBookByIdResponse(Book Book);

public class GetBookByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/catalogs/{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetBookByIdQuery(id));
            var response = result.Adapt<GetBookByIdResponse>();
            return Results.Ok(response);

        }).WithName("GetBookById")
            .Produces<GetBookByIdResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get Book By Id")
            .WithDescription("Get Book By Id");
    }
}
