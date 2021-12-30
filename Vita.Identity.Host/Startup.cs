using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using IdentityServer4.Models;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Vita.Identity.Application.Commands.Users;
using Vita.Identity.Application.Query.Users;
using Vita.Identity.Domain.Aggregates.Users;
using Vita.Identity.Domain.Services.Authentication;
using Vita.Identity.Domain.Services.Passwords;
using Vita.Identity.Host.Claims;
using Vita.Identity.Infrastructure.Sql;
using Vita.Identity.Infrastructure.Sql.Aggregates.Users;
using Vita.Identity.Infrastructure.Sql.Configuration;
using Vita.Identity.Infrastructure.Sql.QueryStore;

namespace Vita.Identity.Host
{
    public class Startup
    {
        private const string ApiCorsPolicy = "api";

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews()
                    .AddRazorRuntimeCompilation();

            services.AddSameSiteCookiePolicy();

            Client[] clients = Configuration.GetSection("IdentityServer:Clients").Get<Client[]>();

            services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseSuccessEvents = true;
            }).AddInMemoryApiScopes(Config.GetApiScopes())
              .AddInMemoryIdentityResources(Config.GetIdentityResources())
              .AddInMemoryClients(clients)
              .AddProfileService<ProfileService>()
              .AddSigningCredential(GenerateCertFromAsym());

            var allowedOrigins = clients.SelectMany(x => x.RedirectUris).Select(uri => ExtractUri(uri)).ToList();

            services.AddCors(options =>
            {
                options.AddPolicy(ApiCorsPolicy, policy =>
                {
                    policy.WithOrigins(allowedOrigins.ToArray()).AllowAnyHeader().AllowAnyMethod();
                });
            });

            AddApplicationBootstrapping(services);
            AddPersistanceBootstrapping(services);

            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);
        }

        private string ExtractUri(string uriString)
        {
            var uri = new Uri(uriString);
            return $"{uri.Scheme}://{uri.Authority}";
        }

        private X509Certificate2 GenerateCertFromAsym()
        {
            var secretClient = new SecretClient(new Uri(Configuration["KeyVault:BaseUrl"]), new DefaultAzureCredential());
            Response<KeyVaultSecret> secret = secretClient.GetSecret("SignInCredentialsCert");

            return new X509Certificate2(Convert.FromBase64String(secret.Value.Value), (string)null, X509KeyStorageFlags.MachineKeySet |
                                                                                                    X509KeyStorageFlags.PersistKeySet |
                                                                                                    X509KeyStorageFlags.Exportable);
        }

        private void AddApplicationBootstrapping(IServiceCollection services)
        {
            services.AddMediatR(typeof(CreateUserCommand), typeof(CreateUserCommandHandler));
            services.AddSingleton<IConnectionStringProvider>(new ConnectionStringProvider(Configuration.GetConnectionString("VitaIdentityDbContext")));
        }

        private void AddPersistanceBootstrapping(IServiceCollection services)
        {
            services.AddScoped<IUsersRepository, UsersRepository>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<IUsersQueryStore, UserQueryStore>();
            services.AddDbContext<IdentityDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("VitaIdentityDbContext")));
        }


        public void Configure(IApplicationBuilder app)
        {
            app.UseSerilogRequestLogging();
            app.UseDeveloperExceptionPage();

            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors(policyName: ApiCorsPolicy);

            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });

            AutoMigrateDB(app);
        }

        public void AutoMigrateDB(IApplicationBuilder app)
        {
            if (Configuration["AutoMigrateDB"] == null || !bool.Parse(Configuration["AutoMigrateDB"]))
                return;

            using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var context = serviceScope.ServiceProvider.GetService<IdentityDbContext>();
            context.Database.Migrate();
        }
    }
}