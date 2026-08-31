namespace Catalog.API.UnitTests.Books.GetBooksByAuthor;

public class GetBooksByAuthorEndpointTests
{
    private readonly Mock<ISender> _senderMock;

    public GetBooksByAuthorEndpointTests()
    {
        _senderMock = new Mock<ISender>();
    }

    [Fact]
    public async Task GivenAuthor_WhenEndpointInvoked_ReturnsOnlyMatchingBooks()
    {
        var requestedAuthor = "Michael";

        var allCatalogBooks = new List<Book>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Domain-Driven Design",
                Genres = new List<string> { "Software Architecture", "Technology" },
                Price = 55.00m,
                Authors = new List<string> { "Michael" }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Clean Code",
                Genres = new List<string> { "Technology", "Programming" },
                Price = 40.00m,
                Authors = new List<string> { "Robert C. Martin" }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Building Microservices",
                Genres = new List<string> { "Software Architecture", "Microservices" },
                Price = 48.00m,
                Authors = new List<string> { "Michael" }
            }
        };

        _senderMock
            .Setup(s => s.Send(It.Is<GetBooksByAuthorQuery>(q => q.Author == requestedAuthor), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetBooksByAuthorQuery query, CancellationToken _) =>
            {
                var filtered = allCatalogBooks
                    .Where(b => b.Authors.Any(a => a.Contains(query.Author, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                return new GetBooksByAuthorResult(filtered);
            });

        var result = await GetBooksByAuthorEndpointDelegate(requestedAuthor, _senderMock.Object);

        var okResult = Assert.IsType<Ok<GetBooksByAuthorResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.NotNull(okResult.Value.Books);

        var returnedBooks = okResult.Value.Books.ToList();

        Assert.Equal(2, returnedBooks.Count);
        Assert.All(returnedBooks, book => Assert.Contains(book.Authors, g => g.Contains(requestedAuthor, StringComparison.OrdinalIgnoreCase)));
        Assert.DoesNotContain(returnedBooks, book => book.Name == "Clean Code");

        _senderMock.Verify(s => s.Send(It.Is<GetBooksByAuthorQuery>(q => q.Author == requestedAuthor), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GivenUnmatchedAuthor_WhenEndpointInvoked_ReturnsEmptyCollection()
    {
        var requestedAuthor = "NonExistentAuthor";

        _senderMock
            .Setup(s => s.Send(It.Is<GetBooksByAuthorQuery>(q => q.Author == requestedAuthor), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetBooksByAuthorResult(new List<Book>()));

        var result = await GetBooksByAuthorEndpointDelegate(requestedAuthor, _senderMock.Object);

        var okResult = Assert.IsType<Ok<GetBooksByAuthorResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Empty(okResult.Value.Books);

        _senderMock.Verify(s => s.Send(It.Is<GetBooksByAuthorQuery>(q => q.Author == requestedAuthor), It.IsAny<CancellationToken>()), Times.Once);
    }
    private async Task<IResult> GetBooksByAuthorEndpointDelegate(string author, ISender sender)
    {
        var result = await sender.Send(new GetBooksByAuthorQuery(author));
        var response = result.Adapt<GetBooksByAuthorResponse>();
        return Results.Ok(response);
    }
}