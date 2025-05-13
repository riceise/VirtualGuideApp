using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Guide.Ui.Components.Services;

public class AuthenticationService : AuthenticationStateProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly NavigationManager _navigationManager;
    private static string _token = string.Empty;

    public AuthenticationService(IHttpClientFactory httpClientFactory, NavigationManager navigationManager)
    {
        _httpClientFactory = httpClientFactory;
        _navigationManager = navigationManager;
    }


    public async Task<bool> Login(string username, string password)
    {
        var _httpClient = _httpClientFactory.CreateClient("BackendAPI");
        var response =
            await _httpClient.PostAsJsonAsync("api/Auth/login", new { UserName = username, Password = password });

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResult>();
            _token = result.Token;

            Console.WriteLine($"Получен токен: {_token}");

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return true;
        }

        return false;
    }

    public void Logout()
    {
        _token = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        _navigationManager.NavigateTo("/");
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (string.IsNullOrEmpty(_token))
        {
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }

        var claims = ParseClaimsFromJwt(_token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        return Task.FromResult(new AuthenticationState(user));
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1]; 
        var jsonBytes = ParseBase64WithoutPadding(payload); 
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        if (keyValuePairs != null)
        {
            foreach (var kvp in keyValuePairs)
            {
                var key = kvp.Key;
                var value = kvp.Value?.ToString(); 

                if (value != null)
                {
                   if (key == ClaimTypes.Role || key == "role") 
                    {
                        if (kvp.Value is JsonElement element && element.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var roleElement in element.EnumerateArray())
                            {
                                var roleValue = roleElement.GetString();
                                if (!string.IsNullOrEmpty(roleValue))
                                {
                                    claims.Add(new Claim(ClaimTypes.Role, roleValue));
                                    Console.WriteLine(
                                        $"[ParseClaims] Added Role Claim: {ClaimTypes.Role} = {roleValue}");
                                }
                            }
                        }
                        else
                        {
                            claims.Add(new Claim(ClaimTypes.Role, value));
                            Console.WriteLine(
                                $"[ParseClaims] Added Role Claim: {ClaimTypes.Role} = {value}"); 
                        }
                    }
                   else if
                        (key == ClaimTypes.NameIdentifier ||
                         key == "nameid") 
                    {
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, value));
                        Console.WriteLine(
                            $"[ParseClaims] Added NameIdentifier Claim: {ClaimTypes.NameIdentifier} = {value}"); // Логирование
                    }
                    else if (key == ClaimTypes.Name || key == "unique_name") 
                    {
                        claims.Add(new Claim(ClaimTypes.Name, value));
                        Console.WriteLine(
                            $"[ParseClaims] Added Name Claim: {ClaimTypes.Name} = {value}"); 
                    }
                   
                    else
                    {
                        
                        if (!new[] { "exp", "iss", "aud", "nbf", "iat", "jti" }.Contains(key.ToLowerInvariant()))
                        {
                            claims.Add(new Claim(key, value));
                            Console.WriteLine($"[ParseClaims] Added Generic Claim: {key} = {value}"); 
                        }
                        else
                        {
                            Console.WriteLine($"[ParseClaims] Skipping standard JWT claim: {key}"); 
                        }
                    }
                }
            }
        }
        else
        {
            Console.WriteLine("[ParseClaims] Failed to deserialize JWT payload."); 
        }


        Console.WriteLine($"[ParseClaims] Total claims parsed: {claims.Count}");
        foreach (var claim in claims)
        {
            Console.WriteLine($"[ParseClaims] --> Type: {claim.Type}, Value: {claim.Value}");
        }

        return claims;
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2:
                base64 += "==";
                break;
            case 3:
                base64 += "=";
                break;
        }

        return Convert.FromBase64String(base64);
    }

    public string GetCurrentToken()
    {
        Console.WriteLine($"Возвращаемый токен: {_token}");
        return _token;
    }
}

public class LoginResult
{
    public string Token { get; set; }
    public DateTime Expiration { get; set; }
}