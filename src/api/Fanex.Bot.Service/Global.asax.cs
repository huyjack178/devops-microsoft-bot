namespace Fanex.Bot.Service
{
    using System.Web;
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Routing;
    using Fanex.Data;
    using Fanex.Data.OldConnectionStringProvider;

    public class WebApiApplication : HttpApplication
#pragma warning disable S1075 // URIs should not be hardcoded
#pragma warning disable S2325 // Methods and properties that don't access instance data should be static

    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            DbSettingProviderManager
                .StartNewSession()
                .Use(new OldConnectionStringProvider())
                .UseDefaultDbSettingProvider("~/App_Data")
                .Run();
        }
    }

#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
#pragma warning restore S1075 // URIs should not be hardcoded
}