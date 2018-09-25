﻿namespace Fanex.Bot
{
    using System.Linq;
    using Fanex.Bot.Core.Utilities.Web;
    using Fanex.Bot.Filters;
    using Fanex.Bot.Models;
    using Fanex.Bot.Services;
    using Fanex.Bot.Skynex.Dialogs;
    using Fanex.Bot.Skynex.MessageHandlers.MessageBuilders;
    using Fanex.Bot.Skynex.MessageHandlers.MessageSenders;
    using Fanex.Bot.Skynex.MessageHandlers.MessengerFormatters;
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
            ConfigureBotMessageHandlers(services);
            ConfigureBotDialog(services);
            ConfigureBotAuthentication(services);

            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(TrustServiceUrlAttribute));
            });
        }

#pragma warning disable S1075 // URIs should not be hardcoded

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

#pragma warning restore S1075 // URIs should not be hardcoded

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
            services.AddSingleton<IWebClient, RestSharpWebClient>();
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
            services.AddSingleton<IUnderMaintenanceService, UnderMaintenanceService>();
            services.AddSingleton<ITokenService, TokenService>();
        }

        private static void ConfigureBotDialog(IServiceCollection services)
        {
            services.AddScoped<ICommonDialog, CommonDialog>();
            services.AddScoped<ILogDialog, LogDialog>();
            services.AddScoped<IGitLabDialog, GitLabDialog>();
            services.AddScoped<ILineDialog, LineDialog>();
            services.AddScoped<IUnderMaintenanceDialog, UnderMaintenanceDialog>();
            services.AddScoped<IDBLogDialog, DBLogDialog>();
        }

        private static void ConfigureBotMessageHandlers(IServiceCollection services)
        {
            services.AddSingleton<IGitLabMessageBuilder, GitLabMessageBuilder>();
            services.AddSingleton<IWebLogMessageBuilder, WebLogMessageBuilder>();
            services.AddSingleton<IDBLogMessageBuilder, DBLogMessageBuilder>();
            services.AddSingleton<IMessengerFormatter, DefaultFormatter>();
            services.AddSingleton<ILineFormatter, LineFormatter>();
            services.AddScoped<ILineConversation, LineConversation>();
            services.AddScoped<ISkypeConversation, SkypeConversation>();
            services.AddScoped<IConversation, Skynex.MessageHandlers.MessageSenders.Conversation>();
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