using MicroserviceUtilities.Exceptions;

namespace Catalog.API.Exceptions;

public class BookNotFoundException : NotFoundException
{
    public BookNotFoundException(Guid nonExistentBookId) : base("Book",nonExistentBookId)
    {

    }
}
