namespace TaskManager.Web.Models;

public class AuthResult
{
    public bool Succeeded { get; set; }
    public string? Token { get; set; }
    public string? Error { get; set; }
} 