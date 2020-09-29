using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CC.Web.Tests
{
    using System;
    using System.Web;
    using Rhino.Mocks;
    using System.Text.RegularExpressions;
    using System.IO;
    using System.Collections.Specialized;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Collections.Generic;


    


        public class ContextMocks
        {
            public Moq.Mock<HttpContextBase> HttpContext { get; private set; }
            public Moq.Mock<HttpRequestBase> Request { get; private set; }
            public Moq.Mock<HttpResponseBase> Response { get; private set; }
            public RouteData RouteData { get; private set; }





            public ContextMocks(Controller onController, bool IsAjax = false, bool isJson = false)
            {

                HttpContext = new Moq.Mock<HttpContextBase>();
                Request = new Moq.Mock<HttpRequestBase>();
                Response = new Moq.Mock<HttpResponseBase>();


                HttpContext.Setup(x => x.Request).Returns(Request.Object);
                HttpContext.Setup(x => x.Response).Returns(Response.Object);
                HttpContext.Setup(x => x.Session).Returns(new FakeSessionState());

                Request.Setup(x => x.Cookies).Returns(new HttpCookieCollection());
                Response.Setup(x => x.Cookies).Returns(new HttpCookieCollection());


                Request.Setup(x => x.QueryString).Returns(new NameValueCollection());
                Request.Setup(x => x.Form).Returns(new NameValueCollection());
                Request.Setup(x => x.ServerVariables).Returns(new NameValueCollection());
                if (IsAjax)
                {
                    Request.Setup(x => x["X-Requested-With"]).Returns("XMLHttpRequest");
                }
                if (isJson)
                {
                    Request.SetupGet(c => c.AcceptTypes).Returns(new[] { "application/json" }).Verifiable();
                }
                Request.Setup(x => x.Params).Returns(new NameValueCollection());


                Request.SetupAllProperties();

                RequestContext rc = new RequestContext(HttpContext.Object, new RouteData());

                onController.ControllerContext = new ControllerContext(rc, onController);






            }





            private class FakeSessionState : HttpSessionStateBase
            {

                Dictionary<string, object> items = new Dictionary<string, object>();
                public override object this[string name]
                {
                    get { return items.ContainsKey(name) ? items[name] : null; }
                    set { items[name] = value; }
                }

            }

        }
    }
