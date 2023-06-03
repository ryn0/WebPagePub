using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebPagePub.Web.Models
{
    public class EmailSendModel
    {
        public string SendToEmails { get; set; }

        public string EmailTitle { get; set; }

        public string EmailMessage { get; set; }
    }
}
