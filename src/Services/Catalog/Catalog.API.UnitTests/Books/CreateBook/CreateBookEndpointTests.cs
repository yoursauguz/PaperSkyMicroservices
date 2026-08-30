using Catalog.API.Books.CreateBook;

namespace Catalog.API.UnitTests.Books.CreateBook;

public class CreateBookEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    private CreateBookRequest _createBookPayload = new(
              Name: "Domain-Driven Design",
              Genres: new List<string> { "Software Architecture", "Technology" },
              Description: "Tackling Complexity in the Heart of Software",
              ImagePath: "ddd-cover.jpg",
              Price: 55.00m,
              NumberOfPages: 560,
              Authors: new List<string> { "Eric Evans" },
              AverageRating: 4.7
    );


    public CreateBookEndpointTests(WebApplicationFactory<Program> applicationFactory, ITestOutputHelper output)
    {
        _client = applicationFactory.CreateClient();
        _output = output;
    }

    [Fact]
    public async Task GivenACatalogIsCreated_WithValidPayload_ReturnsCreatedStatusAndId()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/catalogs", _createBookPayload);
        _output.WriteLine($"Response Status Code: {response.StatusCode}");

        await ItMustCreateANewBookAsync(response);
    }

    [Fact]
    public async Task GivenMinimalPayload_WhenCreatingCatalog_ReturnsCreatedStatus()
    {
        var command = _createBookPayload with
        {
            Description = null,
            ImagePath = null
        };

        var response = await _client.PostAsJsonAsync("/api/v1/catalogs", command);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        _output.WriteLine($"Response Status Code: {response.StatusCode}");

        await ItMustCreateANewBookAsync(response);
    }

    [Fact]
    public async Task GivenMissingName_WhenCreatingCatalog_ReturnsBadRequest()
    {
        var command = _createBookPayload with
        {
            Name = string.Empty
        };

        var response = await _client.PostAsJsonAsync("/api/v1/catalogs", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _output.WriteLine($"Validation caught missing name. Status: {response.StatusCode}");
    }

    [Fact]
    public async Task GivenNegativePrice_WhenCreatingCatalog_ReturnsBadRequest()
    {
        var command = _createBookPayload with
        {
            Price = -10.00m,
        };

        var response = await _client.PostAsJsonAsync("/api/v1/catalogs", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GivenZeroPages_WhenCreatingCatalog_ReturnsBadRequest()
    {
        var command = _createBookPayload with
        {
            NumberOfPages = 0,
        };

        var response = await _client.PostAsJsonAsync("/api/v1/catalogs", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GivenOutOfRangeRating_WhenCreatingCatalog_ReturnsBadRequest()
    {
        var command = _createBookPayload with
        {
            AverageRating = 6.0
        };

        var response = await _client.PostAsJsonAsync("/api/v1/catalogs", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GivenEmptyAuthorsList_WhenCreatingCatalog_ReturnsBadRequest()
    {
        var command = _createBookPayload with
        {
            Authors = new List<string>()
        };

        var response = await _client.PostAsJsonAsync("/api/v1/catalogs", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GivenEmptyGenresList_WhenCreatingCatalog_ReturnsBadRequest()
    {
        var command = _createBookPayload with
        {
            Genres =new List<string>()
        };

        var response = await _client.PostAsJsonAsync("/api/v1/catalogs", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GivenEmptyBody_WhenCreatingCatalog_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync<object?>("/api/v1/catalogs", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task ItMustCreateANewBookAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

        Assert.NotNull(result);
        Assert.True(result.ContainsKey("id"));

        var generatedId = result["id"]?.ToString();
        _output.WriteLine($"Generated Catalog ID: {generatedId}");
    }
}