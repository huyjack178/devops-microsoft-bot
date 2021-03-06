using System.Threading.Tasks;
using Fanex.Bot.Core.Bot.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fanex.Bot.Skynex.Bot
{
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly ITokenService tokenService;

        public TokenController(ITokenService tokenService)
        {
            this.tokenService = tokenService;
        }

        [HttpGet]
        public async Task<string> Get(string clientId, string clientPassword)
        {
            var token = await tokenService.GetToken(clientId, clientPassword);

            if (string.IsNullOrEmpty(token))
            {
                return "Invalid client information";
            }

            return token;
        }
    }
}