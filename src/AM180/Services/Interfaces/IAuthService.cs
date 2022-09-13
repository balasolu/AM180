using AM180.Models.Abstractions;
using Microsoft.AspNetCore.Components.Authorization;

namespace AM180.Services.Interfaces;

/// <summary>
/// 
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    Task<AuthenticationState> BuildAuthenticationStateAsync(string email);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    Task<AuthenticationState> BuildAuthenticationStateAsync(User user);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<bool> IsTokenExpiredAsync(Token token);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<bool> IsTokenInspiredAsync(Token token);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<bool> IsTokenValidAsync(Token token);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    Task<bool> PasswordAuthenticateAsync(string email, string password);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    Task RenewAuthenticationTokenAsync(User user);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    Task RenewRefreshTokenAsync(User user);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    Task<bool> TokenAuthenticateAsync(User user);

    /// <summary>
    /// checks to see if a given token expires soon
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<bool> TokenExpiresSoonAsync(Token token);
}
