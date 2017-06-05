using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Net.Mail;
using System.Data.Odbc;
using it.itryframework2.interfaces;
using it.itryframework2.attributes;
using it.itryframework2.managers.config;
using it.itryframework2.managers.connection;

namespace it.itryframework2.data
{
    public sealed class DBMapper
    {
        private static string PREFISSO_PKEY = "key_";

        string _connString;
        string _providerAssemblyFullName;
        bool _isPwdEncrypted;
        bool _isDefaultDBMapper=false;
        it.itryframework2.config.DALConfigurationSection _DALConfig;
        it.itryframework2.config.LogConfigurationSection _logConfig;

        #region costruttore
        /*
         i tre costruttori sono differenti perchè quello di default pretende che ci siano le sezioni definite nel web.config
         * mentre gli altri due costruiscono una connessione in base ai parametri ricevuti
         */
        private config.LogConfigurationSection _setLoadLogConfigSection()
        {
            _logConfig = it.itryframework2.managers.config.ConfigManager.getLogConfigValues();
            return _logConfig;
        }
        public DBMapper()
        {
            _isDefaultDBMapper = true;
            _DALConfig = it.itryframework2.managers.config.ConfigManager.getDALConfigValues();
            _setLoadLogConfigSection();
        }
        public DBMapper(string connString, string providerAssemblyFullName)
        {
            _connString = connString;
            _providerAssemblyFullName = providerAssemblyFullName;
            _setLoadLogConfigSection();
        }
        public DBMapper(string connString, string providerAssemblyFullName, bool isPwdEncrypted)
        {
            _connString = connString;
            _providerAssemblyFullName = providerAssemblyFullName;
            _isPwdEncrypted = isPwdEncrypted;
            _setLoadLogConfigSection();
        }
        #endregion

        #region private methods
        #region _select
        private bool _select(IGenericObject obj, string nomeCampoId, object idVal)
        {
            StringBuilder sql = new StringBuilder();
            string where = String.Empty;
            sql.Append("SELECT * FROM " + obj.TableName + " WHERE");

            if (!string.IsNullOrEmpty(nomeCampoId) && idVal != null)
            {
                if (!string.IsNullOrEmpty(nomeCampoId))
                {
                    sql.Append(" " + nomeCampoId);
                }
                if (idVal != null)
                {
                    if (idVal is int || idVal is Int64) sql.Append("=" + Convert.ToInt32(idVal));
                    else if (idVal is string) sql.Append("='" + Convert.ToString(idVal).Replace("'", "''") + "'");
                    else throw new Exception("Il valore di idVal non può essere diverso da stringa e intero.");
                }
            }
            else
            {
                //prendo dall'oggetto obj il valore della chiave primaria; 
                Dictionary<string, string> dic = null;
                try { dic = _getDic(obj); }   //dic contiene chiave>>nome colonna | valore>>valore campo
                catch  { throw; }
                foreach (KeyValuePair<string, string> keyVal in dic)
                {
                    if (keyVal.Key.Contains(PREFISSO_PKEY))
                    {
                        //tolgo il prefisso
                        sql.Append(" " + keyVal.Key.Substring(PREFISSO_PKEY.Length) + "=" + keyVal.Value);
                        break;
                    }
                }
            }

            IDataReader reader = null;
            try
            {
                reader = getIDataReader(sql.ToString());
                while (reader.Read())
                {
                    setObject(obj, reader);
                }
                return true;
            }
            catch (Exception ex) { _writeLog(ex, sql.ToString()); throw; }
            finally { if (reader != null) reader.Close(); }
        } 
        #endregion

        #region _readerContains
        private bool _readerContains(IDataReader reader, string dbAttrName)
        {
            DataView view = reader.GetSchemaTable().DefaultView;
            view.RowFilter = "ColumnName='" + dbAttrName + "'";
            return view.Count > 0;
        } 
        #endregion

        #region _convert
        private string _convert(object val, it.itryframework2.attributes.DBAttributes dbAttr)
        {
            it.itryframework2.config.DALConfigurationSection dalSection = ConfigManager.getDALConfigValues();
            //per ora (06.11.07) la lunghezza massima su campo è usata solo per le stringhe
            if (val == null || val is DBNull) return (!string.IsNullOrEmpty(dbAttr.DefaultDataValue)) ? dbAttr.DefaultDataValue : "null";
            if (val is decimal)
            {
                decimal dec;
                if (decimal.TryParse(val.ToString(), out dec)) return dec.ToString().Replace(",", ".");
                //return Convert.ToString(val).Replace(",", ".");
            }
            if (val is double)
            {
                double dbl;
                if (double.TryParse(val.ToString(), out dbl)) return dbl.ToString().Replace(",", ".");
                //return Convert.ToString(val).Replace(",", ".");
            }
            if (val is DateTime)
            {
                DateTime dt = (DateTime)val;
                if (dt.Equals(DateTime.MinValue) || dt.Equals(DateTime.MaxValue)) return "null";
                return "'"+dt.ToString("yyyy-MM-ddTHH':'mm':'ss")+"'";  //dt.Year.ToString() + "-" + dt.Month.ToString().PadLeft(2, '0') + "-" + dt.Day.ToString().PadLeft(2, '0') + " " + dt.Hour.ToString().PadLeft(2, '0') + ":" + dt.Minute.ToString().PadLeft(2, '0') + ":" + dt.Second.ToString().PadLeft(2, '0');
                //if (dalSection.TipoDb.Equals(it.itryframework2.costanti.cosNomiTipiDb.SQLServer, StringComparison.InvariantCultureIgnoreCase))
                //{
                //    return "{ts '" + d2 + "'}";
                //}
                //else
                //{
                //    return "'"+d2+"'";
                //}
            }
            if (val is string)
            {
                string tmpVal = Convert.ToString(val);
                tmpVal = (dbAttr.DataSize > 0 && tmpVal.Length > dbAttr.DataSize) ? tmpVal.Substring(0, dbAttr.DataSize) : tmpVal;
                return (!dbAttr.UseNPrefix) ? "'" + tmpVal.Replace("'", "''") + "'" : "N'"+tmpVal.Replace("'", "''")+"'";
            }
            if (val is int || val is char) return Convert.ToString(val);
            if (val is bool)
            {
                //if (dalSection.TipoDb.Equals(it.itryframework2.costanti.cosNomiTipiDb.SQLServer, StringComparison.InvariantCultureIgnoreCase))
                //{
                //    return (Convert.ToBoolean(val) == true) ? "1" : "0";
                //}
                //return (Convert.ToBoolean(val) == true) ? "true" : "false";
                return (Convert.ToBoolean(val) == true) ? "1" : "0";
            }
            if (val is System.Enum) return Convert.ToString(Convert.ToInt32(val));

            return val.ToString();
        }
        #endregion

        #region _getDic
        private Dictionary<string, string> _getDic(IGenericObject obj) { return _getDic(obj, false); }
        /// <summary>
        /// Ritorna una dictionary con chiave pari al nome della colonna del db e value il valore della proprietà convertito a stringa.
        /// </summary>
        /// <param name="obj">IGenericObject</param>
        /// <param name="checkIfIgnorePKey"></param>
        /// <returns>System.Collections.Generic.Dictionary</returns>
        /// <exception cref="System.Exception">System.Exception</exception>
        private Dictionary<string, string> _getDic(IGenericObject obj, bool checkIfIgnorePKey)
        {
            StringBuilder sbColumns = new StringBuilder();
            StringBuilder sbValues = new StringBuilder();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            Type _type = obj.GetType();
            PropertyInfo[] props = _type.GetProperties();
            
            foreach (PropertyInfo pInfo in props)
            {
                object[] attributes = pInfo.GetCustomAttributes(typeof(DBAttributes),true);
                if (attributes.Length == 0) continue;
                foreach (object attr in attributes)
                {
                    DBAttributes dbAttr = (DBAttributes)attr;
                    string dbColName = dbAttr.DbColumnName; //nome reale della colonna nel db

                    if (dbColName.Equals(obj.PrimaryKey, StringComparison.InvariantCultureIgnoreCase))
                    {
                        dbColName = (checkIfIgnorePKey && dbAttr.IgnoreOnInsert ? "" : PREFISSO_PKEY) + pInfo.Name;
                        //dic.Add(PREFISSO_PKEY + pInfo.Name, Convert.ToString(pInfo.GetValue(obj, null)));
                        //una volta registrata la chiave passa alla prossima proprietà
                        //continue;
                    }
                    
                    string val = String.Empty;
                    try { val = _convert(pInfo.GetValue(obj, null), dbAttr); }
                    catch { throw; }
                    dic.Add(dbColName, val);
                }
            }
            return dic;
        }
        #endregion

        #region _executeSql
        /// <summary>
        /// Esegue la query passata.
        /// </summary>
        /// <param name="sql">query da eseguire</param>
        /// <param name="caller">il chiamante la funzione</param>
        /// <returns>System.Int32</returns>
        /// <exception cref="System.InvalidOperationException">System.InvalidOperationException</exception>
        private int _executeSql(string sql, string caller)
        {
            _setLogSql(caller,sql);

            IDbConnection cn = null;
            try { cn = _getConnManager().Open(); }
            catch (Exception ex)
            {
                if (cn != null) cn.Close();
                _writeLog(ex, sql);
                throw;
            }
            IDbCommand cmd = cn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            int result = 0;
            try
            {
                result = cmd.ExecuteNonQuery();
                cn.Close();
                return result;
            }
            catch (Exception ex2)
            {
                if (cn != null) cn.Close();
                throw;
            }
        }
        #endregion

        #region _getConnManager
        private ConnectionManager _getConnManager()
        {
            ConnectionManager cnMng = null;
            if (_isDefaultDBMapper)
            {
                cnMng = new ConnectionManager();
            }
            else
            {
                cnMng = new ConnectionManager(_connString, _providerAssemblyFullName, _isPwdEncrypted);
            }

            return cnMng;
        }
        #endregion

        #region _setLogSql
        private void _setLogSql(string caller, string sql)
        {
            if (_logConfig != null)
            {
                if (_logConfig.SqlToVsOutputWindow)
                {
                    System.Diagnostics.Debug.WriteLine((!string.IsNullOrEmpty(caller) ? caller : "") + ", executing sql:" + sql);
                }
            }
        }
        #endregion

        #region _fillTable
        private DataTable _fillTable(string sql, string caller)
        {
            _setLogSql(caller, sql);

            IDbConnection cn = null;
            try { cn = _getConnManager().Open(); }
            catch (Exception ex)
            {
                if (cn != null) cn.Close();
                _writeLog(ex, sql);
                throw;
            }

            IDbDataAdapter da = null;
            DataSet ds = new DataSet();

            string assemblyName = cn.GetType().Assembly.FullName;
            string typeName = cn.GetType().FullName.Replace("Connection", "DataAdapter");
            try
            {
                da = (IDbDataAdapter)Activator.CreateInstance(assemblyName, typeName).Unwrap();
            }
            catch (Exception ex1)
            {
                if (cn != null) cn.Close();
                _writeLog(ex1, sql);
                throw;
            }

            IDbCommand cmd = cn.CreateCommand();
            cmd.CommandText = sql;
            da.SelectCommand = cmd;
            
            try
            {
                da.Fill(ds);
                cn.Close();
                return (ds.Tables.Count > 0 && ds.Tables[0] != null) ? ds.Tables[0] : null;
            }
            catch { cn.Close(); throw; }
        }
        #endregion

        #region getStringWithQuery, getIntWithQuery, getBoolWithQuery
        /// <summary>
        /// Indicato per query di SELECT che restituiscono un solo valore.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public string getStringWithQuery(string sql)
        {
            return _getValueWithQuery<string>(sql);
        }
        /// <summary>
        /// Indicato per query di SELECT che restituiscono un solo valore.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public bool getBoolWithQuery(string sql)
        {
            return _getValueWithQuery<bool>(sql);
        }
        /// <summary>
        /// Indicato per query di SELECT che restituiscono un solo valore.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int getIntWithQuery(string sql)
        {
            return _getValueWithQuery<int>(sql);
        }
        private T _getValueWithQuery<T>(string sql)
        {
            if (string.IsNullOrEmpty(sql)) return default(T);

            object val = executeScalar(sql, false);
            if (val is DBNull) return default(T);
            if (val == null) return default(T);
            return (T)Convert.ChangeType(val, typeof(T));
        }
        #endregion

        #region _writeLog
        private void _writeLog(Exception ex, string customMessage)
        {
            _writeLog(ex, new string[] { customMessage });
        }
        private void _writeLog(Exception ex, string[] messages)
        {
            System.Text.StringBuilder s = new StringBuilder();
            foreach (string token in messages)
            {
                s.Append(token + "<br />");
            }
            it.itryframework2.managers.log.LogManager.trace(ex, s.ToString());
        }
        #endregion

        #region _getToken4LastInsertId
        private string _getToken4LastInsertId()
        {
            /*
             * vecchio codice preso dalla versione 1 di ITryFramework
             //if (ConfigManager.getDALConfigValues().TipoDb.Equals(it.itryframework2.costanti.cosNomiTipiDb.SQLServer, StringComparison.InvariantCultureIgnoreCase))
                //{
                //    sql += ";select @@identity";
                //}
                //else if (ConfigManager.getDALConfigValues().TipoDb.Equals(it.itryframework2.costanti.cosNomiTipiDb.MySqlServer, StringComparison.InvariantCultureIgnoreCase))
                //{
                //    sql += ";SELECT LAST_INSERT_ID()";
                //}

                
             */
            if (_DALConfig != null) _providerAssemblyFullName = _DALConfig.Provider;

            if (_providerAssemblyFullName.IndexOf("SqlConnection", StringComparison.InvariantCultureIgnoreCase) != -1) return ";select @@identity";
            else if (_providerAssemblyFullName.IndexOf("MySqlConnection", StringComparison.InvariantCultureIgnoreCase) != -1) return ";SELECT LAST_INSERT_ID()";

            

            return "";
        }
        #endregion

        #endregion

        #region public methods

        #region setObject
        /// <summary>
        /// Setta l'oggetto IObj passato con i valori contenuti nel reader passato.
        /// </summary>
        /// <param name="IObj">oggetto che implementa l'interfaccia IGenericObject e gli attributi DbAttributes</param>
        /// <param name="reader">reader valido</param>
        /// <exception cref="System.InvalidCastException">System.InvalidCastException</exception>
        public void setObject(IGenericObject IObj, IDataReader reader)
        {
            PropertyInfo[] pInfos = IObj.GetType().GetProperties();
            foreach (PropertyInfo pInfo in pInfos)
            {
                object[] arr = pInfo.GetCustomAttributes(typeof(DBAttributes), true);
                if (arr.Length == 0) continue;
                DBAttributes _attr = (DBAttributes)arr[0];   //ce n'è e ce ne deve essere uno solo
                if (!_readerContains(reader, _attr.DbColumnName)) continue;
                object val = reader[_attr.DbColumnName];
                if (val is DBNull) { val = null; }
                else
                {
                    try { val = Convert.ChangeType(val, pInfo.PropertyType); }
                    catch { throw; }
                }
                pInfo.SetValue(IObj, val, null);
            }
        } 
        #endregion

        #region insert
        /// <summary>
        /// Inserisce una riga nel database e ritorna l'id inserito (solo per database SQL SERVER, -1 per altri tipi di database).
        /// </summary>
        /// <param name="obj">IGenericObject</param>
        /// <returns>System.Int32</returns>
        /// <exception cref="System.Exception">System.Exception</exception>
        public int insert(IGenericObject obj)
        {
            StringBuilder sql = new StringBuilder("insert into " + obj.TableName + "(");
            StringBuilder sbColumns = new StringBuilder();
            StringBuilder sbValues = new StringBuilder();
            Dictionary<string, string> dic = null;
            try { dic = _getDic(obj,true); }   //dic contiene chiave>>nome colonna | valore>>valore campo
            catch  { throw; }

            bool hasPKey = false;   //se diventa true allora posso prendere l'ultimo id inserito quando eseguo la query
            foreach (KeyValuePair<string, string> keyVal in dic)
            {
                if (keyVal.Key.Contains(PREFISSO_PKEY))
                {
                    hasPKey = true;
                    continue;    //salto la chiave che ha prefisso key_
                }
                if (sbColumns.Length > 0) sbColumns.Append(",");
                if (sbValues.Length > 0) sbValues.Append(",");
                sbColumns.Append(keyVal.Key);
                sbValues.Append(keyVal.Value);
            }
            sql.Append(sbColumns.ToString() + ") values (" + sbValues.ToString() + ")");

            int result = -1;
            try { 
                result = Convert.ToInt32(executeScalar(sql.ToString(), hasPKey));
                return result;
            }
            catch (Exception ex) {
                _writeLog(ex, sql.ToString());
                throw; 
            }

        }
        #endregion

        #region update
        /// <summary>
        /// Aggiorna il database con l'oggetto passato.
        /// Ritorna True se vi sono state modifiche, False in caso contrario
        /// </summary>
        /// <param name="obj">IGenericObject</param>
        /// <returns>System.Boolean</returns>
        /// <exception cref="System.InvalidOperationException">System.InvalidOperationException</exception> 
        /// <exception cref="System.Exception">System.Exception</exception>
        public bool update(IGenericObject obj) { return update(obj, null); }

        /// <summary>
        /// Aggiorna il database con l'oggetto passato e con le clausole where indicate in System.Collections.Generic.Dictionary (nomeColonna,valoreColonna).
        /// Ritorna True se vi sono state modifiche, False in caso contrario.
        /// </summary>
        /// <param name="obj">IGenericObject</param>
        /// <param name="lstWhere">System.Collections.Generic.Dictionary in chiave una stringa contenente il valore della colonna, in value 
        /// solo int o string, per ogni altro valore lancia un'eccezione.</param>
        /// <returns>System.Boolean</returns>
        /// <exception cref="System.Exception">System.Exception</exception>
        public bool update(IGenericObject obj, Dictionary<string, object> lstWhere)
        {
            string pKeyVal = String.Empty;
            string pKeyName = String.Empty;
            StringBuilder sql = new StringBuilder("update " + obj.TableName + " set ");
            StringBuilder sqlPart = new StringBuilder();
            StringBuilder sbWhere = new StringBuilder(); //qui memorizzo tutte le chiavi primarie impostate sull'oggetto

            Dictionary<string, string> dic = null;
            try { dic = _getDic(obj); }   //dic contiene chiave>>nome colonna | valore>>valore campo
            catch  { throw; }

            foreach (KeyValuePair<string, string> keyVal in dic)
            {
                //salta il campo che corrisponde alla proprietà dell'interfaccia IGenericObject PrimaryKey
                if (keyVal.Key.Contains(PREFISSO_PKEY))
                {
                    //tolgo il prefisso
                    pKeyVal = keyVal.Key.Substring(PREFISSO_PKEY.Length) + "=" + keyVal.Value;
                    pKeyName = keyVal.Key;
                    continue;
                }

                if (sqlPart.Length > 0) sqlPart.Append(",");
                sqlPart.Append(keyVal.Key + " = " + keyVal.Value);
            }

            if (lstWhere == null)
            {
                sbWhere.Append(pKeyVal);
            }
            else
            {
                dic.Remove(pKeyName);
                //clausole where
                foreach (KeyValuePair<string, object> keyVal in lstWhere)
                {
                    if (sbWhere.Length > 0) sbWhere.Append(" and ");
                    if (keyVal.Value is int) sbWhere.Append(keyVal.Key + " = " + keyVal.Value);
                    else if (keyVal.Value is string) sbWhere.Append(keyVal.Key + " = '" + Convert.ToString(keyVal.Value).Replace("'", "''") + "'");
                    else throw new Exception("lstWhere accetta valori interi o stringa");
                }
            }

            sql.Append(sqlPart.ToString() + " where (" + sbWhere.ToString() + ")");
            int result = 0;
            try
            {
                result = _executeSql(sql.ToString(), "update");
                return (result == 1) ? true : false;
            }
            catch (Exception ex){ _writeLog(ex,sql.ToString());throw; }
        }
        #endregion

        #region delete
        /// <summary>
        /// Cancella una riga dal database.
        /// Ritorna True in caso di modifiche, False in caso contrario.
        /// </summary>
        /// <param name="obj">IGenericObject</param>
        /// <returns>System.Boolean</returns>
        /// <exception cref="System.Exception">System.Exception</exception>
        public bool delete(IGenericObject obj)
        {
            StringBuilder sql = new StringBuilder("DELETE FROM " + obj.TableName);
            string pKeyValue = String.Empty;

            Dictionary<string, string> dic = null;
            try { dic = _getDic(obj); }   //dic contiene chiave>>nome colonna | valore>>valore campo
            catch  { throw; }

            foreach (KeyValuePair<string, string> keyVal in dic)
            {
                if (!keyVal.Key.Contains(PREFISSO_PKEY)) continue;
                pKeyValue = keyVal.Key.Substring(PREFISSO_PKEY.Length) + "=" + keyVal.Value;
            }
            sql.Append(" WHERE (" + pKeyValue + ")");
            int result = 0;
            try
            {
                result = _executeSql(sql.ToString(),"delete");
                return (result == 1) ? true : false;
            }
            catch (Exception ex) { _writeLog(ex, sql.ToString()); throw; }
        }
        #endregion

        #region select
        /// <summary>
        /// Carica un oggetto che implementa l'interfaccia IGenericObject ed espone attributi di tipo DBAttributes.
        /// Nella composizione del comando sql da eseguire PKeyValue verrà trattato come stringa.
        /// Ritorna TRUE nel caso in cui sia stato riempito l'oggetto, FALSE in caso contrario.
        /// </summary>
        /// <param name="obj">IGenericObject.</param>
        /// <returns>TRUE se caricato, FALSE in caso contrario.</returns>
        /// <exception cref="System.InvalidCastException">System.InvalidCastException quando si settano le proprietà</exception>
        /// <exception cref="System.Exception">System.Exception</exception>
        public bool select(IGenericObject obj) { return _select(obj, null, null); }
        /// <summary>
        /// Carica un oggetto che implementa l'interfaccia IGenericObject ed espone attributi di tipo DBAttributes.
        /// La ricerca viene filtrata per il campo nomeCampoID passato.
        /// Nella composizione del comando sql da eseguire se il campo PKeyValue è di tipo string verrà trattato come stringa
        /// altrimenti verrà trattato come intero.
        /// Ritorna TRUE nel caso in cui sia stato riempito l'oggetto, FALSE in caso contrario.
        /// </summary>
        /// <param name="obj">IGenericObject.</param>
        /// <param name="nomeCampoId">nome del campo chiave.</param>
        /// <param name="PKeyValue">valore campo chiave.</param>
        /// <returns>TRUE se caricato, FALSE in caso contrario.</returns>
        /// <exception cref="System.InvalidCastException">System.InvalidCastException quando si settano le proprietà</exception>
        /// <exception cref="System.Exception">System.Exception</exception>
        public bool select(IGenericObject obj, string nomeCampoId, object PKeyValue) { return _select(obj, nomeCampoId, PKeyValue); }
        /// <summary>
        /// Restituisce un array di oggetti che implementano l'interfaccia IGenericObject.
        /// </summary>
        /// <typeparam name="T">oggetto che implementa l'interfaccia IGenericObject</typeparam>
        /// <param name="sql">query da eseguire</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">System.Exception</exception>
        public T[] select<T>(string sql) where T : IGenericObject
        {
            List<T> list = new List<T>();
            IDataReader reader = null;
            try
            {
                reader = getIDataReader(sql);
                while (reader.Read())
                {
                    T newT = Activator.CreateInstance<T>();
                    setObject(newT, reader);
                    list.Add(newT);
                }
                return (list.Count > 0) ? list.ToArray() : null;
            }
            catch (Exception ex) { _writeLog(ex, sql); throw; }
            finally { if (reader != null)reader.Close(); }
        }

        /// <summary>
        /// Ritorna un DataTable riempito in base alla stringa sql passata.
        /// </summary>
        /// <param name="sql">string sql da eseguire.</param>
        /// <returns>System.Data.DataTable</returns>
        /// <exception cref="System.Exception">System.Exception</exception>
        public DataTable select(string sql)
        {
            DataTable dt = null;
            try { dt = _fillTable(sql, "(get DataTable) select"); return dt; }
            catch (Exception ex){_writeLog(ex, sql);throw;}
        }
        /// <summary>
        /// Ritorna un DataTable. 
        /// Lancia un'eccezione se non è possibile inizializzare una connessione al database sottostante.
        /// Lancia un'eccezione in caso di errore nell'interrogazione al database.  
        /// </summary>
        /// <param name="obj">IGenericObject</param>
        /// <param name="orderBy">clausola order by o vuoto/nullo per nessun ordine</param>
        /// <returns>DataTable</returns>
        public DataTable select(IGenericObject obj, string orderBy)
        {
            return select(obj, null, null, orderBy);
        }

        public DataTable select(IGenericObject obj, Dictionary<string, object> dic, bool? useSearchAnd, string orderBy)
        {
            return select(obj, dic, useSearchAnd, true, orderBy);
        }

        /// <summary>
        /// Ritorna un DataTable. 
        /// </summary>
        /// <param name="obj">IGenericObject</param>
        /// <param name="dic">Dictionary contenente le clausole where da applicare. La forma è chiave == nomecolonna; valore == valore colonna.</param>
        /// <param name="useSearchAnd">TRUE per legare le clausole WHERE in AND, FALSE per legarle in OR.</param>
        /// <param name="useLike">True per usare l'operatore Like quando il value è stringa</param>
        /// <param name="orderBy">clausola order by o vuoto/nullo per nessun ordine</param>
        /// <returns>System.Data.DataTable</returns>
        /// <exception cref="System.Exception">System.Exception</exception>
        public DataTable select(IGenericObject obj, Dictionary<string, object> dic, bool? useSearchAnd, bool useLike, string orderBy)
        {
            StringBuilder sql = new StringBuilder("select * from " + obj.TableName);
            if (dic != null)
            {
                if (dic.Count > 0)
                {
                    StringBuilder sbValues = new StringBuilder();
                    string clause = "";
                    foreach (KeyValuePair<string, object> keyVal in dic)
                    {
                        if (sbValues.Length > 0) clause = ((bool)useSearchAnd) ? " and " : " or ";

                        if (keyVal.Value.GetType() == typeof(int))
                        {
                            sbValues.Append(clause + keyVal.Key + " = " + Convert.ToString(keyVal.Value).Replace(",", "."));
                        }
                        else if (keyVal.Value.GetType() == typeof(bool))
                        {
                            sbValues.Append(clause + keyVal.Key + " = ");
                            if (Convert.ToBoolean(keyVal.Value) == true) sbValues.Append("1");
                            else sbValues.Append("0");
                        }
                        else if (keyVal.Value.GetType() == typeof(DateTime))
                        {
                            DateTime date = Convert.ToDateTime(keyVal.Key);
                            string strDate = date.Year.ToString() + "-" + date.Month.ToString().PadLeft(2, '0') + "-" + date.Day.ToString().PadLeft(2, '0') + " " + date.Hour.ToString().PadLeft(2, '0') + ":" + date.Minute.ToString().PadLeft(2, '0') + ":" + date.Second.ToString().PadLeft(2, '0');
                            sbValues.Append(clause + keyVal.Key + " = " + "{ts '" + strDate + "'}");
                        }
                        else
                        {
                            if (useLike) sbValues.Append(clause + keyVal.Key + " like '%" + keyVal.Value.ToString().Replace("'", "''") + "%'");
                            else sbValues.Append(clause + keyVal.Key + " = '" + keyVal.Value.ToString().Replace("'", "''") + "'");
                        }
                    }
                    sql.Append(" where (" + sbValues.ToString() + ")");
                }
            }

            if (orderBy != null) if (!orderBy.Equals("")) sql.Append(" order by " + orderBy);
            
            DataTable dt = null;
            try { dt = _fillTable(sql.ToString(),"(get DataTable) select"); return dt; }
            catch (Exception ex) {_writeLog(ex, sql.ToString());throw;}
            
        }
        #endregion

        #region executeTranSql
        /// <summary>
        /// Esegue le istruzioni sql contenute nell'array passato in un unica transazione.
        /// Il livello di isolamento utilizzato è quello di default.
        /// Registra eventuali eccezioni e le rilancia.
        /// </summary>
        /// <param name="arrSql">array di stringhe sql da eseguire</param>
        /// <exception cref="System.InvalidOperationException">System.InvalidOperationException</exception>
        /// <exception cref="System.Exception">System.Exception</exception>
        public void executeTranSql(string[] arrSql)
        {
            _setLogSql("executeTranSql", string.Join(",", arrSql));

            IDbConnection cn = null;
            IDbTransaction tran = null;
            IDbCommand cmd = null;
            try { cn = _getConnManager().Open(); }
            catch (Exception ex)
            {
                if (cn != null) cn.Close();
                _writeLog(ex, arrSql);
                throw;
            }
            try
            {
                cmd = cn.CreateCommand();
                tran = cn.BeginTransaction();
                cmd.Transaction = tran;
                foreach (string sql in arrSql)
                {
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
                tran.Commit();
            }
            catch (Exception ex2)
            {
                if (tran != null) tran.Rollback();
                if (cn != null) cn.Close();
                _writeLog(ex2, arrSql);
                throw;
            }

        } 
        #endregion

        #region getIDataReader
        /// <summary>
        /// Restituisce un IDataReader data una query valida.
        /// Non registra eventuali eccezioni, si limita a rilanciarle.
        /// </summary>
        /// <param name="sql">query sql</param>
        /// <returns>System.Data.IDataReader</returns>
        /// <exception cref="System.Exception">System.Exception</exception>
        public IDataReader getIDataReader(string sql)
        {
            _setLogSql("getIDataReader", sql);

            IDbConnection cn = null;
            try {
                cn=_getConnManager().Open();
            }
            catch (Exception ex)
            {
                if (cn != null) cn.Close();
                _writeLog(ex, sql);
                throw;
            }
            IDbCommand cmd = cn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            IDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex2)
            {
                if (cn != null) cn.Close();
                _writeLog(ex2, sql);
                throw;
            }

            return reader;
        } 
        #endregion

        #region executeScalar
        /// <summary>
        /// Esegue l'istruzione passata. 
        /// In operazioni di insert, se getLastInsertedID è uguale a True restituisce l'ultimo id inserito (solo per database SQL SERVER e MySql).
        /// Non registra eventuali eccezioni, si limita a rilanciarle. 
        /// </summary>
        /// <param name="sql">string sql da eseguire</param>
        /// <param name="getLastInsertedID">In operazioni di insert ritorna l'ultimo ID inserito se il parametro è uguale a True.
        /// (solo per database SQL SERVER e MySql)</param>
        /// <returns>System.Object</returns>
        /// <exception cref="System.Exception">System.Exception</exception>
        public  object executeScalar(string sql, bool getLastInsertedID)
        {
            _setLogSql("executeScalar", sql);

            IDbConnection cn = null;
            try { cn = _getConnManager().Open(); }
            catch (Exception ex)
            {
                if (cn != null) cn.Close();
                _writeLog(ex, sql);
                throw;
            }
            if (getLastInsertedID)
            {
                sql += _getToken4LastInsertId();

            }

            IDbCommand cmd = cn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            object retObj = null;
            try
            {
                retObj = cmd.ExecuteScalar();
                cn.Close();
                return retObj;
            }
            catch (Exception ex2)
            {
                if (cn != null) cn.Close();
                _writeLog(ex2, sql);
                throw;
            }
        } 
        #endregion

        #region executeNonQuery
        /// <summary>
        /// Esegue l'istruzione sql passata e restituisce il numero di righe interessate.
        /// </summary>
        /// <param name="sql">istruzione sql</param>
        /// <returns>System.Int32</returns>
        /// <exception cref="System.InvalidOperationException">System.InvalidOperationException</exception>
        /// <exception cref="System.Exception">System.Exception</exception>
        public int executeNonQuery(string sql)
        {
            _setLogSql("executeNonQuery", sql);

            IDbConnection cn = null;
            try { cn = _getConnManager().Open(); }
            catch (Exception ex)
            {
                if (cn != null) cn.Close();
                _writeLog(ex, sql);
                throw;
            }
            IDbCommand cmd = cn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            int ret = 0;
            try
            {
                ret = cmd.ExecuteNonQuery();
                cn.Close();
                return ret;
            }
            catch (Exception ex2)
            {
                if (cn != null) cn.Close();
                _writeLog(ex2, sql);
                throw;
            }
        } 
        #endregion

        #endregion

    }
}
