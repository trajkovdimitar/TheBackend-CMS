using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TheBackendCmsSolution.ApiService.Dtos
{
    public class ContentItemDto
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public Guid ContentTypeId { get; set; }

        public Dictionary<string, object> Fields { get; set; } = new();
    }
}