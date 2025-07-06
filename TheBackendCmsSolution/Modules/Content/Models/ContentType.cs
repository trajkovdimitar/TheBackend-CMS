using System;
using System.Collections.Generic;

namespace TheBackendCmsSolution.Modules.Content.Models
{
    public class ContentType
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public Dictionary<string, object> Fields { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}
