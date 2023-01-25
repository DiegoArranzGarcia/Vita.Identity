using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using IdentityServer4;
using IdentityServer4.Models;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
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
using Vita.Identity.Host.Config;
using Vita.Identity.Infrastructure.Sql;
using Vita.Identity.Infrastructure.Sql.Aggregates.Users;
using Vita.Identity.Infrastructure.Sql.Configuration;
using Vita.Identity.Infrastructure.Sql.QueryStore;


namespace Vita.Identity.Host
{
    public class Startup
    {
        public const string ApiCorsPolicy = "api";
        public const string IdentityApiScopePolicy = "identity-api-scope";

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
            }).AddInMemoryApiScopes(ApiScopeConfiguration.GetApiScopes())
              .AddInMemoryIdentityResources(IdentityServerConfiguration.GetIdentityResources())
              .AddInMemoryClients(IdentityServerConfiguration.GetClients())
              .AddProfileService<ProfileService>()
              .AddSigningCredential(GenerateCertFromAsym());

            services.AddAuthentication()
                    .AddGoogle(googleOptions =>
                    {
                        googleOptions.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                        googleOptions.SaveTokens = true;

                        googleOptions.Scope.Add("https://www.googleapis.com/auth/calendar");

                        googleOptions.ClientId = Configuration["GoogleClientId"];
                        googleOptions.ClientSecret = Configuration["GoogleClientSecret"];
                    })
                    .AddJwtBearer("Bearer", options =>
                    {
                        options.Authority = "https://localhost:44360";

                        options.RequireHttpsMetadata = false;

                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateAudience = false
                        };
                    });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(IdentityApiScopePolicy, policy =>
                {
                    policy.RequireClaim("scope", ApiScopeConfiguration.IdentityApiScope);
                });
            });

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
        }

        private static string ExtractUri(string uriString)
        {
            Uri uri = new(uriString);
            return $"{uri.Scheme}://{uri.Authority}";
        }

        private static X509Certificate2 GenerateCertFromAsym()
        {
            Uri keyVaultUri = new(Environment.GetEnvironmentVariable("VitaKeyVaultUri"));
            SecretClient secretClient = new(keyVaultUri, new DefaultAzureCredential());

            KeyVaultSecret secret = secretClient.GetSecret("SignInCredentialsCert").Value;

            return new X509Certificate2(Convert.FromBase64String(secret.Value), (string)null, X509KeyStorageFlags.MachineKeySet |
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
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<IUserQueryStore, UserQueryStore>();
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