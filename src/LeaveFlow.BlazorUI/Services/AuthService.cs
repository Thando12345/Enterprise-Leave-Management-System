using System.IdentityModel.Tokens.Jwt;
using Blazored.LocalStorage;
using LeaveFlow.Application.DTOs;

namespace LeaveFlow.BlazorUI.Services;

public class AuthService(ILocalStorageService storage)
{
    private const string TokenKey = "lf_token";
    private const string UserKey  = "lf_user";

    public async Task LoginAsync(LoginResponse response)
    {
        await storage.SetItemAsync(TokenKey, response.AccessToken);
        await storage.SetItemAsync(UserKey, response);
    }

    public async Task LogoutAsync()
    {
        await storage.RemoveItemAsync(TokenKey);
        await storage.RemoveItemAsync(UserKey);
    }

    public async Task<string?> GetTokenAsync() =>
        await storage.GetItemAsync<string>(TokenKey);

    public async Task<LoginResponse?> GetUserAsync() =>
        await storage.GetItemAsync<LoginResponse>(UserKey);

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token)) return false;
        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return jwt.ValidTo > DateTime.UtcNow;
        }
        catch { return false; }
    }

    public async Task<string> GetRoleAsync()
    {
        var user = await GetUserAsync();
        return user?.Role ?? "Employee";
    }

    public async Task<string> GetFullNameAsync()
    {
        var user = await GetUserAsync();
        return user?.FullName ?? "User";
    }
}
