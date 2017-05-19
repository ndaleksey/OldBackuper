using System;
using System.ComponentModel;
using System.Globalization;

namespace Swsu.Tools.DbBackupper
{
    public class EnumValuesToStringNameConverter : EnumConverter
    {
        private readonly Type _enumType;

        /// <summary>Инициализирует экземпляр</summary>
        /// <param name="type">тип Enum</param>
        public EnumValuesToStringNameConverter(Type type)
            : base(type)
        {
            _enumType = type;
        }

        /// <summary>        
        /// </summary>
        /// <param name="context"></param>
        /// <param name="destType"></param>
        /// <returns></returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
        {
            return destType == typeof (string);
        }

        /// <summary>        
        /// </summary>
        /// <param name="context"></param>
        /// <param name="culture"></param>
        /// <param name="value"></param>
        /// <param name="destType"></param>
        /// <returns></returns>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
            Type destType)
        {
            if (value == null) return string.Empty;

            var fi = _enumType.GetField(Enum.GetName(_enumType, value));
            var dna = (DescriptionAttribute) Attribute.GetCustomAttribute(
                fi, typeof (DescriptionAttribute));

            return dna != null ? dna.Description : value.ToString();
        }

        /// <summary>        
        /// </summary>
        /// <param name="context"></param>
        /// <param name="srcType"></param>
        /// <returns></returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
        {
            return srcType == typeof (string);
        }

        /// <summary>        
        /// </summary>
        /// <param name="context"></param>
        /// <param name="culture"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            foreach (var fi in _enumType.GetFields())
            {
                var dna = (DescriptionAttribute) Attribute.GetCustomAttribute(
                    fi, typeof (DescriptionAttribute));

                if ((dna != null) && ((string) value == dna.Description))
                    return Enum.Parse(_enumType, fi.Name);
            }

            return Enum.Parse(_enumType, (string) value);
        }
    }
}