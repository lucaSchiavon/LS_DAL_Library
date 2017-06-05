using System;
using System.Collections.Specialized;
using System.Text;
using System.Configuration;
using System.Web;
using it.itryframework2.exception;
using System.Collections.Generic;
using it.itryframework2.config;

namespace it.itryframework2.managers.config
{
    internal class ConfigManager
    {
        private ConfigManager() { }


	

        /// <summary>
        /// Legge da config il valore espresso nelle chiavi presenti in stringdictionary e ne scrive il valore.
        /// </summary>
        /// <param name="dic">dictionary</param>
        /// <param name="isWebApp">True per leggere da web.config, False per leggere da app.config<remarks>(lancia un'eccezione metodo non implementato)</remarks></param>
        public static void getValuesFromConfig(StringDictionary dic)
        {
            if (isWebApp())
            {
                string webConfigPath = HttpRuntime.AppDomainAppVirtualPath;
                Configuration conf = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(webConfigPath);
                if (conf.AppSettings.Settings.Count == 0) throw new ITryFrameworkException("Chiave appSettings non trovata");

                
                foreach (KeyValuePair<string, string> keyVal in dic)
                {
                    KeyValueConfigurationElement setting = conf.AppSettings.Settings[keyVal.Key];
                    if (setting != null) dic.Add(keyVal.Key, setting.Value);
                    else dic.Add(keyVal.Key, "");
                }
            }
            else
            {
                //throw new ITryFrameworkException("This method is not implemented.");
            }
        }

        private static bool isWebApp()
        {
            //questo metodo dovrà controllare se trattasi di sito web o applicazione client
            //System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            //config.AppSettings.Settings["oldPlace"].Value = "3";
            //config.Save(ConfigurationSaveMode.Modified);
            //ConfigurationManager.RefreshSection("appSettings"); 
            return true;
        }

        /// <summary>
        /// Ritorna i valori espressi nel config per la sezione ITryFrameworkDALConfig
        /// </summary>
        /// <returns></returns>
        /// <exception cref="it.itryframework2.exception.ITryFrameworkException"></exception>
        public static DALConfigurationSection getDALConfigValues()
        {
            try
            {
                return (DALConfigurationSection)System.Configuration.ConfigurationManager.GetSection("ITryFrameworkDALConfig2");
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Ritorna i valori espressi nel config per la sezione ITryFrameworkLogConfig
        /// </summary>
        /// <returns></returns>
        /// <exception cref="it.itryframework2.exception.ITryFrameworkException"></exception>
        public static LogConfigurationSection getLogConfigValues()
        {
            try
            {
                return (LogConfigurationSection)System.Configuration.ConfigurationManager.GetSection("ITryFrameworkLogConfig2");
            }
            catch
            {
                //sezione opzionale
            }

            return null;
        }

        /// <summary>
        /// Ritorna i valori espressi nel config per la sezione ITryFrameworkErrorConfig
        /// Gli errori non sono gestiti.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="it.itryframework2.exception.ITryFrameworkException"></exception> 
        public static ErrorConfigurationSection getErrorConfigValues()
        {
            try
            {
                return (ErrorConfigurationSection)System.Configuration.ConfigurationManager.GetSection("ITryFrameworkErrorConfig2");
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Ritorna i valori espressi nel config per la sezione ITryFrameworkMailConfig
        /// Gli errori non sono gestiti.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="it.itryframework2.exception.ITryFrameworkException"></exception> 
        public static MailConfigurationSection getMailConfigValues()
        {
            try
            {
                return (MailConfigurationSection)System.Configuration.ConfigurationManager.GetSection("ITryFrameworkMailConfig2");
            }
            catch
            {
                throw;
            }
        }
    }
}
