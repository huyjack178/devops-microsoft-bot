namespace Fanex.Bot
{
    using System;
    using System.Linq;
    using System.Net;
    using Fanex.Bot.Dialogs;
    using Fanex.Bot.Dialogs.Impl;
    using Fanex.Bot.Filters;
    using Fanex.Bot.Models;
    using Fanex.Bot.Services;
    using Fanex.Bot.Utilitites;
    using Hangfire;
    using Hangfire.Dashboard;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Bot.Connector;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(_ => Configuration);

            services.AddHangfire(config =>
                config.UseSqlServerStorage(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDbContext<BotDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddOptions();
            services.AddScoped<GitLabAttribute>();
            services.AddSingleton<IWebClient>(new JsonWebClient(
                new Uri(Configuration.GetSection("LogInfo")?.GetSection("mSiteUrl")?.Value)));
            services.AddSingleton<ILogService, LogService>();

            services.AddScoped<IDialog, Dialog>();
            services.AddScoped<IRootDialog, RootDialog>();
            services.AddScoped<ILogDialog, LogDialog>();
            services.AddScoped<IGitLabDialog, GitLabDialog>();

            var credentialProvider = new StaticCredentialProvider(Configuration.GetSection(MicrosoftAppCredentials.MicrosoftAppIdKey)?.Value,
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

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseExceptionHandler("/api/Error/");

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseHangfireServer();
            app.UseHangfireDashboard(options: new DashboardOptions
            {
                Authorization = Enumerable.Empty<IDashboardAuthorizationFilter>()
            });

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}