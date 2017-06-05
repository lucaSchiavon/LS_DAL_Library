using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Xml;

namespace it.itryframework2.config
{
    public sealed class DbErrorConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("connstring", IsRequired = true)]
        public string ConnString
        {
            get { return this["connstring"] as string; }
        }
        [ConfigurationProperty("providerType", IsRequired = true)]
        public string ProviderType
        {
            get { return this["providerType"] as string; }
        }

        internal void setLoadValuesFromXml(XmlNode section)
        {
            //XmlAttributeCollection attrs = section.Attributes;

            //if (attrs["message"] != null)
            //{
            //    _message = attrs["message"].Value;
            //    attrs.RemoveNamedItem("message");
            //}

            //if (attrs["favoriteNumber"] != null)
            //{
            //    _favoriteNumber = Convert.ToInt32(attrs["favoriteNumber"].Value);
            //    attrs.RemoveNamedItem("favoriteNumber");
            //}

            //if (attrs["showMessageInBold"] != null)
            //{
            //    _showMessageInBold = XmlConvert.ToBoolean(attrs["showMessageInBold"].Value);
            //    attrs.RemoveNamedItem("showMessageInBold");
            //}

            //// If there are any further attributes, there's an error!
            //if (attrs.Count > 0)
            //    throw new ConfigurationException("There are illegal attributes provided in the section");
        }
    }

    internal class DbErrorConfigurationHandler : IConfigurationSectionHandler
    {

        #region IConfigurationSectionHandler Membri di

        object IConfigurationSectionHandler.Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            DbErrorConfigurationSection errSection = new DbErrorConfigurationSection();
            errSection.setLoadValuesFromXml(section);
            return errSection;
        }

        #endregion
    }
}
