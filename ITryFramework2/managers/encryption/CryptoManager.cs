using System;
using System.Collections.Generic;
using System.Text;
using it.itryframework2.config;
using System.Security.Cryptography;
using System.IO;

namespace it.itryframework2.managers.encryption
{
    public class CryptoManager
    {

        private string m_key = "_$!9_?#_";
        private byte[] b4 = { 251, 255, 96, 111, 124, 180, 209, 255 }; // any 8 numbers(255 is max)

        public virtual string setEncrypt(string phrase)
        {
            byte[] key = System.Text.Encoding.UTF8.GetBytes(m_key.ToCharArray(), 0, 8);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByte = Encoding.UTF8.GetBytes(phrase);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(key, b4), CryptoStreamMode.Write);
            cs.Write(inputByte, 0, inputByte.Length);
            cs.FlushFinalBlock();
            return Convert.ToBase64String(ms.ToArray());
        }

        public virtual string getDecrypted(string connstring)
        {
            #region recupero della parte criptata
            //m_dalSection = (DALConfigurationSection)System.Configuration.ConfigurationManager.GetSection("ITryFrameworkDALConfig");
            string key = "password=";
            int pos = connstring.IndexOf(key, StringComparison.InvariantCultureIgnoreCase);
            if (pos == -1)
            {
                key = "pwd=";
                pos = connstring.IndexOf(key, StringComparison.InvariantCultureIgnoreCase);
            }
            if (pos == -1) throw new ArgumentException("connstring in ITryFrameworkDALConfig, chiave password o chiave pwd non trovata");
            pos = pos + key.Length;
            int posEnd = connstring.IndexOf(";", pos);
            string encPwd = (posEnd == -1 ? connstring.Substring(pos) : connstring.Substring(pos, posEnd));            
            #endregion

            #region decriptazione
            byte[] inputByte = new byte[encPwd.Trim().Length + 1];
            byte[] bKey = System.Text.Encoding.UTF8.GetBytes(m_key.ToCharArray(), 0, 8);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            inputByte = Convert.FromBase64String(encPwd.Trim());
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(bKey, b4), CryptoStreamMode.Write);
            cs.Write(inputByte, 0, inputByte.Length);
            cs.FlushFinalBlock();
            System.Text.Encoding encoding = System.Text.Encoding.UTF8; 
            #endregion

            return connstring.Replace(encPwd, encoding.GetString(ms.ToArray()));
        }
    }
}
