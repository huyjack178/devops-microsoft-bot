namespace Fanex.Bot.Filters
{
    using System;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Configuration;

    [AttributeUsage(AttributeTargets.Class)]
    public class GitLabAttribute : Attribute, IActionFilter
    {
        private readonly IConfiguration _configuration;

        public GitLabAttribute(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            context.Result = new OkResult();
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;
            var gitLabToken = request.Headers["X-Gitlab-Token"];
            var validGitLabToken = _configuration.GetSection("GitLabInfo")?.GetSection("SecretToken")?.Value;

            if (gitLabToken != validGitLabToken)
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}