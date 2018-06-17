using Newtonsoft.Json;

namespace WebPagePub.Services.Models
{
    public class IpBlocklistModel
    {
        [JsonProperty("ishijacked")]
        public bool Ishijacked { get; set; }

        [JsonProperty("isspider")]
        public bool Isspider { get; set; }

        [JsonProperty("istor")]
        public bool Istor { get; set; }

        [JsonProperty("isdshield")]
        public bool Isdshield { get; set; }

        [JsonProperty("isvpn")]
        public bool Isvpn { get; set; }

        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("isspyware")]
        public bool Isspyware { get; set; }

        [JsonProperty("isspambot")]
        public bool Isspambot { get; set; }

        [JsonProperty("blocklists")]
        public object[] Blocklists { get; set; }

        [JsonProperty("lastseen")]
        public int Lastseen { get; set; }

        [JsonProperty("isbot")]
        public bool Isbot { get; set; }

        [JsonProperty("listcount")]
        public int Listcount { get; set; }

        [JsonProperty("isproxy")]
        public bool Isproxy { get; set; }

        [JsonProperty("ismalware")]
        public bool Ismalware { get; set; }

        [JsonProperty("islisted")]
        public bool Islisted { get; set; }

        [JsonProperty("isexploitbot")]
        public bool Isexploitbot { get; set; }
    }
}