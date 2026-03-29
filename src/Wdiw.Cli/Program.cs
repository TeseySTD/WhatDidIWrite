using System.CommandLine;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using RazorConsole.Core;
using Wdiw.Infrastructure;
using Wdiw.Tui;

var rootCommand = new RootCommand("Wdiw (What Did I Write) - AI-powered commit message generator.");
var configCommand = new Command("config", "Open the TUI configuration menu to setup API keys and styles.");

rootCommand.SetAction(async (_, _) => await StartTuiApp("/commit"));
configCommand.SetAction(async (_, _) => await StartTuiApp("/config"));

rootCommand.Add(configCommand); 

ParseResult parseResult = rootCommand.Parse(args);
return await parseResult.InvokeAsync();


async Task StartTuiApp(string initialRoute)
{
    var builder = Host.CreateApplicationBuilder();

    builder.AddTui(services =>
    {
        services.AddInfrastructure();
    });

    var host = builder.Build();

    var navManager = host.Services.GetRequiredService<NavigationManager>();
    navManager.NavigateTo(initialRoute);

    await host.RunAsync();
}