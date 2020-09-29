using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Web.Mvc.Html;

namespace System.Web.Mvc
{
    public static class MvcExtensions
    {

        public static List<string> ValidationErrorMessages(this ModelStateDictionary ms)
        {
            List<string> errors = new List<string>();
            foreach (ModelState modelState in ms.Values)
            {
                foreach (ModelError error in modelState.Errors)
                {
                    errors.Add(error.ErrorMessage);
                }
            }
            return errors;
        }

		public static InvalidModelException ex(this ModelStateDictionary ms)
		{
			return new InvalidModelException(null, ms);
		}
		public static InvalidModelException ex (this ModelStateDictionary ms, string message)
		{
			return new InvalidModelException(message, ms);
		}

        public static SelectList ToSelectList<TId, TName>(this IEnumerable<IdNamePair<TId, TName>> source)
        {
            var result = new SelectList(source.Select(f => new SelectListItem()
            {
                Value = f.Id == null ? string.Empty : f.Id.ToString(),
                Text = f.Name.ToString()
            }));
            return result;
        }

        public static SelectList ToSelectList<TId, TName>(this IEnumerable<IdNamePair<TId, TName>> source, TId selectedValue)
        {
            var result = new SelectList(source.Select(f => new SelectListItem()
            {
                Value = f.Id == null ? string.Empty : f.Id.ToString(),
                Text = f.Name.ToString(),
                Selected = f.Id.Equals(selectedValue)
            }));
            return result;
        }

        public static MvcHtmlString WarningsSummary(this HtmlHelper helper)
        {
            var str = new System.Text.StringBuilder();
            var warnings = helper.ViewData["Warnings"] as List<string>;
            if(warnings!=null)
            {
                str.Append("<ul class=\"warning\">");
                foreach (var msg in warnings)
                {
                    str.Append("<li>")
                        .Append(helper.Encode(msg))
                        .Append("</li>");
                }
                str.Append("</ul>");

            }
            return new MvcHtmlString(str.ToString());
        }
		public static MvcHtmlString PartialFor<TModel, TProperty>(this HtmlHelper<TModel> helper, System.Linq.Expressions.Expression<Func<TModel, TProperty>> expression, string partialViewName)
		{
			string name = ExpressionHelper.GetExpressionText(expression);
			object model = ModelMetadata.FromLambdaExpression(expression, helper.ViewData).Model;
			var viewData = new ViewDataDictionary(helper.ViewData)
			{
				TemplateInfo = new System.Web.Mvc.TemplateInfo
				{
					HtmlFieldPrefix = name
				}
			};

			return helper.Partial(partialViewName, model, viewData);

		}
       
    }

    public static class RequestExtensions
    {
        public static bool IsJsonRequest(this HttpRequestBase request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            bool rtn = false;
            const string jsonMime = "application/json";

            if (request.AcceptTypes != null)
            {
                rtn = request.AcceptTypes.Any(t => t.Equals(jsonMime, StringComparison.OrdinalIgnoreCase));
            }
            return rtn || request.ContentType.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Any(t => t.Equals(jsonMime, StringComparison.OrdinalIgnoreCase));
        }
        public static bool IsMoblieAppRequest(this HttpRequestBase request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            var result = request.Headers != null &&
                request.Headers["X-CC-DeviceId"] != null;
            return result;
        }
    }

	public class InvalidModelException : Exception
	{
		private string p;
		public System.Web.Mvc.ModelStateDictionary ModelState;

		

		

		public InvalidModelException(string p, System.Web.Mvc.ModelStateDictionary ModelState):base(p)
		{
			this.p = p;
			this.ModelState = ModelState;
		}
	}
}
