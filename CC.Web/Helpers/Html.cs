using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace CC.Web.Helpers
{
	public static class Html
	{

		public static Dictionary<int, string> ToDictionary<T>()
		{
			Dictionary<int, string> result = new Dictionary<int, string>();
			foreach (var key in Enum.GetValues(typeof(T)))
			{
				result.Add((int)key, Enum.GetName(typeof(T), key));
			}
			return result;
		}

	}
}



namespace System.Web.Mvc.Html
{
	public static class EnumHelper
	{
		public static IList<SelectListItem> GetSelectList(Type type)
		{

			IList<SelectListItem> selectList = new List<SelectListItem>();

			// According to HTML5: "The first child option element of a select element with a required attribute and
			// without a multiple attribute, and whose size is "1", must have either an empty value attribute, or must
			// have no text content."  SelectExtensions.DropDownList[For]() methods often generate a matching
			// <select/>.  Empty value for Nullable<T>, empty text for round-tripping an unrecognized value, or option
			// label serves in some cases.  But otherwise, ignoring this does not cause problems in either IE or Chrome.
			Type checkedType = Nullable.GetUnderlyingType(type) ?? type;
			if (checkedType != type)
			{
				// Underlying type was non-null so handle Nullable<T>; ensure returned list has a spot for null
				selectList.Add(new SelectListItem { Text = String.Empty, Value = String.Empty, });
			}

			// Populate the list
			const BindingFlags BindingFlags =
				BindingFlags.DeclaredOnly | BindingFlags.GetField | BindingFlags.Public | BindingFlags.Static;
			foreach (FieldInfo field in checkedType.GetFields(BindingFlags))
			{
				// fieldValue will be an numeric type (byte, ...)
				object fieldValue = field.GetRawConstantValue();

				selectList.Add(new SelectListItem { Text = GetDisplayName(field), Value = fieldValue.ToString(), });
			}

			return selectList;
		}
		private static string GetDisplayName(FieldInfo field)
		{
			DisplayAttribute display = field.GetCustomAttributes(typeof(DisplayAttribute), false).OfType<DisplayAttribute>().FirstOrDefault();
			if (display != null)
			{
				string name = display.GetName();
				if (!String.IsNullOrEmpty(name))
				{
					return name;
				}
			}

			return field.Name;
		}

		public static string EnumDisplayNameFor(this Enum item)
		{
			var type = item.GetType();
			var member = type.GetMember(item.ToString());
			DisplayAttribute displayName = (DisplayAttribute)member[0].GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault();

			if (displayName != null)
			{
				return displayName.Name;
			}

			return item.ToString();
		}
	}
	public static class MyHtmlExtensions
	{
		public static MvcHtmlString FieldIdFor<TModel, TValue>(this HtmlHelper<TModel> html,
			Expression<Func<TModel, TValue>> expression)
		{
			string htmlFieldName = ExpressionHelper.GetExpressionText(expression);
			string inputFieldId = html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(htmlFieldName);
			return MvcHtmlString.Create(inputFieldId);
		}
		public static MvcHtmlString FieldNameFor<TModel, TValue>(this HtmlHelper<TModel> html,
			Expression<Func<TModel, TValue>> expression)
		{
			string htmlFieldName = ExpressionHelper.GetExpressionText(expression);
			string inputFieldId = html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlFieldName);
			return MvcHtmlString.Create(inputFieldId);
		}

		public static MvcHtmlString MenuLink(this HtmlHelper htmlHelper,
			string linkText,
			string actionName,
			string controllerName,
			string area = null
			)
		{
			string currentAction = htmlHelper.ViewContext.RouteData.GetRequiredString("action");
			string currentController = htmlHelper.ViewContext.RouteData.GetRequiredString("controller");
			string currentArea = htmlHelper.ViewContext.RouteData.GetRequiredString("area");
			if (actionName == currentAction && controllerName == currentController && area == currentArea)
			{
				return htmlHelper.ActionLink(
					linkText,
					actionName,
					controllerName,
					null,
					new
					{
						@class = "current"
					});
			}
			return htmlHelper.ActionLink(linkText, actionName, controllerName);
		}

		public static MvcHtmlString DescriptionFor<TModel, TValue>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TValue>> expression)
		{
			var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
			var description = metadata.Description;
			var tb = new TagBuilder("a");
			tb.InnerHtml = "?";
			var editorId = htmlHelper.FieldIdFor(expression);
			tb.Attributes.Add("href", "#" + editorId);
			tb.Attributes.Add("class", "description");
			tb.Attributes.Add("title", description);
			return new MvcHtmlString(tb.ToString(TagRenderMode.Normal));

		}
		public static MvcHtmlString DescriptionFor<TModel, TValue>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TValue>> expression, bool addDescription)
		{
			var res = htmlHelper.DisplayFor(expression);
			if (addDescription)
				return new MvcHtmlString(res.ToString() + htmlHelper.DescriptionFor(expression));
			else
				return res;
		}
		public static MvcHtmlString UnsortedList<TModel>(this HtmlHelper<TModel> html, IEnumerable<string> list)
		{
			var items = string.Empty;
			if (list.Any()) items = "<li>" + string.Join("</li><li>", list) + "</li>";
			return new MvcHtmlString("<ul>" + items + "</ul>");
		}		

	}
}