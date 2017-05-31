using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm;
using NLog;
using Swsu.Tools.DbBackupper.Properties;
using Swsu.Tools.DbBackupper.Service;

namespace Swsu.Tools.DbBackupper.ViewModel
{
	public class MainViewModel : CustomViewModel
	{
		#region Fields

		private string _selectedEncoding;

		private TabViewModel _backupViewModel;
		private TabViewModel _restoreViewModel;

		private string _cultureName;

		#endregion

		#region Properties

		public static bool IsCultureChanged { get; private set; }

		public string CultureName
		{
			get { return _cultureName; }
			set { SetProperty(ref _cultureName, value, nameof(CultureName)); }
		}

		public TabViewModel BackupViewModel
		{
			get { return _backupViewModel; }
			set { SetProperty(ref _backupViewModel, value, nameof(BackupViewModel)); }
		}

		public TabViewModel RestoreViewModel
		{
			get { return _restoreViewModel; }
			set { SetProperty(ref _restoreViewModel, value, nameof(RestoreViewModel)); }
		}

		public string SelectedEncoding
		{
			get { return _selectedEncoding; }
			set { SetProperty(ref _selectedEncoding, value, nameof(SelectedEncoding)); }
		}

		public ObservableCollection<Node> DumpSchemes { get; }
		public ObservableCollection<Node> RestoreSchemes { get; }
		public ObservableCollection<string> DbEncodings { get; }

		#endregion

		#region Commands

		public ICommand ChangeCultureCommand { get; }

		#endregion

		#region Constructors

		public MainViewModel()
		{
			CultureName = Thread.CurrentThread.CurrentUICulture.Name;

			BackupViewModel = new BackupViewModel()
			{
				Host = "127.0.0.1",
				Port = 5432,
				Database = "los_db",
				User = "postgres",
				Password = "postgres"
			};

			RestoreViewModel = new RestoreViewModel()
			{
				Host = "127.0.0.1",
				Port = 5432,
				Database = "los_db",
				User = "postgres",
				Password = "postgres"
			};

			DumpSchemes = new ObservableCollection<Node>();
			RestoreSchemes = new ObservableCollection<Node>();
			DbEncodings = new ObservableCollection<string>();

			ChangeCultureCommand = new DelegateCommand(ChangeCulture, CanChangeCulture);

			try
			{
				var encodings = Encoding.GetEncodings();

				foreach (var encoding in encodings)
					DbEncodings.Add(encoding.Name);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
				Helper.Logger.Log(LogLevel.Error, e);
			}
		}

		#endregion

		#region Commands' methods

		private bool CanChangeCulture()
		{
			return true;
		}

		private void ChangeCulture()
		{
			try
			{
				if (
				MessageBox.Show(Properties.Resources.ChangeLanguageRequest, Properties.Resources.LanguageChanging,
					MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.No)
					return;

				Settings.Default.Culture = Settings.Default.Culture.Name == "ru-RU"
					? new CultureInfo("fr-FR")
					: new CultureInfo("ru-RU");

				Settings.Default.Save();

				IsCultureChanged = true;
				Application.Current.Shutdown(1);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
				Helper.Logger.Error(e);
			}
		}

		#endregion
	}
}