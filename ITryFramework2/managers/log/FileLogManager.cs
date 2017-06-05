using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace it.itryframework2.managers.log
{
    public sealed class FileLogManager : it.itryframework2.interfaces.ILog
    {
        private string m_logFolderPath = "";

        public FileLogManager() {}

        public FileLogManager(string logFolderPath) { m_logFolderPath = logFolderPath; }
        
        #region ILog Membri di

        public bool manageError(it.itryframework2.config.ErrorConfigurationSection errorSection, Exception ex, string customMessage)
        {
            StringBuilder s = new StringBuilder();
            if (ex == null && string.IsNullOrEmpty(customMessage))
            {
                s.AppendLine("Nessuna informazione passata.");
            }
            else
            {
                if (ex != null)
                {
                    s.AppendLine("Message: " + ex.Message);
                    s.AppendLine("Source: " + ex.Source);
                    s.AppendLine("Stack trace: " + ex.StackTrace);
                    s.AppendLine("TargetSite: " + ex.TargetSite.Name);
                }
                if (!string.IsNullOrEmpty(customMessage))
                {
                    s.AppendLine("Custom message: " + customMessage);
                }
            }
            s.Append("##################################################");

            StreamWriter sw = null;
            DirectoryInfo dir = new DirectoryInfo(m_logFolderPath);
            if (!dir.Exists) throw new Exception("Specified directory does not exists.");
            string nomeFile = DateTime.Now.Year.ToString("yyyy") + DateTime.Now.Month.ToString("MM") + DateTime.Now.Day.ToString("dd") + ".log";
            if (!File.Exists(m_logFolderPath + "\\" + nomeFile)) sw = File.CreateText(m_logFolderPath + "\\" + nomeFile);
            else sw = File.AppendText(m_logFolderPath + "\\" + nomeFile);

            sw.Write(s.ToString());
            sw.Close();
            sw.Dispose();
            return true;
        }

        #endregion

    }
}
