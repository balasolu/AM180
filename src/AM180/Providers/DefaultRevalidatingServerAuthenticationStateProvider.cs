using AM180.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Serilog;

namespace AM180.Providers;

public sealed class DefaultRevalidatingServerAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
{
    readonly IAuthService _authService;
    readonly ILocalStorageService _localStorageService;

    public DefaultRevalidatingServerAuthenticationStateProvider(
        IAuthService authService,
        ILocalStorageService localStorageService,
        ILoggerFactory loggerFactory)
            : base(loggerFactory)
    {
        _authService = authService;
        _localStorageService = localStorageService;
    }

    protected override TimeSpan RevalidationInterval =>
        TimeSpan.FromSeconds(5);

    public async override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        await base.GetAuthenticationStateAsync();

    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _authService.TokenAuthenticateAsync(await _localStorageService.GetDefaultUserAsync());
        }
        catch (Exception e)
        {
            Log.Warning(e.Message, e);
            return false;
        }
    }
}
