using System;
using System.Diagnostics;
using System.Windows.Data;
using Swsu.Tools.DbBackupper.Infrastructure;

namespace Swsu.Tools.DbBackupper.Converter
{
	public class WorkflowTypeToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			try
			{
				var worktype = value as EWorkflowType?;
				switch (worktype)
				{
					case null:
						return string.Empty;
					case EWorkflowType.NormalWork:
						return "Normal";
					case EWorkflowType.WorkWithDb:
						return "Обновление данных в БД";
					case EWorkflowType.LoadFromDb:
						return "Загрузка дынных из БД";
					case EWorkflowType.SaveToDb:
						return "Сохранинение данных в БД";
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
			}

			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter,
			System.Globalization.CultureInfo culture)
		{
			try
			{
				var content = (string)value;
				return string.IsNullOrEmpty(content)
					? EWorkflowType.NormalWork
					: EWorkflowType.WorkWithDb;
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
			}
			return false;
		}
	}
}