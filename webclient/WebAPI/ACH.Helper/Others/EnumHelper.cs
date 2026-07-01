using ACH.DataEntity.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace ACH.Helper.Others
{

    /// <summary>
    /// 获取枚举描述信息
    /// </summary>
    public static class EnumHelper
    {
        public static string GetDescription(Enum en)
        {
            Type type = en.GetType();

            FieldInfo fi = en.GetType().GetField(en.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return en.ToString();

        }


        /// <summary>
        /// 获取枚举类型所有值的描述字典 key=int value=code
        /// </summary>
        /// <typeparam name="TEnum">枚举类型</typeparam>
        /// <returns>描述字典</returns>
        public static Dictionary<int, string> GetEnumDescriptions<TEnum>() where TEnum : struct, Enum
        {
            var enumType = typeof(TEnum);
            var descriptions = new Dictionary<int, string>();

            foreach (var value in Enum.GetValues(enumType))
            {
                var enumValue = (int)Convert.ChangeType(value, typeof(int));
                var description = GetDescription((Enum)value);
                descriptions[enumValue] = description;
            }

            return descriptions;
        }


        /// <summary>
        /// 获取枚举类型所有值的描述字典 key= code value= int
        /// </summary>
        /// <typeparam name="TEnum">枚举类型</typeparam>
        /// <returns>描述字典</returns>
        public static Dictionary<string, int> GetReverseEnumDescriptions<TEnum>() where TEnum : struct, Enum
        {
            var enumType = typeof(TEnum);
            var descriptions = new Dictionary<string, int>();

            foreach (var value in Enum.GetValues(enumType))
            {
                var enumValue = (int)Convert.ChangeType(value, typeof(int));
                var description = GetDescription((Enum)value);
                descriptions[description] = enumValue;
            }

            return descriptions;
        }

        public static string GetUnit(EnumSignalType en)
        {
            switch (en)
            {
                case EnumSignalType.A:
                    return "m/s^2";
                case EnumSignalType.V:
                    return "mm/s";
                case EnumSignalType.S:
                    return "mm";
                case EnumSignalType.VT:
                    return "V";
                case EnumSignalType.I:
                    return "kA";
                case EnumSignalType.DEG:
                    return "°";
                default:
                    return "m/s^2";
            }
        }
    }

}
