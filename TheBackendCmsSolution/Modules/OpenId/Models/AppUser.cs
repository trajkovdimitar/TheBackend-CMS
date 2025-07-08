using System.ComponentModel.DataAnnotations;

namespace TheBackendCmsSolution.Modules.OpenId.Models;

public class AppUser
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;
}
