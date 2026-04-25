
using System;
using System.Text.Json.Serialization;

namespace WebPagePub.Services.Models.Sponsors
{
    public class SponsorCardItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("link")]
        public string Link { get; set; } = string.Empty;

        [JsonPropertyName("expirationDate")]
        public DateTime ExpirationDate { get; set; }

        [JsonPropertyName("reviewRating")]
        public double ReviewRating { get; set; }

        [JsonPropertyName("reviewCount")]
        public int ReviewCount { get; set; }

        [JsonPropertyName("reviewLink")]
        public string ReviewLink { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("note")]
        public string Note { get; set; } = string.Empty;

        [JsonPropertyName("sponsorshipType")]
        public string SponsorshipType { get; set; } = string.Empty;

        public bool IsMainSponsor =>
            string.Equals(this.SponsorshipType, "MainSponsor", StringComparison.OrdinalIgnoreCase);
    }
}
