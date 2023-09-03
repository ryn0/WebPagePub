namespace WebPagePub.ChatCommander.Models.InputModels
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9", IsNullable = false)]
    public partial class urlset
    {

        private urlsetUrl[] urlField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("url")]
        public urlsetUrl[] url
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
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
    public partial class urlsetUrl
    {

        private string locField;

        private System.DateTime lastmodField;

        private image imageField;

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
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.google.com/schemas/sitemap-image/1.1")]
        public image image
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
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.google.com/schemas/sitemap-image/1.1")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.google.com/schemas/sitemap-image/1.1", IsNullable = false)]
    public partial class image
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
