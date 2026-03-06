using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FishpiVS2026Plugin.Models
{
    internal class LivenessResponse
    {
        [JsonPropertyName("liveness")]
        public double Liveness { get; set; }
    }
    internal class LivenessrewardResponse
    {
        [JsonPropertyName("sum")]
        public int Sum { get; set; }
    }
}
