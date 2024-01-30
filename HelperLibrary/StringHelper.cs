using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;

namespace HelperLibrary
{
    public static class StringHelper
    {
        public static string BytesToString(long byteCount)
        {
            string[] suf = { " byte", " KB", " MB", " GB", " TB", " PB", " EB" };
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        public static string ToBase64(this string text)
        {
            try
            {
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string FromBase64(this string text)
        {
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(text));
            }
            catch
            {
                return string.Empty;
            }
        }

        public static SecureString ToSecureString(this string text)
        {
            using (SecureString secureString = new SecureString())
            {
                foreach (char c in text)
                    secureString.AppendChar(c);

                return secureString;
            }
        }

        public static bool IsNullOrEmpty(this string text)
        {
            return text == null || text.Length == 0;
        }

        public static string GenerateGuid(bool withDash = true)
        {
            if (withDash)
                return System.Guid.NewGuid().ToString("N");

            return System.Guid.NewGuid().ToString();
        }

        public static string ToCapitalize(this string text)
        {
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(text.ToLowerInvariant());
        }

        public static string ClearText(this string text)
        {
            return text.Trim().ToLowerInvariant().ToSafe();
        }

        public static bool IsValidEmail(this string text)
        {
            return Email.IsMatch(text);
        }

        public static bool IsWebUrl(this string text)
        {
            return WebUrl.IsMatch(text);
        }

        public static bool IsNumeric(this object obj)
        {
            return Regex.IsMatch(obj.ToString(), @"^-*[0-9,\.]+$");
        }

        public static bool IsValidIPAddress(this string text)
        {
            return Regex.IsMatch(text,
                    @"\b(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b");
        }

        public static bool IsCultureCode(this string text)
        {
            return CultureCode.IsMatch(text);
        }

        public static bool IsGuid(this string text)
        {
            return Guid.IsMatch(text);
        }

        public static string ToUrlSlug(this string text)
        {
            text = text.Replace("ş", "s");
            text = text.Replace("Ş", "s");
            text = text.Replace("İ", "i");
            text = text.Replace("I", "i");
            text = text.Replace("ı", "i");
            text = text.Replace("ö", "o");
            text = text.Replace("Ö", "o");
            text = text.Replace("ü", "u");
            text = text.Replace("Ü", "u");
            text = text.Replace("Ç", "c");
            text = text.Replace("ç", "c");
            text = text.Replace("ğ", "g");
            text = text.Replace("Ğ", "g");
            text = text.Replace(" ", "-");
            text = text.Replace("---", "-");
            text = text.Replace("?", "");
            text = text.Replace("/", "");
            text = text.Replace(".", "");
            text = text.Replace("'", "");
            text = text.Replace("#", "");
            text = text.Replace("%", "");
            text = text.Replace("&", "");
            text = text.Replace("*", "");
            text = text.Replace("!", "");
            text = text.Replace("@", "");
            text = text.Replace("+", "");
            text = text.ToLower();
            text = text.Trim();

            // tüm harfleri küçült
            string encodedUrl = (text ?? "").ToLower();

            // & ile " " yer değiştirme
            encodedUrl = Regex.Replace(encodedUrl, @"\&+", "and");

            // " " karakterlerini silme
            encodedUrl = encodedUrl.Replace("'", "");

            // geçersiz karakterleri sil
            encodedUrl = Regex.Replace(encodedUrl, @"[^a-z0-9]", "-");

            // tekrar edenleri sil
            encodedUrl = Regex.Replace(encodedUrl, @"-+", "-");

            // karakterlerin arasına tire koy
            encodedUrl = encodedUrl.Trim('-');

            return encodedUrl;
        }

        public static string TurkishMoneyToText(this object money)
        {
            decimal moneyObj = money.ToDecimal();
            string converted = moneyObj.ToString("F2").Replace(".", ",");
            string turkishLira = converted.Substring(0, converted.IndexOf(','));
            string penny = converted.Substring(converted.IndexOf(',') + 1, 2);
            string moneyText = string.Empty;
            string[] ones = { "", "Bir", "İki", "Üç", "Dört", "Beş", "Altı", "Yedi", "Sekiz", "Dokuz" };
            string[] tens = { "", "On", "Yirmi", "Otuz", "Kırk", "Elli", "Altmış", "Yetmiş", "Seksen", "Doksan" };
            string[] thousands = { "Katrilyon", "Trilyon", "Milyar", "Milyon", "Bin", "" };
            int groupCount = 6;

            string groupValue = string.Empty;
            for (int i = 0; i < groupCount * 3; i += 3)
            {
                groupValue = string.Empty;
                if (turkishLira.Substring(i, 1) != "0")
                    groupValue += ones[turkishLira.Substring(i, 1).ToInt()] + "Yüz";
                if (groupValue.Contains("BirYüz"))
                    groupValue = groupValue.Replace("BirYüz", "Yüz");

                groupValue += tens[turkishLira.Substring(i + 1, 1).ToInt()];
                groupValue += ones[turkishLira.Substring(i + 2, 1).ToInt()];
                if (!string.IsNullOrEmpty(groupValue))
                    groupValue += thousands[i / 3];
                if (groupValue.Contains("BirBin"))
                    groupValue = groupValue.Replace("Birbin", "Bin");
                moneyText += groupValue;
            }

            int moneyTextLength = moneyText.Length;
            if (!string.IsNullOrEmpty(moneyText))
                moneyText += " TL ";
            if (penny.Substring(0, 1) != "0")
                moneyText += tens[penny.Substring(0, 1).ToInt()];
            if (penny.Substring(1, 1) != "0")
                moneyText += ones[penny.Substring(1, 1).ToInt()];
            if (moneyText.Length > moneyTextLength)
                moneyText += " Kuruş";

            return moneyText;
        }

        public static string FixTurkishChar(this string text)
        {
            text = text.Replace("I", "I");
            text = text.Replace("i", "i");

            text = text.Replace("Ö", "Ö");
            text = text.Replace("ö", "ö");

            text = text.Replace("Ö", "Ö");
            text = text.Replace("ö", "ö");

            text = text.Replace("Ü", "Ü");
            text = text.Replace("ü", "ü");

            text = text.Replace("Ü", "Ü");
            text = text.Replace("ü", "ü");

            text = text.Replace("Ç", "Ç");
            text = text.Replace("ç", "ç");

            text = text.Replace("Ç", "Ç");
            text = text.Replace("ç", "ç");

            text = text.Replace("G", "G");
            text = text.Replace("g", "g");

            text = text.Replace("S", "S");
            text = text.Replace("s", "s");

            return text;
        }

        public static string ToJsonString(this object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, new IsoDateTimeConverter() { DateTimeFormat = "dd.MM.yyyy HH:mm:ss" });
        }

        public static T FromJsonString<T>(this string text)
        {
            return JsonConvert.DeserializeObject<T>(text, new IsoDateTimeConverter() { DateTimeFormat = "dd.MM.yyyy HH:mm:ss" });
        }

        public static string ToSafe(this string value, string defaultValue = null)
        {
            if (!string.IsNullOrEmpty(value))
                return value;

            return (defaultValue ?? string.Empty);
        }

        public static bool IsAlpha(this string value)
        {
            return Alpha.IsMatch(value);
        }

        public static bool IsAlphaNumeric(this string value)
        {
            return AlphaNumeric.IsMatch(value);
        }

        public static int[] ToIntArray(this string s)
        {
            return Array.ConvertAll(s.SplitSafe(","), v => int.Parse(v.Trim()));
        }

        public static string[] SplitSafe(this string value, string separator)
        {
            if (string.IsNullOrEmpty(value))
                return new string[0];
            return value.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static bool ToIntArrayContains(this string s, int value, bool defaultValue)
        {
            if (s == null)
                return defaultValue;
            var arr = s.ToIntArray();
            if (arr == null || arr.Count() <= 0)
                return defaultValue;
            return arr.Contains(value);
        }

        public static bool IsIBAN(this string text)
        {
            text = Regex.Replace(text, @"\s", "").ToUpper();
            if (Regex.IsMatch(text, @"\W"))
                return false;
            if (!Regex.IsMatch(text, @"^\D\D\d\d.+"))
                return false;
            if (Regex.IsMatch(text, @"^\D\D00.+|^\D\D01.+|^\D\D99.+"))
                return false;
            string countryCode = text.Substring(0, 2);
            if (text.Length != 26)
                return false;
            if (!Regex.IsMatch(text.Remove(0, 4), @"\d{5}[a-zA-Z0-9]{17}"))
                return false;
            string modifiedIban = text.ToUpper().Substring(4) + text.Substring(0, 4);
            modifiedIban = Regex.Replace(modifiedIban, @"\D", m => (m.Value[0] - 55).ToString());
            int remainer = 0;
            while (modifiedIban.Length >= 7)
            {
                remainer = int.Parse(remainer + modifiedIban.Substring(0, 7)) % 97;
                modifiedIban = modifiedIban.Substring(7);
            }
            remainer = int.Parse(remainer + modifiedIban) % 97;

            if (remainer != 1)
                return false;
            return true;
        }

        public static string FixChars(this string Corrupted)
        {
            string[] Fixeds = new string[] { "Ü", "Ş", "Ğ", "Ç", "İ", "Ö", "ü", "ş", "ğ", "ç", "ı", "ö", "é", "ô", "ú", "ğ", "İ", "ğ", "ı", "ı", "Ş", "ş", "Ş", "ş", "I", "Ğ", "ğ", "ş", "ı" };
            string[] Corrupteds = new string[] { "Ãœ", "ÅŸ", "ÄŸ", "Ã‡", "Ä°", "Ã–", "Ã¼", "ÅŸ", "ÄŸ", "Ã§", "Ä±", "Ã¶", "Ã©", "Ã´", "Ãº", "Ã°", "Ã", "ð", "Ã½", "ý", "Ãž", "Ã¾", "Þ", "þ", "Ý", "Ð", "ð", "þ", "ý" };
            for (int i = 0; i < Corrupteds.Length; i++)
            {
                Corrupted = Corrupted.Replace(Corrupteds[i], Fixeds[i]);
            }
            return Corrupted;
        }

        static readonly Regex Alpha = new Regex("[^a-zA-Z]", RegexOptions.Compiled);
        static readonly Regex AlphaNumeric = new Regex("[^a-zA-Z0-9]", RegexOptions.Compiled);
        static readonly Regex WebUrl = new Regex(@"^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_\=~]*)?$", RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex Email = new Regex("^(?:[\\w\\!\\#\\$\\%\\&\\'\\*\\+\\-\\/\\=\\?\\^\\`\\{\\|\\}\\~]+\\.)*[\\w\\!\\#\\$\\%\\&\\'\\*\\+\\-\\/\\=\\?\\^\\`\\{\\|\\}\\~]+@(?:(?:(?:[a-zA-Z0-9](?:[a-zA-Z0-9\\-](?!\\.)){0,61}[a-zA-Z0-9]?\\.)+[a-zA-Z0-9](?:[a-zA-Z0-9\\-](?!$)){0,61}[a-zA-Z0-9]?)|(?:\\[(?:(?:[01]?\\d{1,2}|2[0-4]\\d|25[0-5])\\.){3}(?:[01]?\\d{1,2}|2[0-4]\\d|25[0-5])\\]))$", RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static readonly Regex Guid = new Regex(@"\{?[a-fA-F0-9]{8}(?:-(?:[a-fA-F0-9]){4}){3}-[a-fA-F0-9]{12}\}?", RegexOptions.Compiled);
        static readonly Regex CultureCode = new Regex(@"^[a-z]{2}(-[A-Z]{2})?$", RegexOptions.Singleline | RegexOptions.Compiled);
    }
}
