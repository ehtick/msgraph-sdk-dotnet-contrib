using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Graph.Community.Test
{
  public class ListRequestTests
  {
    private readonly ITestOutputHelper output;

    private readonly string mockWebUrl = "https://mock.sharepoint.com/sites/mockSite";

    public ListRequestTests(ITestOutputHelper output)
    {
      this.output = output;
    }


    [Fact]
    public void GetById_GeneratesCorrectRequestUriAndHeaders()
    {
      // ARRANGE
      var mockListId = Guid.NewGuid();
      var expectedUri = new Uri($"{mockWebUrl}/_api/web/lists('{mockListId}')");

      using HttpResponseMessage response = new HttpResponseMessage();
      using GraphServiceTestClient testClient = GraphServiceTestClient.Create(response);

      // ACT
      var request = testClient.GraphServiceClient
                          .SharePointAPI(mockWebUrl)
                          .Web
                          .Lists[mockListId]
                          .Request()
                          .GetHttpRequestMessage();

      // ASSERT
      Assert.Equal(expectedUri, request.RequestUri);
      Assert.Equal(SharePointAPIRequestConstants.Headers.AcceptHeaderValue, request.Headers.Accept.ToString());
      Assert.True(request.Headers.Contains(SharePointAPIRequestConstants.Headers.ODataVersionHeaderName), $"Header does not contain {SharePointAPIRequestConstants.Headers.ODataVersionHeaderName} header");
      Assert.Equal(SharePointAPIRequestConstants.Headers.ODataVersionHeaderValue, string.Join(',', request.Headers.GetValues(SharePointAPIRequestConstants.Headers.ODataVersionHeaderName)));
    }

    [Fact]
    public async Task GetById_MissingId_Throws()
    {
      using (var response = new HttpResponseMessage())
      using (var gsc = GraphServiceTestClient.Create(response))
      {
        // ACT & ASSERT
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
        async () => await gsc.GraphServiceClient
                                .SharePointAPI(mockWebUrl)
                                .Web
                                .Lists[Guid.Empty]
                                .Request()
                                .GetAsync()
        );
      }
    }

    [Fact]
    public async Task GetById_ReturnsCorrectResponse()
    {
      // ARRANGE
      var mockListId = new Guid("6f094ea6-2222-4f2e-b864-54f706f8b07a");

      var responseContent = ResourceManager.GetHttpResponseContent("GetListResponse.json");
      var responseMessage = new HttpResponseMessage()
      {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent(responseContent),
      };

      using (responseMessage)
      using (GraphServiceTestClient gsc = GraphServiceTestClient.Create(responseMessage))
      {
        // ACT
        var actual = await gsc.GraphServiceClient
                                  .SharePointAPI(mockWebUrl)
                                  .Web
                                  .Lists[mockListId]
                                  .Request()
                                  .GetAsync();

        // ASSERT
        Assert.Equal("6f094ea6-2222-4f2e-b864-54f706f8b07a", actual.Id);
        Assert.Equal("Events", actual.Title);
        Assert.Equal(106, actual.BaseTemplate);
      }
    }


    [Fact]
    public void GetByTitle_GeneratesCorrectRequestUriAndHeaders()
    {
      // ARRANGE
      var mockListTitle = "mockListTitle";
      var expectedUri = new Uri($"{mockWebUrl}/_api/web/lists/getByTitle('{mockListTitle}')");

      using HttpResponseMessage response = new HttpResponseMessage();
      using GraphServiceTestClient gsc = GraphServiceTestClient.Create(response);

      // ACT
      var request = gsc.GraphServiceClient
                          .SharePointAPI(mockWebUrl)
                          .Web
                          .Lists[mockListTitle]
                          .Request()
                          .GetHttpRequestMessage();

      // ASSERT
      Assert.Equal(expectedUri, request.RequestUri);
      Assert.Equal(SharePointAPIRequestConstants.Headers.AcceptHeaderValue, request.Headers.Accept.ToString());
      Assert.True(request.Headers.Contains(SharePointAPIRequestConstants.Headers.ODataVersionHeaderName), $"Header does not contain {SharePointAPIRequestConstants.Headers.ODataVersionHeaderName} header");
      Assert.Equal(SharePointAPIRequestConstants.Headers.ODataVersionHeaderValue, string.Join(',', request.Headers.GetValues(SharePointAPIRequestConstants.Headers.ODataVersionHeaderName)));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task GetByTitle_MissingTitle_Throws(string title)
    {
      using (var response = new HttpResponseMessage())
      using (var gsc = GraphServiceTestClient.Create(response))
      {
        // ACT & ASSERT
        await Assert.ThrowsAsync<ArgumentNullException>(
        async () => await gsc.GraphServiceClient
                                .SharePointAPI(mockWebUrl)
                                .Web
                                .Lists[title]
                                .Request()
                                .GetAsync()
        );
      }
    }

    [Fact]
    public async Task GetByTitle_ReturnsCorrectResponse()
    {
      // ARRANGE
      var mockListTitle = "Events";

      var responseContent = ResourceManager.GetHttpResponseContent("GetListResponse.json");
      var responseMessage = new HttpResponseMessage()
      {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent(responseContent),
      };

      using (responseMessage)
      using (GraphServiceTestClient gsc = GraphServiceTestClient.Create(responseMessage))
      {
        // ACT
        var actual = await gsc.GraphServiceClient
                                  .SharePointAPI(mockWebUrl)
                                  .Web
                                  .Lists[mockListTitle]
                                  .Request()
                                  .GetAsync();

        // ASSERT
        Assert.Equal("6f094ea6-2222-4f2e-b864-54f706f8b07a", actual.Id);
        Assert.Equal("Events", actual.Title);
        Assert.Equal("", actual.Description);
      }
    }

    [Fact]
    public async Task GetItems_GeneratesCorrectRequestUriAndHeaders()
    {
      // ARRANGE
      var mockListId = Guid.NewGuid();
      var expectedUri = new Uri($"{mockWebUrl}/_api/web/lists('{mockListId}')/items");

      using HttpResponseMessage response = new HttpResponseMessage();
      using GraphServiceTestClient testClient = GraphServiceTestClient.Create(response);

      // ACT
      var request = testClient.GraphServiceClient
                          .SharePointAPI(mockWebUrl)
                          .Web
                          .Lists[mockListId]
                          .Items
                          .Request()
                          .GetHttpRequestMessage();

      // ASSERT
      Assert.Equal(expectedUri, request.RequestUri);
      Assert.Equal(SharePointAPIRequestConstants.Headers.AcceptHeaderValue, request.Headers.Accept.ToString());
      Assert.True(request.Headers.Contains(SharePointAPIRequestConstants.Headers.ODataVersionHeaderName), $"Header does not contain {SharePointAPIRequestConstants.Headers.ODataVersionHeaderName} header");
      Assert.Equal(SharePointAPIRequestConstants.Headers.ODataVersionHeaderValue, string.Join(',', request.Headers.GetValues(SharePointAPIRequestConstants.Headers.ODataVersionHeaderName)));
    }

    [Fact]
    public async Task GetChanges_GeneratesCorrectRequest()
    {
      // ARRANGE
      var query = new ChangeQuery()
      {
        Add = true
      };
      var mockListId = Guid.NewGuid();
      var expectedUri = new Uri($"{mockWebUrl}/_api/web/lists('{mockListId}')/GetChanges");
      var expectedContent = "{\"query\":{\"Add\":true}}";

      using HttpResponseMessage response = new HttpResponseMessage();
      using GraphServiceTestClient gsc = GraphServiceTestClient.Create(response);

      // ACT
      await gsc.GraphServiceClient
                  .SharePointAPI(mockWebUrl)
                  .Web
                  .Lists[mockListId]
                  .Request()
                  .GetChangesAsync(query);
      var actualContent = gsc.HttpProvider.ContentAsString;

      // ASSERT
      gsc.HttpProvider.Verify(
        provider => provider.SendAsync(
          It.Is<HttpRequestMessage>(req =>
            req.Method == HttpMethod.Post &&
            req.RequestUri == expectedUri &&
            req.Headers.Authorization != null
          ),
          It.IsAny<HttpCompletionOption>(),
          It.IsAny<CancellationToken>()
        ),
        Times.Exactly(1)
      );

      Assert.Equal(Microsoft.Graph.CoreConstants.MimeTypeNames.Application.Json, gsc.HttpProvider.ContentHeaders.ContentType.MediaType);
      Assert.Equal(expectedContent, actualContent);
    }
  }
}
