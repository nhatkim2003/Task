using TaskManager.Web.Models;
using System.Threading.Tasks;

namespace TaskManager.Web.Services.Auth
{
    public interface IAuthService
    {
        Task<AuthResult> LoginAsync(LoginRequest request);
        Task<AuthResult> RegisterAsync(LoginRequest request); // <- Thêm dòng này
        Task LogoutAsync();
    }
}
