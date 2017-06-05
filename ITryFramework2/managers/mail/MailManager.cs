using System;
using System.Configuration;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using it.itryframework2.interfaces;
using it.itryframework2.config;
using System.Net.Mime;
using System.IO;

namespace it.itryframework2.managers.mail
{
    public class MailManager
    {
        private string m_smtp;
        /// <summary>
        /// Ottiene l'smtp inserito nel file config.
        /// </summary>
        public string SMTP
        {
            get { return m_smtp; }
        }

        private string m_authenticationPwd;
        /// <summary>
        /// Restituisce la password di autenticazione, se inserita nel config.
        /// </summary>
        public string AuthenticationPwd
        {
            get { return m_authenticationPwd; }
        }

        /// <summary>
        /// Il costruttore di default legge da config il valore per l'SMTP.
        /// </summary>
        public MailManager()
        {
            MailConfigurationSection mailSection = it.itryframework2.managers.config.ConfigManager.getMailConfigValues();
            this.m_smtp = mailSection.Smtp;
            this.m_authenticationPwd = mailSection.AuthenticationPwd;
        }

        /// <summary>
        /// Istanzia un nuovo oggetto MailManager con l'smtp passato
        /// </summary>
        /// <param name="smtp">nome server Smtp</param>
        public MailManager(string smtp, string authenticationPwd)
        {
            this.m_smtp = smtp;
            this.m_authenticationPwd = authenticationPwd;
        }

        /// <summary>
        /// Invia una mail in base ai parametri passati.
        /// Le eccezioni non sono gestite.
        /// </summary>
        /// <param name="from">mail mittente</param>
        /// <param name="to">mail destinatario</param>
        /// <param name="subject">oggetto della mail</param>
        /// <param name="body">corpo della mail</param>
        public void send(string from, string to, string subject, string body)
        {
            _send(from, null, to, null, null, null, subject, body, null, null);
        }
        /// <summary>
        /// Invia una mail in base ai parametri passati.
        /// Le eccezioni non sono gestite.
        /// </summary>
        /// <param name="from">mail mittente</param>
        /// <param name="to">mail destinatario</param>
        /// <param name="bcc">array di mail in bcc</param>
        /// <param name="subject">oggetto della mail</param>
        /// <param name="body">corpo della mail</param>
        public void send(string from, string to, string[] bcc, string subject, string body)
        {
            _send(from, null, to, null, bcc, null, subject, body, null, null);
        }
        /*
         16.11.2012 Federico Torioli
         * rimuovo questo overload perchè va in conflitto con questo metodo
         * public void send(string from, string to, string[] bcc, string subject, string body)
         * nel caso in cui l'array sia passato come null
         */
        ///// <summary>
        ///// Invia una mail in base ai parametri passati.
        ///// Le eccezioni non sono gestite.
        ///// </summary>
        ///// <param name="from">mail mittente</param>
        ///// <param name="fromName">nome mittente</param>
        ///// <param name="to">mail destinatario</param>
        ///// <param name="subject">oggetto della mail</param>
        ///// <param name="body">corpo della mail</param>
        //public void send(string from, string fromName, string to, string subject, string body)
        //{
        //    send(from, fromName, to, null, null, subject, body);
        //}
        /// <summary>
        /// Invia una mail in base ai parametri passati.
        /// Le eccezioni non sono gestite.
        /// </summary>
        /// <param name="from">mail mittente</param>
        /// <param name="fromName">nome mittente</param>
        /// <param name="to">mail destinatario</param>
        /// <param name="toName">nome destinatario</param>
        /// <param name="subject">oggetto della mail</param>
        /// <param name="body">corpo della mail</param>
        public void send(string from, string fromName, string to, string toName, string subject, string body)
        {
            _send(from, fromName, to, toName, null, null, subject, body, null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from">mail mittente</param>
        /// <param name="fromName">nome mittente</param>
        /// <param name="to">mail destinatario</param>
        /// <param name="toName">nome destinatario</param>
        /// <param name="subject">oggetto della mail</param>
        /// <param name="body">corpo della mail</param>
        /// <param name="authenticate">se true legge la password alla chiave AuthenticationPwd nella sezione errorconfiguration</param>
        public void send(string from, string nameFrom, string to, string toName, string[] bcc, string emailReplyTo, string subject, string body, bool authenticate)
        {
            if (authenticate && string.IsNullOrEmpty(m_authenticationPwd))
            {
                throw new it.itryframework2.exception.ITryFrameworkException("E' stato scelto di usare l'autenticazione con password per l'invio della mail ma non è stata specificata alcuna password nella sezione MailConfigurationSection");
            }
            _send(from, nameFrom, to, toName, bcc, emailReplyTo, subject, body, (authenticate ? m_authenticationPwd : null), null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="nameFrom"></param>
        /// <param name="to"></param>
        /// <param name="toName"></param>
        /// <param name="bcc"></param>
        /// <param name="emailReplyTo"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="pwdAuthentication"></param>
        public void send(string from, string nameFrom, string to, string toName, string[] bcc, string emailReplyTo, string subject, string body, string pwdAuthentication, string[] attachments)
        {
            _send(from, nameFrom, to, toName, bcc, emailReplyTo, subject, body, pwdAuthentication, attachments);
        }

        //internal void _send(string from, string nameFrom, string to, string toName, string[] bcc, string emailReplyTo, string subject, string body, string pwdAuthentication, string[] attachments)
        //{
        //    _send(from, nameFrom, to, toName, null, null, subject, body, pwdAuthentication, attachments);
        //}
        /// <summary>
        /// Invia una mail in base ai parametri passati.
        /// Le eccezioni non sono gestite.
        /// </summary>
        /// <param name="from">mail mittente</param>
        /// <param name="nameFrom">nome mittente</param>
        /// <param name="to">mail destinatario</param>
        /// <param name="toName">nome destinatario</param>16.
        /// <param name="bcc">array di mail in bcc</param>
        /// <param name="subject">oggetto della mail</param>
        /// <param name="body">corpo della mail</param>
        private void _send(string from, string nameFrom, string to, string toName, string[] bcc, string emailReplyTo, string subject, string body, string pwdAuthentication, string[] attachments)
        {
            if (string.IsNullOrEmpty(nameFrom)) nameFrom = from;
            if (string.IsNullOrEmpty(toName)) toName = to;

            MailAddress _from = new MailAddress(from, nameFrom);
            MailAddress _to = new MailAddress(to, toName);
            MailMessage mailMsg = new MailMessage(_from, _to);
            if (bcc != null && bcc.Length > 0)
            {
                for (int i = 0; i < bcc.Length; i++)
                {
                    mailMsg.Bcc.Add(new MailAddress(bcc[i], bcc[i]));
                }
            }
            if (!string.IsNullOrEmpty(emailReplyTo)) mailMsg.ReplyTo = new MailAddress(emailReplyTo);
            mailMsg.Subject = subject;
            mailMsg.IsBodyHtml = true;
            mailMsg.Body = body;

            if (attachments != null)
            {
                for (int i = 0; i < attachments.Length; i++)
                {
                    string fileName = attachments[i];
                    Attachment attachment = new Attachment(fileName, MediaTypeNames.Application.Octet);
                    ContentDisposition disposition = attachment.ContentDisposition;
                    disposition.FileName = Path.GetFileName(fileName);
                    disposition.Size = new FileInfo(fileName).Length;
                    disposition.DispositionType = DispositionTypeNames.Attachment;
                    mailMsg.Attachments.Add(attachment);
                }

            }

            SmtpClient smtp = new SmtpClient(m_smtp); //caricato dal costruttore della classe
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            if (string.IsNullOrEmpty(pwdAuthentication))
            {
                smtp.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
            }
            else
            {
                smtp.Credentials = new System.Net.NetworkCredential(from, pwdAuthentication);
            }

            smtp.Send(mailMsg);
        }
    }

}