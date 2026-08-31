namespace Catalog.API.UnitTests.Books.GetBooksByGenre;

public class GetBooksByGenreEndpointTests
{
    private readonly Mock<ISender> _senderMock;

    public GetBooksByGenreEndpointTests()
    {
        _senderMock = new Mock<ISender>();
    }

    [Fact]
    public async Task GivenGenre_WhenEndpointInvoked_ReturnsOnlyMatchingBooks()
    {
        var requestedGenre = "Architecture";

        var allCatalogBooks = new List<Book>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Domain-Driven Design",
                Genres = new List<string> { "Software Architecture", "Technology" },
                Price = 55.00m,
                Authors = new List<string> { "Eric Evans" }
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
                Authors = new List<string> { "Sam Newman" }
            }
        };

        _senderMock
            .Setup(s => s.Send(It.Is<GetBooksByGenreQuery>(q => q.Genre == requestedGenre), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetBooksByGenreQuery query, CancellationToken _) =>
            {
                var filtered = allCatalogBooks
                    .Where(b => b.Genres.Any(g => g.Contains(query.Genre, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                return new GetBooksByGenreResult(filtered);
            });

        var result = await GetBooksByGenreEndpointDelegate(requestedGenre, _senderMock.Object);

        var okResult = Assert.IsType<Ok<GetBooksByGenreResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.NotNull(okResult.Value.Books);

        var returnedBooks = okResult.Value.Books.ToList();

        Assert.Equal(2, returnedBooks.Count);
        Assert.All(returnedBooks, book => Assert.Contains(book.Genres, g => g.Contains(requestedGenre, StringComparison.OrdinalIgnoreCase)));
        Assert.DoesNotContain(returnedBooks, book => book.Name == "Clean Code");

        _senderMock.Verify(s => s.Send(It.Is<GetBooksByGenreQuery>(q => q.Genre == requestedGenre), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GivenUnmatchedGenre_WhenEndpointInvoked_ReturnsEmptyCollection()
    {
        var requestedGenre = "NonExistentGenre";

        _senderMock
            .Setup(s => s.Send(It.Is<GetBooksByGenreQuery>(q => q.Genre == requestedGenre), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetBooksByGenreResult(new List<Book>()));

        var result = await GetBooksByGenreEndpointDelegate(requestedGenre, _senderMock.Object);

        var okResult = Assert.IsType<Ok<GetBooksByGenreResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Empty(okResult.Value.Books);

        _senderMock.Verify(s => s.Send(It.Is<GetBooksByGenreQuery>(q => q.Genre == requestedGenre), It.IsAny<CancellationToken>()), Times.Once);
    }
    private async Task<IResult> GetBooksByGenreEndpointDelegate(string genre, ISender sender)
    {
        var result = await sender.Send(new GetBooksByGenreQuery(genre));
        var response = result.Adapt<GetBooksByGenreResponse>();
        return Results.Ok(response);
    }
}