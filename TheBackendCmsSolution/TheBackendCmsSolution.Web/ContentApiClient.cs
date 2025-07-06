using System.Net.Http.Json;

namespace TheBackendCmsSolution.Web;

public class ContentApiClient(HttpClient httpClient)
{
    public async Task<List<ContentTypeResponse>> GetContentTypesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await httpClient.GetFromJsonAsync<List<ContentTypeResponse>>("/content-types", cancellationToken);
            return result ?? [];
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return [];
        }
    }

    public async Task<ContentTypeResponse?> GetContentTypeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<ContentTypeResponse>($"/content-types/{id}", cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task CreateContentTypeAsync(ContentTypeDto dto, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/content-types", dto, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateContentTypeAsync(Guid id, ContentTypeDto dto, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync($"/content-types/{id}", dto, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteContentTypeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"/content-types/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}

public record ContentTypeDto(string Name, string DisplayName, Dictionary<string, object> Fields);
public record ContentTypeResponse(Guid Id, string Name, string DisplayName, Dictionary<string, object> Fields);
