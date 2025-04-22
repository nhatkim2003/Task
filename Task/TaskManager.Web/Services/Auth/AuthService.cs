using Blazored.LocalStorage;
using TaskManager.Web.Models;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

namespace TaskManager.Web.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly CustomAuthStateProvider _authStateProvider;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            HttpClient httpClient, 
            ILocalStorageService localStorage, 
            AuthenticationStateProvider authProvider,
            ILogger<AuthService> logger)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authStateProvider = (CustomAuthStateProvider)authProvider;
            _logger = logger;
        }

        public async Task<AuthResult> LoginAsync(LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Attempting to login user: {Username}", request.Username);
                
                var response = await _httpClient.PostAsJsonAsync("api/Auth/login", request);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("Login response status: {StatusCode}, Content: {Content}", 
                    response.StatusCode, responseContent);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Login failed with status code {StatusCode}: {Content}", 
                        response.StatusCode, responseContent);
                    
                    return new AuthResult 
                    { 
                        Succeeded = false, 
                        Error = $"Đăng nhập thất bại: {response.StatusCode} - {responseContent}" 
                    };
                }

                var result = await response.Content.ReadFromJsonAsync<AuthResult>();
                
                if (result is not null && result.Succeeded)
                {
                    _logger.LogInformation("Login successful for user: {Username}", request.Username);
                    await _localStorage.SetItemAsync("authToken", result.Token);
                    _authStateProvider.NotifyUserAuthentication(result.Token);
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", result.Token);
                }
                else
                {
                    _logger.LogWarning("Login failed for user {Username}: {Error}", 
                        request.Username, result?.Error ?? "Unknown error");
                }

                return result ?? new AuthResult 
                { 
                    Succeeded = false, 
                    Error = "Không thể xử lý phản hồi từ máy chủ" 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", request.Username);
                return new AuthResult 
                { 
                    Succeeded = false, 
                    Error = $"Lỗi khi đăng nhập: {ex.Message}" 
                };
            }
        }

        public async Task<AuthResult> RegisterAsync(LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Attempting to register user: {Username}", request.Username);
                
                var response = await _httpClient.PostAsJsonAsync("api/Auth/register", request);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("Register response status: {StatusCode}, Content: {Content}", 
                    response.StatusCode, responseContent);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Registration failed with status code {StatusCode}: {Content}", 
                        response.StatusCode, responseContent);
                    
                    return new AuthResult 
                    { 
                        Succeeded = false, 
                        Error = $"Đăng ký thất bại: {response.StatusCode} - {responseContent}" 
                    };
                }

                var result = await response.Content.ReadFromJsonAsync<AuthResult>();
                
                if (result?.Succeeded == true)
                {
                    _logger.LogInformation("Registration successful for user: {Username}", request.Username);
                }
                else
                {
                    _logger.LogWarning("Registration failed for user {Username}: {Error}", 
                        request.Username, result?.Error ?? "Unknown error");
                }

                return result ?? new AuthResult 
                { 
                    Succeeded = false, 
                    Error = "Không thể xử lý phản hồi từ máy chủ" 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user {Username}", request.Username);
                return new AuthResult 
                { 
                    Succeeded = false, 
                    Error = $"Lỗi khi đăng ký: {ex.Message}" 
                };
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                _logger.LogInformation("Logging out user");
                await _localStorage.RemoveItemAsync("authToken");
                _authStateProvider.NotifyUserLogout();
                _httpClient.DefaultRequestHeaders.Authorization = null;
                _logger.LogInformation("User logged out successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
            }
        }
    }
}
