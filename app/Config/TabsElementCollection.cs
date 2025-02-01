using System.Configuration;
namespace GopsDailySheet.Config
{
    //Extend the ConfigurationElementCollection class.
    //Decorate the class with the class that represents a single element in the collection.
    [ConfigurationCollection(typeof(TabElement))]
    public class TabsElementCollection : ConfigurationElementCollection
    {
        public TabElement this[int index]
        {
            get { return (TabElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }
        protected override ConfigurationElement CreateNewElement()
        {
            return new TabElement();
        }
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TabElement)element).Name;
        }
    }
}