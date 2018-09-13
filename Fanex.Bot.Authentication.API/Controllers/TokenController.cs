﻿namespace Fanex.Bot.Authentication.API.Controllers
{
    using System.Threading.Tasks;
    using Fanex.Bot.Authentication.API.Services;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
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