using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SkyDocs.Blazor.Models
{
    public class TheGraphShareResultModel
    {
        [JsonPropertyName("shares")]
        public List<TheGraphShare> Shares { get; set; } = new List<TheGraphShare>();
    }
    public class TheGraphShare
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("shareData")]
        public string? Skylink { get; set; }

        [JsonPropertyName("sender")]
        public string? Sender { get; set; }

        [JsonPropertyName("receiver")]
        public string? Receiver { get; set; }

        [JsonPropertyName("blockNumber")]
        public string? BlockNumber { get; set; }

        

    }
}
