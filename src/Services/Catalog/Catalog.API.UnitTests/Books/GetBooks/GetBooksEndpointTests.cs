namespace Catalog.API.UnitTests.Books.GetBooks;

public class GetBooksEndpointTests
{
    private readonly Mock<ISender> _senderMock;

    public GetBooksEndpointTests()
    {
        _senderMock = new Mock<ISender>();
    }

    [Fact]
    public async Task GivenExistingBooks_WhenGetBooks_ReturnsOkWithBooksList()
    {
        var books = new List<Book>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Clean Code",
                Genres = new List<string> { "Technology" },
                Price = 40.00m,
                NumberOfPages = 464,
                Authors = new List<string> { "Robert C. Martin" },
                AverageRating = 4.8
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Domain-Driven Design",
                Genres = new List<string> { "Architecture" },
                Price = 55.00m,
                NumberOfPages = 560,
                Authors = new List<string> { "Eric Evans" },
                AverageRating = 4.7
            }
        };

        var queryResult = new GetBooksResult(books);

        _senderMock
            .Setup(s => s.Send(It.IsAny<GetBooksQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryResult);

        var result = await GetBooksEndpointDelegate(_senderMock.Object);

        var okResult = Assert.IsType<Ok<GetBooksResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.NotNull(okResult.Value.Books);
        Assert.Equal(2, okResult.Value.Books.Count());

        _senderMock.Verify(
            s => s.Send(It.IsAny<GetBooksQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GivenNoBooks_WhenGetBooks_ReturnsOkWithEmptyList()
    {
        var queryResult = new GetBooksResult(new List<Book>());

        _senderMock
            .Setup(s => s.Send(It.IsAny<GetBooksQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryResult);

        var result = await GetBooksEndpointDelegate(_senderMock.Object);

        var okResult = Assert.IsType<Ok<GetBooksResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.NotNull(okResult.Value.Books);
        Assert.Empty(okResult.Value.Books);

        _senderMock.Verify(
            s => s.Send(It.IsAny<GetBooksQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private async Task<IResult> GetBooksEndpointDelegate(ISender sender)
    {
        var result = await sender.Send(new GetBooksQuery());
        var response = result.Adapt<GetBooksResponse>();
        return Results.Ok(response);
    }
}