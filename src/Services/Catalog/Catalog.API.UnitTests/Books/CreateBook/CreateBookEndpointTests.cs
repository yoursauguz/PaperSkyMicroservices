namespace Catalog.API.UnitTests.Books.CreateBook;

public class CreateBooksByIdEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    private static CreateBookRequest _createBookPayload = new(
         Name: "Domain-Driven Design",
         Genres: new List<string> { "Software Architecture", "Technology" },
         Description: "Tackling Complexity in the Heart of Software",
         ImagePath: "ddd-cover.jpg",
         Price: 55.00m,
         NumberOfPages: 560,
         Authors: new List<string> { "Eric Evans" },
         AverageRating: 4.7
   );

    public static TheoryData<CreateBookRequest?, string> InvalidCatalogPayloads =>
        new()
        {
            { _createBookPayload with { Name = string.Empty }, "Empty Name" },
            { _createBookPayload with { Price = -10.00m }, "Negative Price" },
            { _createBookPayload with { NumberOfPages = 0 }, "Zero Pages" },
            { _createBookPayload with { AverageRating = 6.0 }, "Out-of-range Rating" },
            { _createBookPayload with { Authors = new List<string>() }, "Empty Authors List" },
            { null, "Null Payload" }
        };

    public CreateBooksByIdEndpointTests(WebApplicationFactory<Program> applicationFactory, ITestOutputHelper output)
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

    [Theory]
    [MemberData(nameof(InvalidCatalogPayloads))]
    public async Task GivenInvalidPayload_WhenCreatingCatalog_ReturnsBadRequest(
        CreateBookRequest? command,
        string scenario)
    {
        _output.WriteLine($"Executing Scenario: {scenario}");

        var response = await _client.PostAsJsonAsync("/api/v1/catalogs", command);

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