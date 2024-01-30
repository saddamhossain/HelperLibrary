using System;
using System.Web;

namespace HelperLibrary
{
    public static class CookieHelper
    {
        public static void Set(string key, string value, int day)
        {
            if (WebHelper.IsRequestAvailable)
            {
                var c = new HttpCookie(key)
                {
                    Value = value,
                    Expires = DateTime.Now.AddDays(day),
                };
                HttpContext.Current.Response.Cookies.Add(c);
            }
        }

        public static string Get(string key)
        {
            var value = string.Empty;

            if (WebHelper.IsRequestAvailable)
            {
                var c = HttpContext.Current.Request.Cookies[key];
                return c != null ? HttpContext.Current.Server.HtmlEncode(c.Value).Trim() : value;
            }
            return value;
        }

        public static bool Exists(string key)
        {
            if (WebHelper.IsRequestAvailable)
                return HttpContext.Current.Request.Cookies[key] != null;

            return false;
        }

        public static void Delete(string key)
        {
            if (WebHelper.IsRequestAvailable)
            {
                if (Exists(key))
                {
                    var c = new HttpCookie(key)
                    {
                        Expires = DateTime.Now.AddDays(-1)
                    };
                    HttpContext.Current.Response.Cookies.Add(c);
                }
            }
        }

        public static void RemoveAll()
        {
            if (WebHelper.IsRequestAvailable)
            {
                for (int i = 0; i <= HttpContext.Current.Request.Cookies.Count - 1; i++)
                {
                    if (HttpContext.Current.Request.Cookies[i] != null)
                        Delete(HttpContext.Current.Request.Cookies[i].Name);
                }
            }
        }
    }
}
