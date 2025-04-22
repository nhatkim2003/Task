using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Models;

public class LoginRequest
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    public string Password { get; set; } = string.Empty;
} 