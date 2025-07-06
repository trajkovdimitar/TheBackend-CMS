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

    public async Task<List<ContentItemResponse>> GetContentItemsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await httpClient.GetFromJsonAsync<List<ContentItemResponse>>("/content", cancellationToken);
            return result ?? [];
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return [];
        }
    }

    public async Task<ContentItemResponse?> GetContentItemAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<ContentItemResponse>($"/content/{id}", cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task CreateContentItemAsync(ContentItemDto dto, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/content", dto, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateContentItemAsync(Guid id, ContentItemDto dto, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync($"/content/{id}", dto, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteContentItemAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"/content/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}

public record ContentTypeDto(string Name, string DisplayName, Dictionary<string, object> Fields);
public record ContentTypeResponse(Guid Id, string Name, string DisplayName, Dictionary<string, object> Fields);
public record ContentTypeSummary(string Name, string DisplayName);
public record ContentItemDto(string Title, Guid ContentTypeId, Dictionary<string, object> Fields);
public record ContentItemResponse(Guid Id, string Title, Guid ContentTypeId, ContentTypeSummary ContentType, Dictionary<string, object> Fields, DateTime CreatedAt, DateTime? UpdatedAt);
