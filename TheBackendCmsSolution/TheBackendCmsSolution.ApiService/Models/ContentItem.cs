namespace TheBackendCmsSolution.ApiService.Models;

public class ContentItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Type { get; set; } = "generic";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ContentType ContentType { get; set; }
}