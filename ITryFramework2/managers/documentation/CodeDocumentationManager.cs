using System;
using System.Collections.Generic;
using System.Web;

namespace it.itryframework.managers.documentation
{
    public class CodeDocumentationManager : System.Web.IHttpHandler
    {
        string dirToScan = "/App_Code";
        string userWantsDir = "";

        public CodeDocumentationManager()
        {

        }

        #region IHttpHandler Membri di

        bool IHttpHandler.IsReusable
        {
            get { return true; }
        }

        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            HttpContext.Current.Response.ContentType = "text/html";
            HttpContext.Current.Response.Charset = "UTF-8";

            if (!string.IsNullOrEmpty(context.Request["dir"]))
            {
                userWantsDir = "/"+context.Request["dir"];
            }

            System.Text.StringBuilder s = new System.Text.StringBuilder();
            s.Append("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
            s.Append("<html xmlns=\"http://www.w3.org/1999/xhtml\"><title>Code documentation for "+context.Request.ServerVariables["SERVER_NAME"]+" </title><head>"+_getCssStyle());
            s.Append("<body><div class=\"container\">");
            _setDocumentationFile(s,dirToScan+userWantsDir);
            s.Append("</div></body>");
            s.Append("</head></html>");
            context.Response.Write(s.ToString());

            userWantsDir = "";
        }

        #endregion

        #region _getCssColors
        private string _getCssColors(string line)
        {
            System.Text.StringBuilder sb=new System.Text.StringBuilder();
            string tempLine = HttpContext.Current.Server.HtmlDecode(line);
            if (tempLine.StartsWith("///") && tempLine.EndsWith(">"))
            {
                sb.Append("<span class=\"grey\">" + line + "</span>");
            }
            else if (tempLine.StartsWith("///") && !tempLine.EndsWith(">"))
            {
                sb.Append("<span class=\"green\">" + line + "</span>");
            }
            else
            {
                line = line.Replace("public", _getSpanCss("public", "blue"))
                    .Replace("protected", _getSpanCss("protected", "blue"))
                    .Replace("override", _getSpanCss("override", "blue"))
                    .Replace("virtual", _getSpanCss("virtual", "blue"))
                    .Replace("abstract", _getSpanCss("abstract", "blue"))
                    .Replace("void", _getSpanCss("void", "blue"))
                    .Replace("int", _getSpanCss("int", "blue"))
                    .Replace("int[]", _getSpanCss("int[]", "blue"))
                    .Replace("byte", _getSpanCss("byte", "blue"))
                    .Replace("byte[]", _getSpanCss("byte[]", "blue"))
                    .Replace("bool", _getSpanCss("bool", "blue"))
                    .Replace("bool[]", _getSpanCss("bool[]", "blue"))
                    .Replace("string", _getSpanCss("string", "blue"))
                    .Replace("string[]", _getSpanCss("string[]", "blue"))
                    .Replace("long", _getSpanCss("long", "blue"))
                    .Replace("long[]", _getSpanCss("long[]", "blue"))
                    .Replace("static", _getSpanCss("static", "blue"))
                    .Replace("base", _getSpanCss("base", "blue"))
                    .Replace("enum", _getSpanCss("enum", "blue"))
                    .Replace("String", _getSpanCss("String", "azure"))
                    .Replace("DateTime", _getSpanCss("DateTime", "azure"))
                    .Replace("Dictionary", _getSpanCss("Dictionary", "azure"));


                sb.Append(line);

            }
            return (sb.Length == 0 ? sb.Append(line).ToString() : sb.ToString());
        }
        #endregion


        private string _getSpanCss(string val,string spanColor)
        {
            return "<span class=\""+spanColor+"\">"+val+"</span>";
        }

        private string _getCssStyle()
        {
            return "<style>"+
                   ".blue{ color:#00f;padding:0 3px;}"+
                   ".azure{ color:#1982D1;padding:0 3px;}" +
                   ".grey{ color:#999;padding:0 3px;}" +
                   ".green{ color:#008000;padding:0 3px;}" +
                   "h2{ margin-bottom:2px;border:1px solid #999;}" +
                   "</style>";
        }


        private void _setDocumentationFile(System.Text.StringBuilder s,string percorso)
        {
            string[] dirs = System.IO.Directory.GetDirectories(HttpContext.Current.Server.MapPath(percorso));
            if (dirs != null && dirs.Length > 0)
            {
                string path = "";
                for (int i = 0; i < dirs.Length; i++)
                {
                    path = dirs[i].Substring(dirs[i].IndexOf("App_Code"));
                    _setDocumentationFile(s, path);
                }
            }
            else
            {
                //string line = null;
                string[] arrFile = System.IO.Directory.GetFiles(HttpContext.Current.Server.MapPath(percorso));
                foreach (string file in arrFile)
                {
                    s.Append("<div class=\"code_box\">");
                    s.Append("<h2>" + System.IO.Path.GetFileNameWithoutExtension(file.Substring(file.LastIndexOf("\\") + 1)) + "</h2>");
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(file))
                    {
                        //string tag = _setTag(null, _getXMLDocSection(reader), false);
                        //if (!string.IsNullOrEmpty(tag)) s.Append(tag);

                        s.Append(_getXMLDocSection(reader));
                        reader.Close();
                    }
                    s.Append("</div>");
                }
            }
        }

        private string _setTag(string tagName, string tagValue, bool useCData)
        {
            if (string.IsNullOrEmpty(tagValue)) return null;
            string tag = "";
            if (!string.IsNullOrEmpty(tagName)) tag="<" + tagName.ToLower() + ">";
            tag += (!useCData ? tagValue : "<![CDATA[" + tagValue + "]]>");
            if (!string.IsNullOrEmpty(tagName)) tag += "</" + tagName.ToLower() + ">";
            return tag;
        }
        private string _getXMLDocSection(System.IO.StreamReader reader)
        {
            string line = null;
            bool isDivOpenSummaryAdded = false;
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            try
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line)) continue;
                    line = line.Trim();
                    

                    if (line.StartsWith("///"))
                    {
                        if (line.Equals("/// <summary>"))
                        {
                            //s.Append("<div class=\"summary\">");
                            isDivOpenSummaryAdded = true;
                        }
                        //if (!isDivSOpenummaryAdded)
                        //{
                        //    s.Append("<div class=\"summary\">");
                        //    
                        //}

                        s.Append(_getNewLineChar()+_getCssColors(HttpContext.Current.Server.HtmlEncode(line)));
                    }
                    else
                    {
                        if (
                            (line.StartsWith("public") || line.StartsWith("protected")) //|| line.StartsWith("private")
                            && 
                            (line.IndexOf("class") == -1 && line.IndexOf("interface") == -1)
                            )
                        {
                            if (isDivOpenSummaryAdded)
                            {
                                //s.Append("</div>");
                                isDivOpenSummaryAdded = false;
                            }
                            s.Append(_getNewLineChar() + _getCssColors(line));
                            
                            //if (isDivSOpenummaryAdded)
                            //{
                            //    s.Append("</div>");
                            //    isDivSOpenummaryAdded = false;
                            //}
                        }
                        else
                        {
                            continue;
                        }
                        
                    }

                    
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                reader.Close();
            }
            
            return s.ToString();
        }

        private string _getNewLineChar()
        {
            return "<br />";
        }
    }
}
