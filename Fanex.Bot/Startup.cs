namespace Fanex.Bot
{
    using System;
    using System.Linq;
    using Fanex.Bot.Dialogs;
    using Fanex.Bot.Models;
    using Fanex.Bot.Services;
    using Fanex.Bot.Utilitites;
    using Hangfire;
    using Hangfire.Dashboard;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Bot.Builder.BotFramework;
    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Builder.TraceExtensions;
    using Microsoft.Bot.Connector.Authentication;
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

            //var qnaEndpoint = new QnAMakerEndpoint
            //{
            //    Host = Configuration.GetSection("QnaKBHost")?.Value,
            //    EndpointKey = Configuration.GetSection("QnaKBEndpointKey")?.Value,
            //    KnowledgeBaseId = Configuration.GetSection("QnaKBId")?.Value
            //};
            services.AddMemoryCache();
            services.AddHangfire(config =>
                config.UseSqlServerStorage(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDbContext<BotDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddSingleton<IWebClient>(new JsonWebClient(new Uri(Configuration.GetSection("mSiteUrl")?.Value)));
            services.AddSingleton<ILogService, LogService>();
            services.AddScoped<ILogDialog, LogDialog>();

            ConversationStarter.AppId = Configuration.GetSection("MicrosoftAppId")?.Value;
            ConversationStarter.AppPassword = Configuration.GetSection("MicrosoftAppPassword")?.Value;

            services.AddBot<Bot>(options =>
            {
                options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);
                options.Middleware.Add(new CatchExceptionMiddleware<Exception>(async (context, exception) =>
                {
                    await context.TraceActivity("Bot Exception", exception);
                    await context.SendActivity(exception.InnerException.Message);
                }));
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseDeveloperExceptionPage();
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseHangfireServer();
            app.UseHangfireDashboard(options: new DashboardOptions
            {
                Authorization = Enumerable.Empty<IDashboardAuthorizationFilter>()
            });

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseAuthentication()
                .UseBotFramework();
        }
    }
}