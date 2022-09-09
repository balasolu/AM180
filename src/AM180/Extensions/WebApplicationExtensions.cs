using AM180.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AM180.Extensions;

public static class WebApplicationExtensions
{
    public static async Task<WebApplication> MigrateDefaultDbContextAsync(this WebApplication webApplication)
    {
        using var scope = webApplication.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();
        await context.Database.MigrateAsync();
        return webApplication;
    }
}

