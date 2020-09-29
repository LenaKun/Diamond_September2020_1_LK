using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;


namespace CC.Extensions
{
    public static class Errors
    {
        
        /// <summary>
        /// Dumps info of an http request into a string
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string CreateMessage(HttpContext context)
        {
            var sb = new StringBuilder();

            var exception = context.Server.GetLastError();

            sb.AppendFormat("An error occured at {0}.", context.Request.RawUrl);

            sb.AppendFormat("Last server error:{0}", ExceptionDetails(exception));

            sb.AppendFormat("Form values:{0}", dumpCollectionValues(context.Request.Form));

            sb.AppendFormat("Servars:{0}", dumpCollectionValues(context.Request.ServerVariables));


            return sb.ToString();
        }


        private static string dumpCollectionValues(global::System.Collections.Specialized.NameValueCollection c)
        {
            var sb = new StringBuilder();
            return sb.ToString();
        }

        private static string ExceptionDetails(Exception ex)
        {
            return ex == null ? string.Empty : string.Format("Message:{0}. Stack:{1}. \r\n Inner:{2}", ex.Message, ex.StackTrace, ExceptionDetails(ex.InnerException));
        }


    }
}
