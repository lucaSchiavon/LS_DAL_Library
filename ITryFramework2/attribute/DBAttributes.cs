using System;

namespace it.itryframework2.attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class DBAttributes : Attribute
    {
        public DBAttributes() { }

        private bool m_ignoreOnInsert;
        /// <summary>
        /// Se False permette inserimenti anche nella colonna indicata dalla proprieta PrimaryKey dell'interfaccia IGenericObject
        /// </summary>
        public bool IgnoreOnInsert
        {
            get { return m_ignoreOnInsert; }
            set { m_ignoreOnInsert = value; }
        }

        /// <summary>
        /// Ottiene o imposta il nome esatto della colonna nel database.
        /// </summary>
        private string m_dbColumnName;
        public string DbColumnName
        {
            get { return m_dbColumnName; }
            set { m_dbColumnName = value; }
        }

        /// <summary>
        /// Ottiene o imposta il tipo di dato accettato nel database.
        /// </summary>
        private string m_dataType;
        public string DataType
        {
            get { return m_dataType; }
            set { m_dataType = value; }
        }

        /// <summary>
        /// Ottiene o imposta la lunghezza del campo consentita.
        /// </summary>
        private int m_dataSize;
        public int DataSize
        {
            get { return m_dataSize; }
            set { m_dataSize = value; }
        }

        /// <summary>
        /// Ottiene o imposta se il campo accetta o meno valori Null.
        /// </summary>
        private bool m_allowNull;
        public bool AllowNull
        {
            get { return m_allowNull; }
            set { m_allowNull = value; }
        }

        /// <summary>
        /// Ottiene o imposta il valore di default da utilizzare.
        /// </summary>
        private string m_defaultDataValue;
        public string DefaultDataValue
        {
            get { return m_defaultDataValue; }
            set { m_defaultDataValue = value; }
        }

        private bool m_useNPrefix;
        /// <summary>
        /// Se True usa il prefisso N. Prefisso valido per database SQL Server
        /// </summary>
        public bool UseNPrefix
        {
            get { return m_useNPrefix; }
            set { m_useNPrefix = value; }
        }

	
    }
}
