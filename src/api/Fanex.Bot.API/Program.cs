namespace Fanex.Bot.API
{
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Serilog;

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel()
                .UseIISIntegration()
                .UseSerilog(
                (hostingContext, loggerConfiguration) => loggerConfiguration
                    .WriteTo.File(
                        $"{hostingContext.HostingEnvironment.ContentRootPath}\\Logs\\log.txt",
                        rollingInterval: RollingInterval.Day))
                .UseStartup<Startup>();
    }
}