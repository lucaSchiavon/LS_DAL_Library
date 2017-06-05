using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace it.itryframework2.config
{
    public sealed class MailConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("smtp", IsRequired = true)]
        public string Smtp
        {
            get { return this["smtp"] as string; }
        }

        [ConfigurationProperty("authentication_pwd", IsRequired = false)]
        public string AuthenticationPwd
        {
            get { return this["authentication_pwd"] as string; }
        }
    }
}
