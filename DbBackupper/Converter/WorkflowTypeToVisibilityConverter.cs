using System;
using System.Windows;
using System.Windows.Data;
using Swsu.Tools.DbBackupper.Infrastructure;

namespace Swsu.Tools.DbBackupper.Converter
{
	public class WorkflowTypeToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{

			var worktype = value as EWorkflowType?;
			return worktype == EWorkflowType.NormalWork ? Visibility.Collapsed : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter,
			System.Globalization.CultureInfo culture)
		{
			if (value == null) return EWorkflowType.NormalWork;

			var content = (Visibility)value;
			return content == Visibility.Collapsed
				? EWorkflowType.NormalWork
				: EWorkflowType.WorkWithDb;
		}
	}
}