using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace HelperLibrary
{
    public static class HtmlHelper
    {
        #region Enum Helper
        public static MvcHtmlString EnumSelectListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, object htmlAttributes)
        {
            return EnumSelectListFor(htmlHelper, expression, null, htmlAttributes);
        }

        public static MvcHtmlString EnumSelectListFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, string optionLabel, object htmlAttributes)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            Type enumType = metadata.ModelType;

            Type underlyingType = Nullable.GetUnderlyingType(enumType);
            if (underlyingType != null)
                enumType = underlyingType;

            var values = from Enum value in Enum.GetValues(enumType)
                         select new
                         {
                             Id = value,
                             Name = value.GetType().Name
                         };

            return htmlHelper.DropDownListFor(expression, new SelectList(values, "Id", "Name", metadata.Model), optionLabel, htmlAttributes);
        }

        public static MvcHtmlString EnumDisplayNameFor<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression)
        {
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var type = (Enum)metadata.Model;

            return new MvcHtmlString(type.GetType().Name);
        }
        #endregion


        #region Style/Script Tag Helper
        public static MvcHtmlString Script(this System.Web.Mvc.HtmlHelper htmlHelper, string path, bool isAsync = false)
        {
            return MvcHtmlString.Create(string.Format("<script src=\"{0}\"{1}></script>", ReCreatePath(path), isAsync ? " async" : string.Empty));
        }

        public static MvcHtmlString Style(this System.Web.Mvc.HtmlHelper htmlHelper, string path)
        {
            return MvcHtmlString.Create(string.Format("<link rel=\"stylesheet\" href=\"{0}\" />", ReCreatePath(path)));
        }

        public static MvcHtmlString Favicon(this System.Web.Mvc.HtmlHelper htmlHelper, string path)
        {
            return MvcHtmlString.Create(string.Format("<link rel=\"shortcut icon\" href=\"{0}\" />", ReCreatePath(path)));
        }

        public static MvcHtmlString Picture(this System.Web.Mvc.HtmlHelper htmlHelper, string path, string alt = "")
        {
            return MvcHtmlString.Create(string.Format("<img src=\"{0}\" alt=\"{1}\" />", ReCreatePath(path), alt));
        }

        private static string ReCreatePath(string path)
        {
            string newPath = path.Replace("~", string.Empty);

            if (path.StartsWith("https:", StringComparison.CurrentCultureIgnoreCase))
                path = path.Replace("https:", string.Empty);

            if (path.StartsWith("http:", StringComparison.CurrentCultureIgnoreCase))
                path = path.Replace("http:", string.Empty);

            if (!newPath.StartsWith("/", StringComparison.CurrentCultureIgnoreCase) &&
                !newPath.StartsWith("//", StringComparison.CurrentCultureIgnoreCase))
                newPath = string.Concat("/", newPath);

            if (newPath.Contains("\\"))
                newPath = newPath.Replace("\\", "//");

            return newPath;
        }
        #endregion
    }
}
