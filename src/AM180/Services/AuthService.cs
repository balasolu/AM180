using AM180.Contexts;
using AM180.Models.Abstractions;
using AM180.Models.Enums;
using AM180.Models.Tokens;
using AM180.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;

namespace AM180.Services;

/// <inheritdoc cref="IAuthService" />
sealed class AuthService : IAuthService
{
    readonly IDbContextFactory<DefaultDbContext> _defaultDbContextFactory;
    readonly SignInManager<User> _signInManager;
    readonly UserManager<User> _userManager;
    readonly ILocalStorageService _localStorageService;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="defaultDbContextFactory"></param>
    /// <param name="signInManager"></param>
    /// <param name="userManager"></param>
    /// <param name="localStorageService"></param>
    public AuthService(
        IDbContextFactory<DefaultDbContext> defaultDbContextFactory,
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        ILocalStorageService localStorageService)
    {
        _defaultDbContextFactory = defaultDbContextFactory;
        _signInManager = signInManager;
        _userManager = userManager;
        _localStorageService = localStorageService;
    }

    /// <inheritdoc cref="IAuthService.BuildAuthenticationStateAsync(string)" />
    public async Task<AuthenticationState> BuildAuthenticationStateAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user != null)
        {
            var principal = await _signInManager.CreateUserPrincipalAsync(user);
            var identity = new ClaimsIdentity(principal.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
            principal = new ClaimsPrincipal(identity);
            return new AuthenticationState(principal);
        }
        return new AuthenticationState(new ClaimsPrincipal());
    }

    /// <inheritdoc cref="IAuthService.BuildAuthenticationStateAsync(User)" />
    public async Task<AuthenticationState> BuildAuthenticationStateAsync(User user)
    {
        var principal = await _signInManager.CreateUserPrincipalAsync(user);
        var identity = new ClaimsIdentity(principal.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
        principal = new ClaimsPrincipal(identity);
        return new AuthenticationState(principal);
    }

    async Task<Token> GetAuthenticationTokenAsync(User user)
    {
        Log.Information("creating default db context");
        await using var context = await _defaultDbContextFactory.CreateDbContextAsync();
        if (context.Users != null && context.Tokens != null)
        {
            Log.Information("finding user including tokens");
            var userContext = await context.Users.Include(x => x.Tokens).FirstOrDefaultAsync(x => x.Id == user.Id);
            if (userContext != null && userContext.Tokens != null)
            {
                Log.Information("finding authentication token");
                var token = userContext.Tokens.FirstOrDefault(x => x.TokenType == TokenType.Authentication);
                if (token != null)
                    return token;
                Log.Warning("authentication token was null");
                token = GenerateAuthenticationToken(user);
                await context.Tokens.AddAsync(token);
                await context.SaveChangesAsync();
                return token;
            }
        }
        return new AuthenticationToken();
    }

    async Task<Token> GetRefreshTokenAsync(User user)
    {
        Log.Information("creating default db context");
        await using var context = await _defaultDbContextFactory.CreateDbContextAsync();
        if (context.Users != null && context.Tokens != null)
        {
            Log.Information("finding user including tokens");
            var userContext = await context.Users.Include(x => x.Tokens).FirstOrDefaultAsync(x => x.Id == user.Id);
            if (userContext != null && userContext.Tokens != null)
            {
                Log.Information("finding refresh token");
                var token = userContext.Tokens.FirstOrDefault(x => x.TokenType == TokenType.Refresh);
                if (token != null)
                    return token;
                Log.Warning("refresh token was null");
                token = GenerateRefreshToken(user);
                await context.Tokens.AddAsync(token);
                await context.SaveChangesAsync();
                return token;
            }
        }
        return new RefreshToken();
    }

    async Task<Token> GetConfirmationTokenAsync(User user)
    {
        await using var context = await _defaultDbContextFactory.CreateDbContextAsync();
        if (context.Users != null)
        {
            var userContext = await context.Users.Include(x => x.Tokens).FirstOrDefaultAsync(x => x.Id == user.Id);
            if (userContext != null && userContext.Tokens != null)
            {
                var token = userContext.Tokens.FirstOrDefault(x => x.TokenType == TokenType.Confirmation);
                if (token != null)
                    return token;
            }
        }
        return new ConfirmationToken();
    }

    /// <inheritdoc cref="IAuthService.PasswordAuthenticateAsync(string, string)" />
    public async Task<bool> PasswordAuthenticateAsync(string email, string password)
    {
        Log.Information("finding user by email");
        var user = await _userManager.FindByEmailAsync(email);
        if (user != null)
        {
            Log.Information("deleting default user from local store");
            await _localStorageService.DeleteDefaultUserAsync();
            if (await _userManager.CheckPasswordAsync(user, password))
            {
                Log.Information("getting authentication token");
                var authenticationToken = await GetAuthenticationTokenAsync(user);
                if (authenticationToken != null)
                {
                    Log.Information("checking if authentication token requires renewal");
                    if (await TokenExpiresSoonAsync(authenticationToken))
                    {
                        Log.Information("renewing authentication token");
                        await UpsertTokenAsync(user, GenerateAuthenticationToken(user));
                    }
                }
                else
                    await UpsertTokenAsync(user, GenerateAuthenticationToken(user));
                Log.Information("getting refresh token");
                var refreshToken = await GetRefreshTokenAsync(user);
                if (refreshToken != null)
                {
                    Log.Information("checking if refresh token requires renewal");
                    if (await TokenExpiresSoonAsync(refreshToken))
                    {
                        Log.Information("renewing refresh token");
                        await UpsertTokenAsync(user, GenerateRefreshToken(user));
                    }
                }
                else
                    await UpsertTokenAsync(user, GenerateRefreshToken(user));
                Log.Information("creating default db context");
                await using var context = await _defaultDbContextFactory.CreateDbContextAsync();
                if (context.Users != null)
                {
                    Log.Information("finding user including tokens");
                    var userContext = await context.Users.Include(x => x.Tokens).FirstOrDefaultAsync(x => x.Id == user.Id);
                    if (userContext != null)
                    {
                        Log.Information("setting default user to local store");
                        await _localStorageService.SetDefaultUserAsync(userContext);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <inheritdoc cref="IAuthService.TokenExpiresSoonAsync(Token)" />
    public async Task<bool> TokenExpiresSoonAsync(Token token)
    {
        await using var context = await _defaultDbContextFactory.CreateDbContextAsync();
        if (context.Tokens != null)
        {
            var tokenContext = await context.Tokens.FirstOrDefaultAsync(x => x.Id == token.Id);
            if (tokenContext != null)
            {
                var soon = DateTimeOffset.UtcNow.AddHours(-2.5f).ToUnixTimeSeconds();
                var expired = soon > tokenContext.Expiration;
                return expired;
            }    
        }
        return false;
    }

    /// <inheritdoc cref="IAuthService.IsTokenExpiredAsync(Token)" />
    public async Task<bool> IsTokenExpiredAsync(Token token)
    {
        await using var context = await _defaultDbContextFactory.CreateDbContextAsync();
        if (context.Tokens != null)
        {
            var tokenContext = await context.Tokens.FirstOrDefaultAsync(x => x.Id == token.Id);
            if (tokenContext != null)
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var expired = now > tokenContext.Expiration;
                return expired;
            }
        }
        return true;
    }

    /// <inheritdoc cref="IAuthService.IsTokenInspiredAsync(Token)" />
    public async Task<bool> IsTokenInspiredAsync(Token token)
    {
        await using var context = await _defaultDbContextFactory.CreateDbContextAsync();
        if (context.Tokens != null)
        {
            var tokenContext = await context.Tokens.FirstOrDefaultAsync(x => x.Id == token.Id);
            if (tokenContext != null)
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var inspired = now < tokenContext.Expiration;
                return inspired;
            }
        }
        return false;
    }

    /// <inheritdoc cref="IAuthService.IsTokenValidAsync(Token)" />
    public async Task<bool> IsTokenValidAsync(Token token)
    {
        await using var context = await _defaultDbContextFactory.CreateDbContextAsync();
        if (context.Tokens != null)
        {
            var tokenContext = await context.Tokens.FirstOrDefaultAsync(x => x.Id == token.Id);
            if (tokenContext != null)
            {
                if (token.Hash == tokenContext.Hash)
                    return true;
            }
        }
        return false;
    }

    /// <inheritdoc cref="IAuthService.RenewAuthenticationTokenAsync(User)" />
    public async Task RenewAuthenticationTokenAsync(User user)
    {
        await UpsertTokenAsync(user, GenerateAuthenticationToken(user));
        await using var context = await _defaultDbContextFactory.CreateDbContextAsync();
        if (context.Users != null)
        {
            var userContext = await context.Users.Include(x => x.Tokens).FirstOrDefaultAsync(x => x.Id == user.Id);
            if (userContext != null)
                await _localStorageService.SetDefaultUserAsync(userContext);
        }
    }

    /// <inheritdoc cref="IAuthService.RenewRefreshTokenAsync(User)" />
    public async Task RenewRefreshTokenAsync(User user)
    {
        await UpsertTokenAsync(user, GenerateRefreshToken(user));
        await using var context = await _defaultDbContextFactory.CreateDbContextAsync();
        if (context.Users != null)
        {
            var userContext = await context.Users.Include(x => x.Tokens).FirstOrDefaultAsync(x => x.Id == user.Id);
            if (userContext != null)
                await _localStorageService.SetDefaultUserAsync(userContext);
        }
    }

    /// <inheritdoc cref="IAuthService.TokenAuthenticateAsync(Token, Token)" />
    public async Task<bool> TokenAuthenticateAsync(User user)
    {
        if (user.Tokens != null)
        {
            var authenticationToken = user.Tokens.FirstOrDefault(x => x.TokenType == TokenType.Authentication);
            var refreshToken = user.Tokens.FirstOrDefault(x => x.TokenType == TokenType.Refresh);
            if (authenticationToken != null && refreshToken != null)
            {
                Log.Information("checking for tokens inspiration");
                if (await IsTokenInspiredAsync(authenticationToken))
                    return await IsTokenValidAsync(authenticationToken);
                else if (await IsTokenInspiredAsync(refreshToken))
                {
                    var valid = await IsTokenValidAsync(refreshToken);
                    if (valid)
                        await RenewAuthenticationTokenAsync(user);
                    await RenewRefreshTokenAsync(user);
                    return valid;
                }
            }
        }
        Log.Information("tokens were null");
        return false;
    }

    async Task UpsertTokenAsync(
        User user,
        Token token)
    {
        Log.Information("creating default db context");
        await using var context = await _defaultDbContextFactory.CreateDbContextAsync();
        if (context.Users != null && context.Tokens != null)
        {
            Log.Information("finding user including tokens");
            var userContext = await context.Users.Include(x => x.Tokens).FirstOrDefaultAsync(x => x.Id == user.Id);
            if (userContext != null && userContext.Tokens != null)
            {
                Log.Information("finding existing token");
                var existingToken = userContext.Tokens.FirstOrDefault(x => x.TokenType == token.TokenType);
                if (existingToken != null)
                {
                    Log.Information("removing existing token");
                    context.Tokens.Remove(existingToken);
                }
                Log.Information("adding token");
                await context.Tokens.AddAsync(token);
                await context.SaveChangesAsync();
            }
        }
    }

    public static Token GenerateAuthenticationToken(User user) =>
        new AuthenticationToken()
        {
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            TokenType = TokenType.Authentication,
            Expiration = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds(),
            Hash = Guid.NewGuid().ToString(),
            UserForeignKey = user.Id
        };

    public static Token GenerateRefreshToken(User user) =>
        new RefreshToken()
        {
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            TokenType = TokenType.Refresh,
            Expiration = DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds(),
            Hash = Guid.NewGuid().ToString(),
            UserForeignKey = user.Id
        };

    public static Token GenerateConfirmationToken(User user) =>
        new ConfirmationToken()
        {
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            TokenType = TokenType.Confirmation,
            Expiration = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds(),
            Hash = Guid.NewGuid().ToString(),
            UserForeignKey = user.Id
        };

    public static Token GenerateDefaultToken(TokenType tokenType, int days) =>
        new DefaultToken()
        {
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            TokenType = tokenType,
            Hash = Guid.NewGuid().ToString(),
            Expiration = DateTimeOffset.UtcNow.AddDays(days).ToUnixTimeSeconds()
        };
}
