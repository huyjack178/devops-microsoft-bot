namespace Fanex.Bot.API
{
    using System;
    using System.Linq;
    using Fanex.Bot.API.Middlewares;
    using Fanex.Bot.API.Services;
    using Fanex.Bot.Core.Utilities.Web;
    using Fanex.Data;
    using Fanex.Data.Repository;
    using Fanex.Logging;
    using Fanex.Logging.RabbitMQ;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ONEbook.UM;
    using ONEbook.UM.AspNetCore.Configuration;
    using ONEbook.UM.Helpers;
    using ONEbook.UM.Services;
    using ONEbook.UM.Transport;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            LogManager
                .SetDefaultLogCategory(Configuration["Fanex.Logging:DefaultCategory"])
                .Use(new RabbitMQLogging(Configuration["Fanex.Logging:RabbitMQConnectionStrings"]));
            services.AddSingleton(Logger.Log);
            services.AddSingleton<RestSharp.IRestClient, RestSharp.RestClient>();
            services.AddSingleton<IWebClient, RestSharpWebClient>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IMaintenanceService, MaintenanceService>();
            services.AddSingleton<IDynamicRepository, DynamicRepository>();
            services.AddSingleton<ILogService, Services.LogService>();
            services.AddSingleton<IDBLogService, DBLogService>();
            services.AddSingleton<IZabbixService, ZabbixService>();
            services.AddSession();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware(typeof(ErrorHandlingMiddleware));
            app.UseSession();
            app.UseHttpsRedirection();
            app.UseMvc();

            var umConfig = new UMClientConfiguration(Configuration);

            UMClientManager
               .UseConfig(umConfig)
               .UseRestClient(new RestClient(umConfig))
               .UseHttpContext(new HttpContextHelper(app.ApplicationServices.GetService<IHttpContextAccessor>()));

            var connections = Configuration
                 .GetSection("ConnectionStrings")
                 .GetChildren()
                 .ToDictionary(connection => connection.Key, connection => connection.Value, StringComparer.OrdinalIgnoreCase);

            DbSettingProviderManager
                .StartNewSession()
                .UseConnectionStrings(connections)
                .UseDefaultDbSettingProvider(
                    Configuration.GetSection("StoreProcedureDirectory").Value ??
                    throw new InvalidOperationException("Missing StoreProcedureDirectory config"))
                .Run();
        }
    }
}