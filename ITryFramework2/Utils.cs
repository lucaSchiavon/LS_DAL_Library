using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace it.itryframework2
{
    public class Utils
    {
        public Utils() { }

        /// <summary>
        /// Codifica MD5 della stringa passata.
        /// </summary>
        /// <param name="input">stringa da codificare</param>
        /// <returns></returns>
        public string getMd5Hash(string input)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++) { sBuilder.Append(data[i].ToString("x2")); }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        /// <summary>
        /// Verifica dell'uguaglianza tra la stringa in entrata ed il codice hash passato.
        /// </summary>
        /// <param name="input">stringa input</param>
        /// <param name="hash">hash da verificare</param>
        /// <returns></returns>
        public bool verifyMd5Hash(string input, string hash)
        {
            // Hash the input.
            string hashOfInput = getMd5Hash(input);

            // Create a StringComparer an comare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash)) return true;
            return false;
        }
        
        /// <summary>
        /// Data una stringa controlla se è un'email ritornando True in caso di successo, False in caso contrario.
        /// </summary>
        /// <param name="email">email da controllare</param>
        /// <returns></returns>
        public bool isValidEmail(string email)
        {
            Regex emailregex = new Regex(@"^[a-zA-Z][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$");
            Match m = emailregex.Match(email);
            if (!m.Success) return false;
            return true;
        }
    }

}
