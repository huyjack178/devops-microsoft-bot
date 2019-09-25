using Newtonsoft.Json;

namespace Fanex.Bot.Core.Sentry.Models
{
    public class Context
    {
        [JsonProperty("os")]
        public Os Os { get; set; }

        [JsonProperty("browser")]
        public Browser Browser { get; set; }

        [JsonProperty("device")]
        public Device Device { get; set; }
    }

    public class Os
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Browser
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Device
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        [JsonProperty("arch")]
        public string Arch { get; set; }
    }
}