using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FishpiVS2026Plugin.Models
{
    public class BreezemoonRequest
    {
        [JsonPropertyName("apiKey")]
        public string ApiKey { get; set; }

        [JsonPropertyName("breezemoonContent")]
        public string BreezemoonContent { get; set; }
    }
}
