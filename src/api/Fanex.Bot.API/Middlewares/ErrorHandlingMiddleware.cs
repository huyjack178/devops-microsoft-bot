namespace Fanex.Bot.API.Middlewares
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Fanex.Logging;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;

    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context /* other dependencies */)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.Error(BuildErrorMessage(context, ex));
                await HandleExceptionAsync(context, ex);
            }
        }

        private static string BuildErrorMessage(HttpContext httpContext, Exception ex)
        {
            var request = httpContext.Request;
#pragma warning disable S4462 // Calls to "async" methods should not be blocking
            var bodyData = request.GetRawBodyStringAsync().Result;
#pragma warning restore S4462 // Calls to "async" methods should not be blocking
            var path = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
            var headers = string.Join(
                "\r\n",
                request.Headers.Select(header => $"{header.Key}:{header.Value}"));
            var cookies = string.Join(
                "\r\n",
                request.Cookies.Select(cookie => $"{cookie.Key}:{cookie.Value}"));

            return string.Join(
                "\r\n\r\n",
                ex.Message,
                $"PATH: {path}",
                $"BODY: {bodyData}",
                $"HEADERS: \r\n {headers}",
                $"COOKIES: \r\n {cookies}",
                $"EXCEPTION: {ex}");
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            const HttpStatusCode code = HttpStatusCode.InternalServerError; // 500 if unexpected

            ////if (exception is MyNotFoundException) code = HttpStatusCode.NotFound;
            ////else if (exception is MyUnauthorizedException) code = HttpStatusCode.Unauthorized;
            ////else if (exception is MyException) code = HttpStatusCode.BadRequest;

            var result = JsonConvert.SerializeObject(new { error = exception.Message });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(result);
        }
    }
}