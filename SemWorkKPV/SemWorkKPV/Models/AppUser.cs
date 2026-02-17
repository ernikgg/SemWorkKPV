namespace SemWorkKPV.Models;

public class AppUser
{
    public int Id { get; set; }

    public string UserName { get; set; } = "";

    public string PasswordHash { get; set; } = "";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

