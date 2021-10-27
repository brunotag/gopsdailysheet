using System.Configuration;
namespace GopsDailySheet.Config
{
    //Extend the ConfigurationElement class.  This class represents a single element in the collection.
    //Create a property for each xml attribute in your element.
    //Decorate each property with the ConfigurationProperty decorator.  See MSDN for all available options.
    public class TabElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }
        [ConfigurationProperty("caption", IsRequired = true)]
        public string Caption
        {
            get { return (string)this["caption"]; }
            set { this["caption"] = value; }
        }
        [ConfigurationProperty("url", IsRequired = true)]
        public string Url
        {
            get { return (string)this["url"]; }
            set { this["url"] = value; }
        }
        [ConfigurationProperty("zoomFactor", IsRequired = false)]
        public double? ZoomFactor
        {
            get { return (double?)this["zoomFactor"]; }
            set { this["zoomFactor"] = value; }
        }
    }
}