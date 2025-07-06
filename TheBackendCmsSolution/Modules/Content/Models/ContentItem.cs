using System;

namespace TheBackendCmsSolution.Modules.Content.Models
{
    public class ContentItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public Guid ContentTypeId { get; set; }
        public ContentType ContentType { get; set; } = null!;
        public Dictionary<string, object> Fields { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
