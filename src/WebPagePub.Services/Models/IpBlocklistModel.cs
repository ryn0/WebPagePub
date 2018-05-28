namespace WebPagePub.Services.Models
{


    public class IpBlocklistModel
    {
        public bool ishijacked { get; set; }
        public bool isspider { get; set; }
        public bool istor { get; set; }
        public bool isdshield { get; set; }
        public bool isvpn { get; set; }
        public string ip { get; set; }
        public bool isspyware { get; set; }
        public bool isspambot { get; set; }
        public object[] blocklists { get; set; }
        public int lastseen { get; set; }
        public bool isbot { get; set; }
        public int listcount { get; set; }
        public bool isproxy { get; set; }
        public bool ismalware { get; set; }
        public bool islisted { get; set; }
        public bool isexploitbot { get; set; }
    }

}
