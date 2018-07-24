namespace Fanex.Bot.Letstalk.Models.WebHookRequest
{
    using Newtonsoft.Json;
    using RestSharp.Serializers;

    public class Source
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }
    }
}