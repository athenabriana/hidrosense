using System.ComponentModel.DataAnnotations;

namespace HidroSense.Api.Models;

public class AppUser
{
    public int Id { get; set; }

    [MaxLength(80)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(40)]
    public string Role { get; set; } = "Operator";

    public DateTime CreatedAt { get; set; }
}
