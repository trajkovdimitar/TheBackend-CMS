using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using TheBackendCmsSolution.Web;
using Xunit;

public class ContentApiClientTests
{
    [Fact]
    public async Task GetContentTypesAsync_ReturnsEmptyList_OnNotFound()
    {
        var handler = new StubHandler(new HttpResponseMessage(HttpStatusCode.NotFound));
        var httpClient = new HttpClient(handler) { BaseAddress = new System.Uri("http://localhost") };
        var api = new ContentApiClient(httpClient);

        var result = await api.GetContentTypesAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateContentTypeAsync_PostsDtoToCorrectEndpoint()
    {
        var response = new HttpResponseMessage(HttpStatusCode.Created);
        var handler = new StubHandler(response);
        var httpClient = new HttpClient(handler) { BaseAddress = new System.Uri("http://localhost") };
        var api = new ContentApiClient(httpClient);

        var dto = new ContentTypeDto("blog", "Blog", []);
        await api.CreateContentTypeAsync(dto);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal("/content-types", handler.LastRequest?.RequestUri?.AbsolutePath);
        var sentDto = await handler.LastRequest!.Content!.ReadFromJsonAsync<ContentTypeDto>();
        Assert.Equal(dto.Name, sentDto!.Name);
    }

    private class StubHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;
        public HttpRequestMessage? LastRequest { get; private set; }
        public StubHandler(HttpResponseMessage response) => _response = response;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(_response);
        }
    }
}
