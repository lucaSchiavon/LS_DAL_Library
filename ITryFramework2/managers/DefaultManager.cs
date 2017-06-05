using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Data;
using it.itryframework2;
using it.itryframework2.data;
using it.itryframework2.managers.connection;

namespace it.itryframework2.managers
{
    public class DefaultManager<T> where T : it.itryframework2.interfaces.IGenericObject
    {
        private DBMapper _dbMapper = null;
        public DBMapper DBMapper
        {
            get { return _dbMapper; }
        }

        public DefaultManager() {
            _dbMapper = new DBMapper();
        }
        public DefaultManager(string connString, string providerAssemblyFullName)
        {
            _dbMapper = new DBMapper(connString, providerAssemblyFullName, false);
        }
        public DefaultManager(string connString, string providerAssemblyFullName, bool isPwdEncrypted) 
        {
            _dbMapper = new DBMapper(connString, providerAssemblyFullName, isPwdEncrypted);
        }

        /// <summary>
        /// Riempie l'oggetto con i dati prelevati dal db.
        /// </summary>
        /// <param name="tClass">oggetto</param>
        /// <returns></returns>
        public virtual bool select(T tClass) { return _dbMapper.select(tClass); }
        /// <summary>
        /// Riempie l'oggetto con i dati prelevati dal db.
        /// Le eccezioni dovranno essere gestite dal chiamante. 
        /// </summary>
        /// <param name="tClass">oggetto</param>
        /// <param name="pKeyFieldName">nome della colonna che contiene la chiave primaria</param>
        /// <param name="id">valore della chiave primaria</param>
        /// <returns></returns>
        public virtual bool select(T tClass, string pKeyFieldName, string id)
        {
            return _dbMapper.select(tClass, pKeyFieldName, id);
        }

        /// <summary>
        /// Riempie l'oggetto con i dati prelevati dal db.
        /// Le eccezioni dovranno essere gestite dal chiamante. 
        /// </summary>
        /// <param name="tClass">oggetto</param>
        /// <param name="pKeyFieldName">nome della colonna che contiene la chiave primaria</param>
        /// <param name="id">valore della chiave primaria</param>
        /// <returns></returns>
        public virtual bool select(T tClass, string pKeyFieldName, int id)
        {
            return _dbMapper.select(tClass, pKeyFieldName, id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public virtual T[] select(string sql)
        {
            return _dbMapper.select<T>(sql);
        }

        /// <summary>
        /// Cancella dal db l'oggetto passato.
        /// Le eccezioni dovranno essere gestite dal chiamante. 
        /// </summary>
        /// <param name="tClass">oggetto da cancellare</param>
        /// <returns></returns>
        public virtual bool delete(T tClass)
        {
            return _dbMapper.delete(tClass);
        }

        /// <summary>
        /// Aggiorna il db con i dati contenuti in oggetto passato.
        /// Le eccezioni dovranno essere gestite dal chiamante.
        /// </summary>
        /// <param name="tClass">oggetto</param>
        /// <returns></returns>
        public virtual bool update(T tClass)
        {
            return _dbMapper.update(tClass);
        }

        /// <summary>
        /// Aggiorna il db con i dati contenuti in oggetto passato e con le clausole where contenute in dictionary
        /// (chiave ==> nome della colonna, valore ==> (tipo object)valore di tipo int o string.
        /// Le eccezioni dovranno essere gestite dal chiamante.
        /// </summary>
        /// <param name="tClass">oggetto</param>
        /// <param name="dicWhere">dictionary contenente nomi colonne e valori per le clausole where</param>
        /// <returns></returns>
        public virtual bool update(T tClass, Dictionary<string, object> dicWhere)
        {
            return _dbMapper.update(tClass, dicWhere);
        }

        /// <summary>
        /// Inserisce in db l'oggetto passato e restituisce l'id della riga inserita.
        /// (per database Access non ritorna l'ultimo id inserito)
        /// Le eccezioni dovranno essere gestite dal chiamante. 
        /// </summary>
        /// <param name="tClass"></param>
        /// <returns></returns>
        public virtual int insert(T tClass)
        {
            return _dbMapper.insert(tClass);
        }

        #region getStringWithQuery, getIntWithQuery, getBoolWithQuery
        /// <summary>
        /// Indicato per query di SELECT che restituiscono un solo valore.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public string getStringWithQuery(string sql)
        {
            return _dbMapper.getStringWithQuery(sql);
        }
        /// <summary>
        /// Indicato per query di SELECT che restituiscono un solo valore.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public bool getBoolWithQuery(string sql)
        {
            return _dbMapper.getBoolWithQuery(sql);
        }
        /// <summary>
        /// Indicato per query di SELECT che restituiscono un solo valore.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public int getIntWithQuery(string sql)
        {
            return _dbMapper.getIntWithQuery(sql);
        }
        #endregion
    }
}


