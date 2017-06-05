using System;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Text;

namespace it.itryframework2.interfaces
{
    public interface IGenericObject
    {
        string TableName
        {
            get;
        }

        string PrimaryKey
        {
            get;
        }
    }
}
