using System;
using System.Collections.Generic;
using System.Text;

namespace it.itryframework2.interfaces
{
    internal interface ILog
    {
        bool manageError(it.itryframework2.config.ErrorConfigurationSection errorSection,Exception ex, string customMessage);

    }
}
