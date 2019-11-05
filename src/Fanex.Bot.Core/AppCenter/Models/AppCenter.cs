using System;
using Newtonsoft.Json;

namespace Fanex.Bot.Core.AppCenter.Models
{
    public class AppCenterEvent
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("sent_at")]
        public DateTimeOffset SendAt { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}