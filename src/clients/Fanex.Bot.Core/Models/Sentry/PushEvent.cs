namespace Fanex.Bot.Models.Sentry
{
    using Newtonsoft.Json;

    public class PushEvent
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("project")]
        public string Project { get; set; }

        [JsonProperty("project_name")]
        public string ProjectName { get; set; }

        [JsonProperty("culprit")]
        public string Culprit { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

#pragma warning disable S3996 // URI properties should not be strings

        [JsonProperty("url")]
        public string Url { get; set; }

#pragma warning restore S3996 // URI properties should not be strings

        [JsonProperty("level")]
        public string Level { get; set; }

        [JsonProperty("event")]
        public Event Event { get; set; }
    }
}