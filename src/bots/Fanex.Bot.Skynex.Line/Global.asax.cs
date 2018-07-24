namespace Fanex.Bot.Skynex.Line
{
    using System.Configuration;
    using System.Web;
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Routing;
    using global::Line.Messaging;
    using Microsoft.Bot.Connector.DirectLine;
    using SimpleInjector;
    using SimpleInjector.Integration.WebApi;
    using SimpleInjector.Lifestyles;

    public class WebApiApplication : HttpApplication
    {
        private readonly string accessToken = ConfigurationManager.AppSettings["ChannelAccessToken"];
        private readonly string directLineSecret = ConfigurationManager.AppSettings["DirectLineSecret"];

        protected void Application_Start()
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            container.Register(() => new LineMessagingClient(accessToken), Lifestyle.Singleton);
            container.Register(() => new DirectLineClient(directLineSecret), Lifestyle.Singleton);
            container.Register<ILineBotApp, LineBotApp>(Lifestyle.Scoped);
            container.Verify();

            GlobalConfiguration.Configuration.DependencyResolver =
                new SimpleInjectorWebApiDependencyResolver(container);

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}