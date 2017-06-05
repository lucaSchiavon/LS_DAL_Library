using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Collections.Generic;
using System.Text;
using it.itryframework2.exception;
using it.itryframework2.config;
using it.itryframework2.costanti;

namespace it.itryframework2.managers.connection
{
    public sealed class ConnectionManager
    {
        DALConfigurationSection _dalConfig = null;
        string _connString = null, _providerAssemblyFullName=null;
        bool _decriptPwd;

        /// <summary>
        /// Ottiene il tipo di database a cui è associata la connessione.
        /// </summary>
        //public static string getDbType()
        //{
        //    return m_dalSection.TipoDb;
        //}

        
        public ConnectionManager()
        {
            _loadDalConfig();
        }
        public ConnectionManager(string connString, string providerAssemblyFullName)
        {
            _connString = connString;
            _providerAssemblyFullName = providerAssemblyFullName;
        }
        public ConnectionManager(string connString, string providerAssemblyFullName, bool decriptPwd)
        {
            _connString = connString;
            _providerAssemblyFullName = providerAssemblyFullName;
            _decriptPwd = decriptPwd;
        }
        private void _loadDalConfig()
        {
            try
            {
                _dalConfig = it.itryframework2.managers.config.ConfigManager.getDALConfigValues();
            }
            catch (ITryFrameworkException itex)
            {
                throw itex;
            }
        }

        /// <summary>
        /// Legge i parametri dal web.config ed una apre una connessine al database indicato.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.Exception">System.Exception</exception>
        public IDbConnection Open()
        {
            IDbConnection cn = null;
            if (_dalConfig != null)
            {
                _providerAssemblyFullName = _dalConfig.Provider;
                _connString = _dalConfig.ConnString;
                _decriptPwd = _dalConfig.DecryptPwd;
            }

            try
            {
                cn = (IDbConnection)Activator.CreateInstance(Type.GetType(_providerAssemblyFullName));
            }
            catch
            {
                throw;
            }

            cn.ConnectionString = _connString;

            if (_decriptPwd)
            {
                it.itryframework2.managers.encryption.CryptoManager cryptoMng = null;
                //if (string.IsNullOrEmpty(_dalConfig.ManagerCryptoName))
                //{
                    //usa il default
                cryptoMng = new it.itryframework2.managers.encryption.CryptoManager();
                //}
                //else
                //{
                    //usa quello definito
                    //System.Runtime.Remoting.ObjectHandle objHandle = Activator.CreateInstance(AppDomain.CurrentDomain,"testweb.manager", "MyCriptoManager");
                    //System.Runtime.Remoting.ObjectHandle objHandle = AppDomain.CurrentDomain.CreateInstance("MyCriptoManager", m_dalSection.ManagerCryptoName);   
                    //cryptoMng = (it.itryframework2.managers.encryption.CryptoManager)objHandle.Unwrap();



                //}

                cn.ConnectionString = cryptoMng.getDecrypted(cn.ConnectionString);
            }


            try
            {
                cn.Open();
                return cn;
            }
            catch (Exception ex)
            {
                if (cn != null)cn.Close();
                cn = null;
                throw;
            }
            
        }
    }
}
