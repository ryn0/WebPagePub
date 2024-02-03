using System.Xml.Serialization;

namespace WebPagePub.PageManager.Console.Models.InputModels
{
    [Serializable()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType = true, Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
    [XmlRoot("urlset", Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9", IsNullable = false)]
    public partial class UrlSet
    {
        private UrlSetUrl[] urlField = Array.Empty<UrlSetUrl>();

        [System.Xml.Serialization.XmlElementAttribute("url")]
        public UrlSetUrl[] Url
        {
            get { return this.urlField; }
            set { this.urlField = value; }
        }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType("urlsetUrl", AnonymousType = true, Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
    public partial class UrlSetUrl
    {
        private string locField = string.Empty;

        private System.DateTime lastmodField;

        private decimal priorityField;

        [XmlElement("loc")]
        public string Loc
        {
            get
            {
                return this.locField;
            }
            set
            {
                this.locField = value;
            }
        }

        [XmlElement("lastmod")]
        public System.DateTime Lastmod
        {
            get
            {
                return this.lastmodField;
            }
            set
            {
                this.lastmodField = value;
            }
        }

        [XmlElement("priority")]
        public decimal Priority
        {
            get
            {
                return this.priorityField;
            }
            set
            {
                this.priorityField = value;
            }
        }
    }
}