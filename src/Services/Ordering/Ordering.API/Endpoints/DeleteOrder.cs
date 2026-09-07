using Ordering.Application.Orders.Commands.DeleteOrder;

namespace Ordering.API.Endpoints;

public record DeleteResponse(bool IsSuccess);

public class DeleteOrder : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/v1/orders/{id}", async (Guid id, ISender sender) =>
        {            
            var result = await sender.Send(new DeleteOrderCommand(id));
            var response = result.Adapt<DeleteResponse>();
            return Results.Ok(response);
        })
        .WithName("DeleteOrder")
        .Produces<DeleteResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Delete order")
        .WithDescription("Delete order");
    }
}
