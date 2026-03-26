using Microsoft.Extensions.Hosting;
using RazorConsole.Core;
using WhatDidIWrite;

IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args)
    .UseRazorConsole<App>();
IHost host = hostBuilder.Build();
await host.RunAsync();