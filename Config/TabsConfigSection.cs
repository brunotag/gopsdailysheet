using System.Configuration;
namespace GopsDailySheet.Config
{
    //Extend the ConfigurationSection class.  Your class name should match your section name and be postfixed with "Section".
    public class TabsConfigSection : ConfigurationSection
    {
        //Decorate the property with the tag for your collection.
        [ConfigurationProperty("tabs")]
        public TabsElementCollection Tabs
        {
            get { return (TabsElementCollection)this["tabs"]; }
        }
    }
}