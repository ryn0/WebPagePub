namespace WebPagePub.ChatCommander.Models.InputModels
{
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(TypeName = "urlset", AnonymousType = true, Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9", IsNullable = false)]
    public partial class Urlset
    {
        private UrlsetUrl[]? urlField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("url")]
        public UrlsetUrl[] url
        {
            get
            {
                return this.urlField;
            }
            set
            {
                this.urlField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(TypeName = "urlsetUrl", AnonymousType = true, Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
    public partial class UrlsetUrl
    {
        private string? locField;

        private System.DateTime lastmodField;

        private Image? imageField;

        /// <remarks/>
        public string loc
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

        /// <remarks/>
        public System.DateTime lastmod
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

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute( ElementName = "image", Namespace = "http://www.google.com/schemas/sitemap-image/1.1")]
        public Image Image
        {
            get
            {
                return this.imageField;
            }
            set
            {
                this.imageField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(TypeName = "image", AnonymousType = true, Namespace = "http://www.google.com/schemas/sitemap-image/1.1")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.google.com/schemas/sitemap-image/1.1", IsNullable = false)]
    public partial class Image
    {

        private string locField;

        /// <remarks/>
        public string loc
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
    }


}
