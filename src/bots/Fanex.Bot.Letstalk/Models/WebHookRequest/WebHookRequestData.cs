namespace Fanex.Bot.Letstalk.Models.WebHookRequest
{
    using Newtonsoft.Json;
    using RestSharp.Serializers;

    public class WebHookRequestData
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("source")]
        public Source Source { get; set; }

        [JsonProperty("payload")]
        public string Payload { get; set; }
    }
}