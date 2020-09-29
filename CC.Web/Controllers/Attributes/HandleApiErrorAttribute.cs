using System;
using CC.Web.Controllers;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;

namespace CC.Web.Attributes
{
    public class HandleApiErrorAttribute : System.Web.Mvc.HandleErrorAttribute
    {
        private HttpStatusCode statusCode;

        public HandleApiErrorAttribute() :base()
        {

        }
        public HandleApiErrorAttribute(System.Net.HttpStatusCode statusCode) : this()
        {
            this.statusCode = statusCode;
        }

        public override void OnException(ExceptionContext context)
        {
            if (context.HttpContext.Request.IsJsonRequest())
            {
                var exceptionData = context.Exception.Data;
                exceptionData.Add("ExceptionType", context.Exception.GetType().Name);
                // if request was an Ajax request, respond with json with Error field

                var result = new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = exceptionData,
                };
                context.HttpContext.Response.TrySkipIisCustomErrors = true;
                context.HttpContext.Response.StatusCode = (int)this.statusCode;
                
                context.Result = result;
                context.ExceptionHandled = true;
            }
            else
            {
                // if not an ajax request, continue with logic implemented by MVC -> html error page
                base.OnException(context);
            }
        }
    }
}