namespace Fanex.Bot
{
    using System.Linq;
    using Fanex.Bot.Core.Utilities.Web;
    using Fanex.Bot.Dialogs;
    using Fanex.Bot.Filters;
    using Fanex.Bot.Models;
    using Fanex.Bot.Services;
    using Fanex.Bot.Utilitites.Bot;
    using Hangfire;
    using Hangfire.Dashboard;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Bot.Connector;
    using Microsoft.EntityFrameworkCore;
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

            RegisterHangfire(services);

            services.AddDbContext<BotDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<GitLabAttribute>();

            RegisterBotServices(services);
            RegisterBotDialogs(services);
            RegisterBotAuthentication(services);

            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(TrustServiceUrlAttribute));
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseExceptionHandler("/api/Error/");
            app.UseHangfireServer();
            app.UseHangfireDashboard(options: new DashboardOptions
            {
                Authorization = Enumerable.Empty<IDashboardAuthorizationFilter>()
            });

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc();
        }

        private void RegisterHangfire(IServiceCollection services)
        {
            services.AddHangfire(config =>
                config.UseSqlServerStorage(Configuration.GetConnectionString("DefaultConnection")));
            services.AddSingleton<IRecurringJobManager, RecurringJobManager>();
            services.AddSingleton<IBackgroundJobClient, BackgroundJobClient>();
        }

        private void RegisterBotAuthentication(IServiceCollection services)
        {
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
        }

        private static void RegisterBotServices(IServiceCollection services)
        {
            services.AddSingleton<ILogService, LogService>();
        }

        private static void RegisterBotDialogs(IServiceCollection services)
        {
            services.AddScoped<IConversation, Conversation>();
            services.AddScoped<IDialog, Dialog>();
            services.AddScoped<IRootDialog, RootDialog>();
            services.AddScoped<ILogDialog, LogDialog>();
            services.AddScoped<IGitLabDialog, GitLabDialog>();
        }
    }
}