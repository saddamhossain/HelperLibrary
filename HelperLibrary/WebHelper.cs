using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HelperLibrary
{
    public static class WebHelper
    {
        public static void ClearSession()
        {
            if (IsRequestAvailable && HttpContext.Current.Session != null)
                HttpContext.Current.Session.Clear();
        }

        public static Uri GetUri
        {
            get
            {
                Uri uri = null;
                if (IsRequestAvailable)
                    uri = HttpContext.Current.Request.Url;

                return uri;
            }
        }

        public static string GetUrlReferrer
        {
            get
            {
                string refererUrl = string.Empty;

                if (IsRequestAvailable && HttpContext.Current.Request.UrlReferrer != null)
                    refererUrl = HttpContext.Current.Request.UrlReferrer.PathAndQuery;

                return refererUrl;
            }
        }

        public static string GetCurrentPageUrl(bool includeQueryString)
        {
            string url = string.Empty;

            if (!IsRequestAvailable && HttpContext.Current.Request.Url != null)
                return url;

            if (includeQueryString)
                url = HttpContext.Current.Request.RawUrl;
            else
            {
                if (HttpContext.Current.Request.Url != null)
                    url = HttpContext.Current.Request.Url.PathAndQuery;
            }

            return url;
        }

        public static string GetCurrentUrl(bool includeQueryString = false)
        {
            string result = string.Empty;
            if (IsRequestAvailable && HttpContext.Current.Request.Url != null)
            {
                if (includeQueryString)
                    result = HttpContext.Current.Request.Url.AbsoluteUri;
                else
                    result = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path);
            }
            return result;
        }

        public static string GetDomainUrl
        {
            get
            {
                return HttpContext.Current.Request.Url.Scheme +
                    Uri.SchemeDelimiter +
                    HttpContext.Current.Request.Url.Host +
                    (HttpContext.Current.Request.Url.IsDefaultPort ? "" : ":" + HttpContext.Current.Request.Url.Port);
            }
        }

        public static bool IsRequestAvailable
        {
            get
            {
                try
                {
                    if (HttpContext.Current == null)
                        return false;

                    if (HttpContext.Current.Request == null)
                        return false;
                }
                catch (HttpException)
                {
                    return false;
                }

                return true;
            }
        }

        public static string IpAddress
        {
            get
            {
                if (!IsRequestAvailable)
                    return string.Empty;

                string result = string.Empty;
                try
                {
                    if (IsRequestAvailable && HttpContext.Current.Request.UserHostAddress != null)
                    {
                        if (HttpContext.Current.Request.UserHostAddress.Contains("::1"))
                            result = "127.0.0.1";
                        else
                        {
                            result = HttpContext.Current.Request.UserHostAddress;
                        }
                    }
                }
                catch (HttpException)
                {
                    if (string.IsNullOrEmpty(ServerVariables("HTTP_X_FORWARDED_FOR")))
                        result = ServerVariables("REMOTE_ADDR");
                    else
                        result = ServerVariables("HTTP_X_FORWARDED_FOR");

                    if (result == null || result.Contains(":1"))
                        result = "127.0.0.1";
                }

                if (!string.IsNullOrEmpty(result) && result.Contains(","))
                    result = result.Split(',')[0];

                return result;
            }
        }

        public static string ServerVariables(string name)
        {
            string result = string.Empty;
            try
            {
                if (IsRequestAvailable)
                    if (HttpContext.Current.Request.ServerVariables[name] != null)
                        result = HttpContext.Current.Request.ServerVariables[name];
            }
            catch
            {
                result = string.Empty;
            }
            return result;
        }

        public static bool TryWriteWebConfig()
        {
            try
            {
                File.SetLastWriteTimeUtc(FileHelper.MapPath("~/Web.config"), DateTime.Now);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryWriteGlobalAsax()
        {
            try
            {
                File.SetLastWriteTimeUtc(FileHelper.MapPath("~/Global.asax"), DateTime.Now);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static AspNetHostingPermissionLevel GetTrustLevel()
        {
            AspNetHostingPermissionLevel _trustLevel = AspNetHostingPermissionLevel.None;

            foreach (AspNetHostingPermissionLevel trustLevel in new[] { AspNetHostingPermissionLevel.Unrestricted,
                AspNetHostingPermissionLevel.High, AspNetHostingPermissionLevel.Medium,
                AspNetHostingPermissionLevel.Low, AspNetHostingPermissionLevel.Minimal })
            {
                try
                {
                    new AspNetHostingPermission(trustLevel).Demand();
                    _trustLevel = trustLevel;
                    break;
                }
                catch (System.Security.SecurityException)
                {
                    continue;
                }
            }
            return _trustLevel;
        }

        public static bool IsSecureConnection
        {
            get
            {
                bool usingSsl = false;
                if (IsRequestAvailable)
                    usingSsl = HttpContext.Current.Request.IsSecureConnection;

                return usingSsl;
            }
        }

        public static bool IsLocal
        {
            get
            {
                bool isLocal = false;
                if (IsRequestAvailable)
                    isLocal = HttpContext.Current.Request.IsLocal;

                return isLocal;
            }
        }

        public static bool IsStaticResource(HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            string extension = VirtualPathUtility.GetExtension(request.Path);

            if (string.IsNullOrEmpty(extension))
                return false;

            switch (extension.ToLower())
            {
                case ".axd":
                case ".ashx":
                case ".bmp":
                case ".css":
                case ".gif":
                case ".htm":
                case ".html":
                case ".ico":
                case ".jpeg":
                case ".jpg":
                case ".js":
                case ".png":
                case ".rar":
                case ".zip":
                case ".map":
                case ".json":
                case ".svg":
                case ".woff":
                case ".woff2":
                case ".ttf":
                case ".eot":
                case ".txt":
                case ".less":
                case ".sass":
                    return true;
            }

            return false;
        }

        public static string UserAgent
        {
            get
            {
                string result = string.Empty;

                if (IsRequestAvailable)
                    result = HttpContext.Current.Request.UserAgent;

                return result;
            }
        }

        public static string Browser
        {
            get
            {
                string result = string.Empty;
                if (IsRequestAvailable)
                {
                    var browser = HttpContext.Current.Request.Browser;
                    var platform = browser.Platform;
                    if (browser.IsMobileDevice)
                    {
                        platform = browser.MobileDeviceModel;
                    }
                    result = (browser.Browser + " " + browser.Version + " " + platform).ToLower();
                }

                return result;
            }
        }

        //public static bool IsMobileDevice
        //{
        //    get
        //    {
        //        bool isMobileDevice = false;
        //        if (IsRequestAvailable)
        //            isMobileDevice = HttpContext.Current.Request.Browser.IsMobileDevice;

        //        return isMobileDevice;
        //    }
        //}

        private static readonly Regex b = new Regex(@"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        private static readonly Regex v = new Regex(@"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public static bool IsMobileBrowser
        {
            get
            {
                return (b.IsMatch(UserAgent) || v.IsMatch(UserAgent.Substring(0, 4)));
            }
        }

        public static string GetServerInfo
        {
            get
            {
                try
                {
                    return string.Concat(Environment.MachineName, " - ", Environment.UserName);
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        public static string AppSetting(string appSettingName)
        {
            appSettingName = appSettingName.Trim();
            return ConfigurationManager.AppSettings[appSettingName];
        }

        public static string ConnectionString(string connectionName)
        {
            connectionName = connectionName.Trim();
            return ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
        }

        public static string ModifyQueryString(string url, string queryStringModification, string anchor)
        {
            if (url == null)
                url = string.Empty;
            url = url.ToLowerInvariant();

            if (queryStringModification == null)
                queryStringModification = string.Empty;
            queryStringModification = queryStringModification.ToLowerInvariant();

            if (anchor == null)
                anchor = string.Empty;
            anchor = anchor.ToLowerInvariant();


            string str = string.Empty;
            string str2 = string.Empty;
            if (url.Contains("#"))
            {
                str2 = url.Substring(url.IndexOf("#") + 1);
                url = url.Substring(0, url.IndexOf("#"));
            }
            if (url.Contains("?"))
            {
                str = url.Substring(url.IndexOf("?") + 1);
                url = url.Substring(0, url.IndexOf("?"));
            }
            if (!string.IsNullOrEmpty(queryStringModification))
            {
                if (!string.IsNullOrEmpty(str))
                {
                    var dictionary = new Dictionary<string, string>();
                    foreach (string str3 in str.Split(new[] { '&' }))
                    {
                        if (!string.IsNullOrEmpty(str3))
                        {
                            string[] strArray = str3.Split(new[] { '=' });
                            if (strArray.Length == 2)
                            {
                                if (!dictionary.ContainsKey(strArray[0]))
                                    dictionary[strArray[0]] = strArray[1];
                            }
                            else
                            {
                                dictionary[str3] = null;
                            }
                        }
                    }
                    foreach (string str4 in queryStringModification.Split(new[] { '&' }))
                    {
                        if (!string.IsNullOrEmpty(str4))
                        {
                            string[] strArray2 = str4.Split(new[] { '=' });
                            if (strArray2.Length == 2)
                            {
                                dictionary[strArray2[0]] = strArray2[1];
                            }
                            else
                            {
                                dictionary[str4] = null;
                            }
                        }
                    }
                    var builder = new StringBuilder();
                    foreach (string str5 in dictionary.Keys)
                    {
                        if (builder.Length > 0)
                        {
                            builder.Append("&");
                        }
                        builder.Append(str5);
                        if (dictionary[str5] != null)
                        {
                            builder.Append("=");
                            builder.Append(dictionary[str5]);
                        }
                    }
                    str = builder.ToString();
                }
                else
                {
                    str = queryStringModification;
                }
            }
            if (!string.IsNullOrEmpty(anchor))
            {
                str2 = anchor;
            }
            return (url + (string.IsNullOrEmpty(str) ? "" : ("?" + str)) + (string.IsNullOrEmpty(str2) ? "" : ("#" + str2))).ToLowerInvariant();
        }

        public static string RemoveQueryString(string url, string queryString)
        {
            if (url == null)
                url = string.Empty;
            url = url.ToLowerInvariant();

            if (queryString == null)
                queryString = string.Empty;
            queryString = queryString.ToLowerInvariant();


            string str = string.Empty;
            if (url.Contains("?"))
            {
                str = url.Substring(url.IndexOf("?") + 1);
                url = url.Substring(0, url.IndexOf("?"));
            }
            if (!string.IsNullOrEmpty(queryString))
            {
                if (!string.IsNullOrEmpty(str))
                {
                    var dictionary = new Dictionary<string, string>();
                    foreach (string str3 in str.Split(new[] { '&' }))
                    {
                        if (!string.IsNullOrEmpty(str3))
                        {
                            string[] strArray = str3.Split(new[] { '=' });
                            if (strArray.Length == 2)
                            {
                                dictionary[strArray[0]] = strArray[1];
                            }
                            else
                            {
                                dictionary[str3] = null;
                            }
                        }
                    }
                    dictionary.Remove(queryString);

                    var builder = new StringBuilder();
                    foreach (string str5 in dictionary.Keys)
                    {
                        if (builder.Length > 0)
                        {
                            builder.Append("&");
                        }
                        builder.Append(str5);
                        if (dictionary[str5] != null)
                        {
                            builder.Append("=");
                            builder.Append(dictionary[str5]);
                        }
                    }
                    str = builder.ToString();
                }
            }
            return (url + (string.IsNullOrEmpty(str) ? string.Empty : ("?" + str)));
        }

        public static string StripTags(string text)
        {
            if (String.IsNullOrEmpty(text))
                return string.Empty;

            text = Regex.Replace(text, @"(>)(\r|\n)*(<)", "><");
            text = Regex.Replace(text, "(<[^>]*>)([^<]*)", "$2");
            text = Regex.Replace(text, "(&#x?[0-9]{2,4};|&quot;|&amp;|&nbsp;|&lt;|&gt;|&euro;|&copy;|&reg;|&permil;|&Dagger;|&dagger;|&lsaquo;|&rsaquo;|&bdquo;|&rdquo;|&ldquo;|&sbquo;|&rsquo;|&lsquo;|&mdash;|&ndash;|&rlm;|&lrm;|&zwj;|&zwnj;|&thinsp;|&emsp;|&ensp;|&tilde;|&circ;|&Yuml;|&scaron;|&Scaron;)", "@");

            return text;
        }

        public static string ReplaceAnchorTags(string text)
        {
            if (String.IsNullOrEmpty(text))
                return string.Empty;

            text = Regex.Replace(text, @"<a\b[^>]+>([^<]*(?:(?!</a)<[^<]*)*)</a>", "$1", RegexOptions.IgnoreCase);
            return text;
        }

        public static string ConvertPlainTextToHtml(string text)
        {
            if (String.IsNullOrEmpty(text))
                return string.Empty;

            text = text.Replace("\r\n", "<br />");
            text = text.Replace("\r", "<br />");
            text = text.Replace("\n", "<br />");
            text = text.Replace("\t", "&nbsp;&nbsp;");
            text = text.Replace("  ", "&nbsp;&nbsp;");

            return text;
        }

        public static string ConvertHtmlToPlainText(string text,
            bool decode = false, bool replaceAnchorTags = false)
        {
            if (String.IsNullOrEmpty(text))
                return string.Empty;

            if (decode)
                text = HttpUtility.HtmlDecode(text);

            text = text.Replace("<br>", "\n");
            text = text.Replace("<br >", "\n");
            text = text.Replace("<br />", "\n");
            text = text.Replace("&nbsp;&nbsp;", "\t");
            text = text.Replace("&nbsp;&nbsp;", "  ");

            if (replaceAnchorTags)
                text = ReplaceAnchorTags(text);

            return text;
        }

        public static string ConvertPlainTextToParagraph(string text)
        {
            if (String.IsNullOrEmpty(text))
                return string.Empty;

            Regex paragraphStartRegex = new Regex("<p>", RegexOptions.IgnoreCase);
            Regex paragraphEndRegex = new Regex("</p>", RegexOptions.IgnoreCase);

            text = paragraphStartRegex.Replace(text, string.Empty);
            text = paragraphEndRegex.Replace(text, "\n");
            text = text.Replace("\r\n", "\n").Replace("\r", "\n");
            text = text + "\n\n";
            text = text.Replace("\n\n", "\n");
            var strArray = text.Split(new[] { '\n' });
            var builder = new StringBuilder();
            foreach (string str in strArray)
            {
                if ((str != null) && (str.Trim().Length > 0))
                {
                    builder.AppendFormat("<p>{0}</p>\n", str);
                }
            }
            return builder.ToString();
        }

        public static IEnumerable<CultureInfo> GetCultures()
        {
            return CultureInfo.GetCultures(CultureTypes.NeutralCultures).AsEnumerable();
        }

        private static string GetErrorMessage(ModelError error, ModelState modelState)
        {
            if (!string.IsNullOrEmpty(error.ErrorMessage))
            {
                return error.ErrorMessage;
            }
            if (modelState.Value == null)
            {
                return error.ErrorMessage;
            }
            var args = new object[] { modelState.Value.AttemptedValue };
            return string.Format("ValueNotValidForProperty=The value '{0}' is invalid", args);
        }

        public static object SerializeErrors(this ModelStateDictionary modelState)
        {
            return modelState.Where(entry => entry.Value.Errors.Any())
                .ToDictionary(entry => entry.Key, entry => SerializeModelState(entry.Value));
        }

        private static Dictionary<string, object> SerializeModelState(ModelState modelState)
        {
            var dictionary = new Dictionary<string, object>();
            dictionary["errors"] = modelState.Errors.Select(x => GetErrorMessage(x, modelState)).ToArray();
            return dictionary;
        }

        public static object ToDataSourceResult(this ModelStateDictionary modelState)
        {
            if (!modelState.IsValid)
            {
                return modelState.SerializeErrors();
            }
            return null;
        }
    }
}
