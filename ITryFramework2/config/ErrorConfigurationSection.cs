using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace it.itryframework2.config
{
    public sealed class ErrorConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("managerClassName", IsRequired = true)]
        public string ManagerClassName
        {
            get { return this["managerClassName"] as string; }
        }
        [ConfigurationProperty("mailFrom", IsRequired = true)]
        public string MailFrom
        {
            get { return this["mailFrom"] as string; }
        }
        [ConfigurationProperty("mailTo", IsRequired = true)]
        public string MailTo
        {
            get { return this["mailTo"] as string; }
        }
        [ConfigurationProperty("mailBcc", IsRequired = false)]
        public string MailBcc
        {
            get { return this["mailBcc"] as string; }
        }
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

        [ConfigurationProperty("enable", IsRequired = false,DefaultValue=true)]
        public bool Enable
        {
            get {
                try { 
                    bool _enable;
                    bool.TryParse(this["enable"].ToString(),out _enable);
                    return _enable;
                }
                catch { return false; }
            }
        }
    }
}
