using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace it.itryframework2.config
{
    public sealed class LogConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("sqlToVsOutputWindow", IsRequired = false)]
        public bool SqlToVsOutputWindow
        {
            get { return Convert.ToBoolean(this["sqlToVsOutputWindow"]); }
        }

        
    }
}
