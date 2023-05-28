using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;

namespace Vita.Identity.Host;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.Title = "IdentityServer";

        BuildWebHostBuilder(args).Build().Run();
    }

    public static IHostBuilder BuildWebHostBuilder(string[] args)
    {
        return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
            .UseSerilog((ctx, config) =>
            {
                config.MinimumLevel.Debug()
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                    .Enrich.FromLogContext();

                if (ctx.HostingEnvironment.IsDevelopment())
                    config.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}");
            })
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
            .ConfigureAppConfiguration((_, config) =>
            {
                Uri keyVaultUri = new(Environment.GetEnvironmentVariable("VitaKeyVaultUri"));
                SecretClient secretClient = new(keyVaultUri, new DefaultAzureCredential());

                config.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
            });
    }
}
