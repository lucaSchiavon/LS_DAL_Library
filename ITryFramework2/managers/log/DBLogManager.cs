using System;
using System.Collections.Generic;
using System.Text;

namespace it.itryframework2.managers.log
{
    public sealed class DBLogManager : it.itryframework2.interfaces.ILog
    {
        #region ILog Membri di

        bool it.itryframework2.interfaces.ILog.manageError(it.itryframework2.config.ErrorConfigurationSection errorSection, Exception ex, string customMessage)
        {
            throw new NotImplementedException("ancora da sviluppare");
        }

        #endregion
    }
}
