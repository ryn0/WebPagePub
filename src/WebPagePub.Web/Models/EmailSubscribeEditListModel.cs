﻿using System.Collections.Generic;

namespace WebPagePub.Web.Models
{
    public class EmailSubscribeEditListModel
    {
        public List<EmailSubscribeEditModel> Items { get; set; } = new List<EmailSubscribeEditModel>();

        public string Emails { get; set; }

        public string UnsubscribeLink { get; set; }
    }
}
