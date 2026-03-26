using Microsoft.Extensions.DependencyInjection;
using Wdiw.Infrastructure.Abstractions;
using Wdiw.Infrastructure.Persistence;
using Wdiw.Infrastructure.Services;

namespace Wdiw.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IGitService, GitCliService>();
        services.AddSingleton<IConfigRepository, JsonConfigRepository>();
        return services;
    }
}