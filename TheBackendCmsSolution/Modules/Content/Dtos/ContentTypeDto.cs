using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TheBackendCmsSolution.Modules.Content.Dtos
{
    public class ContentTypeDto
    {
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string DisplayName { get; set; } = string.Empty;

        public Dictionary<string, object> Fields { get; set; } = new();
    }
}
