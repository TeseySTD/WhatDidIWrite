using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RazorConsole.Core;

namespace Wdiw.Tui;

public static class DependencyInjection
{
    public static HostApplicationBuilder AddTui(this HostApplicationBuilder builder, Action<IServiceCollection> configureServices)
    {
        builder.UseRazorConsole<App>(configure: config =>
            {
                config.Services.Configure<ConsoleAppOptions>(opt => { opt.EnableTerminalResizing = true; });
                configureServices(config.Services);
            }
        );
        return builder;
    }
}