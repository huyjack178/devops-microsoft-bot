namespace Fanex.Bot.Service
{
    using System.Web.Http;
    using Fanex.Bot.Service.App_Start;
    using Fanex.Bot.Service.Services;
    using Fanex.Data.Repository;
    using Unity;
    using Unity.Lifetime;

    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            var container = new UnityContainer();
            container.RegisterType<IDynamicRepository, DynamicRepository>(new SingletonLifetimeManager());
            container.RegisterType<ILogService, LogService>(new SingletonLifetimeManager());
            config.DependencyResolver = new UnityResolver(container);

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}