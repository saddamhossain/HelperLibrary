using System;
using System.ComponentModel.DataAnnotations;

namespace HelperLibrary
{
    public static class EnumHelper
    {
        public static string GetDisplayName(this Enum value)
        {
            if (value == null)
                return string.Empty;

            if (!value.GetType().IsEnum && value.GetType().GetMember(value.ToString()).Length <= 0)
                throw new ArgumentException(string.Format("Type '{0}' is not Enum", value.GetType()));

            var member = value.GetType().GetMember(value.ToString())[0];
            var attributes = member.GetCustomAttributes(typeof(DisplayAttribute), false);

            if (attributes.Length <= 0)
                throw new ArgumentException(string.Format("'{0}.{1}' doesn't have DisplayAttribute", value.GetType().Name, value));

            var attribute = (DisplayAttribute)attributes[0];
            return attribute.GetName();
        }

        public static string GetNameByValue<TEnum>(this TEnum obj, int value) where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            return Enum.GetName(typeof(TEnum), value);
        }

        public static int GetValueByName<TEnum>(this TEnum obj, string name) where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            int value = 0;
            var values = Enum.GetValues(typeof(TEnum));
            foreach (var item in values)
            {
                string valueName = Enum.GetName(typeof(TEnum), item);
                if (valueName == name)
                    value = (int)item;
            }

            return value;
        }

        public static TEnum Parse<TEnum>(string value, bool ignoreCase) where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException("T must be an enum type.");

            var result = (TEnum)Enum.Parse(typeof(TEnum), value, ignoreCase);
            return result;
        }

        public static TEnum ToEnum<TEnum>(this string value, bool ignoreCase) where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            return Parse<TEnum>(value, ignoreCase);
        }

        public static TEnum Parse<TEnum>(string value) where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            return Parse<TEnum>(value, false);
        }

        public static TEnum ToEnum<TEnum>(this string value) where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            return Parse<TEnum>(value);
        }
    }
}
