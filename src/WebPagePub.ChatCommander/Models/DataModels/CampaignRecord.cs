namespace WebPagePub.ChatCommander.Models.DataModels
{
    public class CampaignRecord
    {
        public string ID { get; set; }
        public string Banner { get; set; }
        public string Campaign { get; set; }
        public char Type { get; set; }
        public DateTime Date { get; set; }
        public string IP { get; set; }
        public string Channel { get; set; }
        public string ReferrerURL { get; set; }

        public CampaignRecord(
            string id,
            string banner,
            string campaign,
            char type,
            DateTime date,
            string ip,
            string channel,
            string referrerURL)
        {
            ID = id;
            Banner = banner;
            Campaign = campaign;
            Type = type;
            Date = date;
            IP = ip;
            Channel = channel;
            ReferrerURL = referrerURL;
        }

        public override string ToString()
        {
            return $"ID: {ID}, Banner: {Banner}, Campaign: {Campaign}, Type: {Type}, Date: {Date}, IP: {IP}, Channel: {Channel}, Referrer URL: {ReferrerURL}";
        }
    }
}