using System.ComponentModel.DataAnnotations;

namespace TheBackendCmsSolution.Modules.Taxonomy.Models;

public class TaxonomyTerm
{
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string Slug { get; set; } = string.Empty;
    public string? Type { get; set; }
    public DateTime CreatedAt { get; set; }
}
