using System;
using System.ComponentModel;
using System.Reflection;

namespace Swsu.Tools.DbBackupper.Infrastructure
{
	public static class EnumExtensions
	{
		public static string GetDescription(this Enum enumValue)
		{
			var fi = enumValue.GetType().GetField(enumValue.ToString());
			var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

			return attributes.Length > 0 ? attributes[0].Description : enumValue.ToString();
		}
	}
}