namespace TheBackendCmsSolution.ApiService.Models;

public class ContentType
{
    public string Name { get; set; } = string.Empty; 
    public string DisplayName { get; set; } = string.Empty; 
    public ICollection<ContentItem> ContentItems { get; set; } = new List<ContentItem>();
}