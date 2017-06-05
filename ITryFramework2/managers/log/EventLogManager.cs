using System;
using System.Collections.Generic;
using System.Text;
using it.itryframework2.exception;

namespace it.itryframework2.managers.log
{
    /// <summary>
    /// Classe per la gestione errori nel registro eventi di Windows
    /// </summary>
    public sealed class EventLogManager : it.itryframework2.interfaces.ILog
    {
        public EventLogManager() { }
        
        #region ILog Membri di

        public bool manageError(it.itryframework2.config.ErrorConfigurationSection errorSection, Exception ex, string customMessage)
        {
            System.Diagnostics.EventLog.WriteEntry(customMessage, "SOURCE: " + ex.Source + " MESSAGE: " + ex.Message + " STACKTRACE: " + ex.StackTrace);
            return true;
        }

        #endregion
    }
}
