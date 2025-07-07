using System.ComponentModel.DataAnnotations;

namespace TheBackendCmsSolution.Modules.Users.Models;

public class User
{
    public Guid Id { get; set; }
    [Required]
    public string UserName { get; set; } = string.Empty;
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
}
