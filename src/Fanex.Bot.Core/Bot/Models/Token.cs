using RestSharp.Deserializers;

namespace Fanex.Bot.Core.Bot.Models
{
    public class Token
    {
        [DeserializeAs(Name = "token_type")]
        public string TokenType { get; set; }

        [DeserializeAs(Name = "expires_in")]
        public int ExpiresIn { get; set; }

        [DeserializeAs(Name = "access_token")]
        public string AccessToken { get; set; }
    }
}