
namespace Basket.API.Exceptions;

public class BasketNotFoundException : NotFoundException
{
    public BasketNotFoundException(string nonExistentBasket) : base("Basket", nonExistentBasket)
    {

    }
}
