using System.Net.Http.Json;

namespace TheBackendCmsSolution.Web;

public class ContentApiClient(HttpClient httpClient)
{
    public async Task<List<ContentTypeResponse>> GetContentTypesAsync(CancellationToken cancellationToken = default)
    {
        var result = await httpClient.GetFromJsonAsync<List<ContentTypeResponse>>("/content-types", cancellationToken);
        return result ?? [];
    }

    public async Task<ContentTypeResponse?> GetContentTypeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<ContentTypeResponse>($"/content-types/{id}", cancellationToken);
    }

    public async Task CreateContentTypeAsync(ContentTypeDto dto, CancellationToken cancellationToken = default)
    {
        await httpClient.PostAsJsonAsync("/content-types", dto, cancellationToken);
    }

    public async Task UpdateContentTypeAsync(Guid id, ContentTypeDto dto, CancellationToken cancellationToken = default)
    {
        await httpClient.PutAsJsonAsync($"/content-types/{id}", dto, cancellationToken);
    }
}

public record ContentTypeDto(string Name, string DisplayName, Dictionary<string, object> Fields);
public record ContentTypeResponse(Guid Id, string Name, string DisplayName, Dictionary<string, object> Fields);
