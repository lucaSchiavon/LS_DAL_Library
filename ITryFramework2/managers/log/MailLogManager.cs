using System;
using System.Collections.Generic;
using System.Text;
using it.itryframework2.config;

namespace it.itryframework2.managers.log
{
    public sealed class MailLogManager : it.itryframework2.interfaces.ILog
    {
        #region ILog Membri di

        public bool manageError(it.itryframework2.config.ErrorConfigurationSection errorSection, Exception ex, string customMessage)
        {
            StringBuilder s = new StringBuilder();
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            if (context != null)
            {
                s.Append("<b>Sito</b><br><u>" + context.Request.ServerVariables["SERVER_NAME"] + "</u><br>");
                s.Append("<br><b>Pagina richiesta</b><br><u>" + context.Request.RawUrl + "</u><br>");
                s.Append("<br><b>IP richiedente</b><br>" + context.Request.ServerVariables["REMOTE_ADDR"] + "<br>");
                s.Append("<br><b>Host name</b><br>" + context.Request.ServerVariables["REMOTE_HOST"] + "<br>");
           }
            s.Append("<br><b>Message</b>:<br>" + ex.Message + "<br>");
            s.Append("<br><b>Source</b>:<br>" + ex.Source + "<br>");
            s.Append("<br><b>Stack</b>:<br>"+ex.StackTrace+"<br>");
            s.Append("<br><b>CustomMessage</b>:<br>"+customMessage);
            it.itryframework2.managers.mail.MailManager mailMng = new it.itryframework2.managers.mail.MailManager(errorSection.Smtp,null);
            string[] arrBcc=null;
            if (!string.IsNullOrEmpty(errorSection.MailBcc)) 
            {
                string bccs=errorSection.MailBcc;
                if (bccs.IndexOf(",") != -1) arrBcc = bccs.Split(new char[] { ',' });
                else arrBcc = new string[1] { bccs };
            }

            mailMng.send(errorSection.MailFrom
                        , errorSection.MailFrom
                        , errorSection.MailTo
                        , errorSection.MailTo
                        , arrBcc
                        , null
                        , (context != null ? context.Request.ServerVariables["SERVER_NAME"].ToUpper() : System.Reflection.Assembly.GetExecutingAssembly().FullName) + " - errore catturato da ITryFramework"
                        , s.ToString()
                        , (string.IsNullOrEmpty(errorSection.AuthenticationPwd) ? null : errorSection.AuthenticationPwd)
                        ,null);

            return true;
        }

        #endregion
    }
}
