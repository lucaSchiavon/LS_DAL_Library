using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace it.itryframework2.config
{
    public sealed class DALConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("connstring", IsRequired = true)]
        public string ConnString
        {
            get { return this["connstring"] as string; }
        }

        [ConfigurationProperty("provider", IsRequired = false)]
        public string Provider
        {
            get { return this["provider"] as string; }
        }

        [ConfigurationProperty("managerCryptoName", IsRequired = false)]
        public string ManagerCryptoName
        {
            get { return this["managerCryptoName"] as string; }
        }
        [ConfigurationProperty("decryptpwd", IsRequired = false, DefaultValue = false)]
        public bool DecryptPwd
        {
            get
            {
                try
                {
                    bool _decrypt;
                    bool.TryParse(this["decryptpwd"].ToString(), out _decrypt);
                    return _decrypt;
                }
                catch { return false; }
            }
        }
    }
}
