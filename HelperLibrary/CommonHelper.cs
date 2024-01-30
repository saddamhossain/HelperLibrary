using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace HelperLibrary
{
    public static class CommonHelper
    {
        public static int ToInt(this object obj)
        {
            try
            {
                return Convert.ToInt32(obj);
            }
            catch
            {
                return 0;
            }
        }

        public static long ToLong(this object obj)
        {
            try
            {
                return Convert.ToInt64(obj);
            }
            catch
            {
                return 0;
            }
        }

        public static double ToDouble(this object obj)
        {
            try
            {
                return Convert.ToDouble(obj);
            }
            catch
            {
                return 0;
            }
        }

        public static decimal ToDecimal(this object obj)
        {
            try
            {
                return Convert.ToDecimal(obj);
            }
            catch
            {
                return decimal.Zero;
            }
        }

        public static float ToFloat(this object obj)
        {
            try
            {
                return Convert.ToSingle(obj);
            }
            catch
            {
                return 0;
            }
        }

        public static short ToShort(this object obj)
        {
            try
            {
                return Convert.ToInt16(obj);
            }
            catch
            {
                return 0;
            }
        }

        public static bool ToBoolean(this object obj)
        {
            try
            {
                return Convert.ToBoolean(obj);
            }
            catch
            {
                return false;
            }
        }

        public static bool ToBoolean(this object obj, bool defaultValue)
        {
            if (obj == null)
                return defaultValue;

            return obj.ToBoolean();
        }

        public static char ToChar(this object obj)
        {
            try
            {
                return Convert.ToChar(obj);
            }
            catch
            {
                return ' ';
            }
        }

        public static byte ToByte(this object obj)
        {
            try
            {
                return Convert.ToByte(obj);
            }
            catch
            {
                return 0;
            }
        }

        public static bool IsNull(this object obj)
        {
            return obj == null;
        }

        public static bool IsDbNull(this object obj)
        {
            return obj == DBNull.Value;
        }

        public static bool IsEmpty<T>(this ICollection<T> source)
        {
            return (source == null || source.Count == 0);
        }

        public static bool IsNullOrDefault<T>(this T? value) where T : struct
        {
            return default(T).Equals(value.GetValueOrDefault());
        }

        public static T IfNullDefaultValue<T>(this T obj)
        {
            if (obj == null)
                return default(T);
            else
                return obj;
        }

        public static bool HasProperty(this object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName) != null;
        }

        public static object GetPropertyValue(this object obj, string propertyName)
        {
            if (obj.HasProperty(propertyName))
                return obj.GetType().GetProperty(propertyName).GetValue(obj, null);

            return null;
        }

        public static void SetPropertyValue(this object obj, string propertyName, object value)
        {
            if (obj.HasProperty(propertyName))
                obj.GetType().GetProperty(propertyName).SetValue(obj, value, null);
        }

        public static PropertyInfo[] GetProperties(this object obj)
        {
            return obj.GetType().GetProperties();
        }

        public static string[] GetPropertyNames(this object obj)
        {
            return obj.GetType().GetProperties().Select(x => x.Name).ToArray();
        }

        public static object[] GetPropertyValues(this object obj)
        {
            return obj.GetType().GetProperties().Select(x => x.GetValue(obj)).ToArray();
        }

        public static Dictionary<string, object> GetPropertyDictionary(this object obj)
        {
            return obj.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(obj));
        }

        public static void Open(this SqlCommand cmd)
        {
            if (cmd != null && cmd.Connection != null && cmd.Connection.State != ConnectionState.Open)
                cmd.Connection.Open();
        }

        public static void Close(this SqlCommand cmd)
        {
            if (cmd != null && cmd.Connection != null && cmd.Connection.State != ConnectionState.Closed)
                cmd.Connection.Close();
        }

        public static DataTable ToDataTable<T>(this IEnumerable<T> collection)
        {
            DataTable dt = null;
            try
            {
                dt = new DataTable(nameof(T));
                PropertyInfo[] propinfo = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var item in propinfo)
                {
                    dt.Columns.Add(item.Name, item.PropertyType);
                }
                foreach (T item in collection)
                {
                    DataRow drow = dt.NewRow();
                    drow.BeginEdit();
                    foreach (var pi in propinfo)
                        drow[pi.Name] = pi.GetValue(item, null);
                    drow.EndEdit();
                    dt.Rows.Add(drow);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return dt;
        }

        public static List<T> ToList<T>(this DataTable dt) where T : new()
        {
            try
            {
                List<string> columns = new List<string>();
                foreach (DataColumn item in dt.Columns)
                    columns.Add(item.ColumnName);

                return dt.AsEnumerable().ToList().ConvertAll(x => GetObject<T>(x, columns)).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static T GetObject<T>(DataRow row, List<string> columns) where T : new()
        {
            try
            {
                T obj = new T();
                foreach (PropertyInfo item in typeof(T).GetProperties())
                {
                    string name = columns.Find(x => x.ToLower() == item.Name.ToLower());
                    if (!string.IsNullOrEmpty(name))
                    {
                        string value = row[name].ToString();
                        if (!string.IsNullOrEmpty(value))
                        {
                            if (Nullable.GetUnderlyingType(item.PropertyType) != null)
                            {
                                value = row[name].ToString().Replace("$", string.Empty).Replace(",", string.Empty);
                                item.SetValue(obj, Convert.ChangeType(value, Type.GetType(Nullable.GetUnderlyingType(item.PropertyType).ToString())), null);
                            }
                            else
                            {
                                value = row[name].ToString().Replace("%", "");
                                item.SetValue(obj, Convert.ChangeType(value, Type.GetType(item.PropertyType.ToString())), null);
                            }
                        }
                    }
                }
                return obj;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static T FromXml<T>(this string xmlData)
        {
            T obj = default(T);

            if (string.IsNullOrEmpty(xmlData))
            {
                return obj;
            }

            XDocument dataXmlDoc = XDocument.Parse(xmlData);

            foreach (var prop in typeof(T).GetProperties())
            {
                if (dataXmlDoc.Descendants().SingleOrDefault(p => p.Name.LocalName == prop.Name) != null)
                    prop.SetValue(obj, Convert.ChangeType(dataXmlDoc.Descendants().SingleOrDefault(p => p.Name.LocalName == prop.Name).Value, prop.PropertyType));
            }

            return obj;
        }

        public static T MapTo<S, T>(S source) where T : new() where S : class
        {
            T target = new T();
            var sourceProperties = source.GetProperties();

            for (int i = 0; i < sourceProperties.Length; i++)
            {
                var current = sourceProperties[i];
                target.SetPropertyValue(current.Name, source.GetPropertyValue(current.Name));
            }

            return target;
        }

        public static IEnumerable<T> MapTo<S, T>(IEnumerable<S> source) where T : new() where S : class
        {
            return source.Select(x => MapTo<S, T>(x)).ToList();
        }
    }
}
