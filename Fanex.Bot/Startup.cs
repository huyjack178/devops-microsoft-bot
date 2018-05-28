namespace Fanex.Bot
{
    using System;
    using Fanex.Bot.Dialogs;
    using Fanex.Bot.Services;
    using Fanex.Bot.Utilitites;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Bot.Builder.BotFramework;
    using Microsoft.Bot.Builder.Core.Extensions;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Builder.TraceExtensions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

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

            services.AddSingleton<IWebClient>(new JsonWebClient(new Uri(Configuration.GetSection("mSiteUrl")?.Value)));
            services.AddSingleton<ILogService, LogService>();
            services.AddSingleton<ILogDialog, LogDialog>();

            services.AddBot<Bot>(options =>
            {
                options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);
                options.Middleware.Add(new CatchExceptionMiddleware<Exception>(async (context, exception) =>
                {
                    await context.TraceActivity("Bot Exception", exception);
                    await context.SendActivity(exception.Message);
                }));
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseAuthentication()
                .UseBotFramework();
        }
    }
}