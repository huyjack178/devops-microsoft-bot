namespace Fanex.Bot
{
    using System.Linq;
    using Fanex.Bot.Core.Utilities.Web;
    using Fanex.Bot.Filters;
    using Fanex.Bot.Skynex.Dialogs;
    using Fanex.Bot.Skynex.Models;
    using Fanex.Bot.Skynex.Services;
    using Fanex.Bot.Skynex.Utilities.Bot;
    using Hangfire;
    using Hangfire.Dashboard;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Connector.DirectLine;
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
            ConfigureCommonMiddlewares(services);
            ConfigureHangfire(services);
            ConfigureDbContext(services);
            ConfigureAttributes(services);
            ConfigureBotServices(services);
            ConfigureBotDialogs(services);
            ConfigureBotAuthentication(services);

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

        private static void ConfigureAttributes(IServiceCollection services)
        {
            services.AddScoped<GitLabAttribute>();
        }

        private void ConfigureDbContext(IServiceCollection services)
        {
            services.AddDbContext<BotDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
        }

        private void ConfigureCommonMiddlewares(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton(_ => Configuration);
            services.AddSingleton<IRestClient, RestClient>();
            services.AddSingleton<IWebClient, WebClient>();
        }

        private void ConfigureHangfire(IServiceCollection services)
        {
            services.AddHangfire(config =>
                config.UseSqlServerStorage(Configuration.GetConnectionString("DefaultConnection")));
            services.AddSingleton<IRecurringJobManager, RecurringJobManager>();
            services.AddSingleton<IBackgroundJobClient, BackgroundJobClient>();
        }

        private static void ConfigureBotServices(IServiceCollection services)
        {
            services.AddSingleton<ILogService, LogService>();
            services.AddSingleton<IUMService, UMService>();
        }

        private static void ConfigureBotDialogs(IServiceCollection services)
        {
            services.AddScoped<ILineConversation, LineConversation>();
            services.AddScoped<ISkypeConversation, SkypeConversation>();
            services.AddScoped<IConversation, Skynex.Utilities.Bot.Conversation>();
            services.AddScoped<ICommonDialog, CommonDialog>();
            services.AddScoped<ILogDialog, LogDialog>();
            services.AddScoped<IGitLabDialog, GitLabDialog>();
            services.AddScoped<ILineDialog, LineDialog>();
            services.AddScoped<IUMDialog, UMDialog>();
        }

        private void ConfigureBotAuthentication(IServiceCollection services)
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

            services.AddSingleton<IDirectLineClient>(new DirectLineClient(Configuration.GetSection("DirectLineSecret")?.Value));
        }
    }
}