namespace Fanex.Bot.Letstalk
{
    using Fanex.Bot.Core.Utilities.Web;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Bot.Connector;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using RestSharp;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton(_ => Configuration);
            services.AddSingleton<IRestClient, RestClient>();
            services.AddSingleton<IWebClient, WebClient>();

            var credentialProvider = new StaticCredentialProvider(
            Configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value,
            Configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppPasswordKey)?.Value);

            services.AddAuthentication(
                    options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    }
                )
                .AddBotAuthentication(credentialProvider);

            services.AddSingleton(typeof(ICredentialProvider), credentialProvider);

            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(TrustServiceUrlAttribute));
            });
        }

#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
#pragma warning disable S1075 // URIs should not be hardcoded

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseExceptionHandler("/api/Error/");

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc();
        }

#pragma warning restore S1075 // URIs should not be hardcoded

#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
    }
}