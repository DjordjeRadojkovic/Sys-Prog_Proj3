using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace project_3
{
    public static class HttpHelper
    {
        public static string GetKeyWord(string fullPath)
        {
            NameValueCollection queryparams = HttpUtility.ParseQueryString(fullPath);
            return queryparams["keyword"];
        }
        public static void SendError(HttpListenerContext context, int code, string message)
        {
            try
            {
                string html = $"<html><body style='font-family:sans-serif;text-align:center;padding:50px'>" +
                              $"<h1>{code}</h1><p>{message}</p></body></html>";
                byte[] data = Encoding.UTF8.GetBytes(html);

                context.Response.StatusCode = code;
                context.Response.ContentType = "text/html; charset=utf-8";
                context.Response.ContentLength64 = data.Length;
                context.Response.OutputStream.Write(data, 0, data.Length);
                context.Response.OutputStream.Close();
            }
            catch { }
        }
    }
}
