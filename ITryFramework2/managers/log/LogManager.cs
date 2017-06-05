using System;
using System.Web;
using System.Net.Mail;
using System.Configuration;
using System.Collections.Specialized;
using System.Text;
using System.Collections;
using it.itryframework2.exception;
using it.itryframework2.config;
using it.itryframework2.interfaces;

namespace it.itryframework2.managers.log
{
    public sealed class LogManager
    {
        private LogManager(){}

        public static void trace(Exception ex, string customError)
        {
            //string[] errConfig = it.itryframework2.managers.config.ConfigManager.getErrorConfigValues();
            ErrorConfigurationSection errSection = it.itryframework2.managers.config.ConfigManager.getErrorConfigValues();
            if (!errSection.Enable) return;

            ILog iLog = null;
            iLog = (ILog)Activator.CreateInstance(Type.GetType(errSection.ManagerClassName));
            iLog.manageError(errSection,ex, customError);
        }
    }
}
