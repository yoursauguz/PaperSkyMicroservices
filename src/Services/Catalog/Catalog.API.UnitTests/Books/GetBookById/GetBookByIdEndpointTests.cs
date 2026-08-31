namespace Catalog.API.UnitTests.Books.GetBookById;

public class GetBookByIdEndpointTests
{
    private readonly Mock<ISender> _senderMock;

    public GetBookByIdEndpointTests()
    {
        _senderMock = new Mock<ISender>();
    }

    [Fact]
    public async Task GivenExistingBookId_WhenGetBookById_ReturnsOkWithResponse()
    {
        var bookId = Guid.NewGuid();
        var sampleBook = new Book
        {
            Id = bookId,
            Name = "Clean Code",
            Genres = new List<string> { "Technology" },
            Description = "A Handbook of Agile Software Craftsmanship",
            ImagePath = "clean-code.jpg",
            Price = 40.00m,
            NumberOfPages = 464,
            Authors = new List<string> { "Robert C. Martin" },
            AverageRating = 4.8
        };

        var queryResult = new GetBookByIdResult(sampleBook);

        _senderMock
            .Setup(s => s.Send(It.Is<GetBookByIdQuery>(q => q.Id == bookId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryResult);

        var result = await GetBookByIdEndpointDelegate(bookId, _senderMock.Object);

        var okResult = Assert.IsType<Ok<GetBookByIdResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.NotNull(okResult.Value.Book);
        Assert.Equal(bookId, okResult.Value.Book.Id);
        Assert.Equal("Clean Code", okResult.Value.Book.Name);

        _senderMock.Verify(
            s => s.Send(It.Is<GetBookByIdQuery>(q => q.Id == bookId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GivenNonExistentBookId_WhenGetBookById_ThrowsBookNotFoundException()
    {
        var nonExistentBookId = Guid.NewGuid();

        _senderMock
            .Setup(s => s.Send(It.Is<GetBookByIdQuery>(q => q.Id == nonExistentBookId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BookNotFoundException(nonExistentBookId));

        var exception = await Assert.ThrowsAsync<BookNotFoundException>(
            () => GetBookByIdEndpointDelegate(nonExistentBookId, _senderMock.Object));

        Assert.Contains(nonExistentBookId.ToString(), exception.Message);

        _senderMock.Verify(
            s => s.Send(It.Is<GetBookByIdQuery>(q => q.Id == nonExistentBookId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private async Task<IResult> GetBookByIdEndpointDelegate(Guid id, ISender sender)
    {
        var result = await sender.Send(new GetBookByIdQuery(id));
        var response = result.Adapt<GetBookByIdResponse>();
        return Results.Ok(response);
    }
}