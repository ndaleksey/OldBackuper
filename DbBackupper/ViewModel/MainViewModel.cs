using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm;
using Swsu.Tools.DbBackupper.Infrastructure;
using Swsu.Tools.DbBackupper.Properties;
using Swsu.Tools.DbBackupper.Service;

namespace Swsu.Tools.DbBackupper.ViewModel
{
	public class MainViewModel : ViewModelBase
	{
		#region Fields

		private string _selectedEncoding;

		private TabViewModel _backupViewModel;
		private TabViewModel _restoreViewModel;

		private string _cultureName;

		private EWorkflowType _workflowType;

		#endregion

		#region Properties

		public EWorkflowType WorkflowType
		{
			get { return _workflowType; }
			set { SetProperty(ref _workflowType, value, nameof(WorkflowType)); }
		}

		public Action<EWorkflowType> WorkflowTypeChangedHandler { get; }

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

		#endregion

		#region Commands

		public ICommand ChangeCultureCommand { get; }
		public ICommand<CancelEventArgs> CanCloseCommand { get; }

		#endregion

		#region Constructors

		public MainViewModel()
		{
			WorkflowTypeChangedHandler = type => WorkflowType = type;
			CultureName = Thread.CurrentThread.CurrentUICulture.Name;

			BackupViewModel = new BackupViewModel(WorkflowTypeChangedHandler)
			{
				Host = "127.0.0.1",
				Port = 5432,
				Database = "los_db"
			};

			RestoreViewModel = new RestoreViewModel(WorkflowTypeChangedHandler)
			{
				Host = "127.0.0.1",
				Port = 5432,
				Database = "los_db"
			};

			DumpSchemes = new ObservableCollection<Node>();
			RestoreSchemes = new ObservableCollection<Node>();

			ChangeCultureCommand = new DelegateCommand(ChangeCulture, CanChangeCulture);
			CanCloseCommand = new DelegateCommand<CancelEventArgs>(CanClose);
		}

		private void CanClose(CancelEventArgs args)
		{
			if (WorkflowType == EWorkflowType.NormalWork) return;

			if (MessageBox.Show("Выполняется продолжительная операция. Вы хотите прервать процесс?", "Закрытие приложения",
				    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
			{

				return;
			}

			args.Cancel = true;

			/*if (MessageBox.Show("Вы уверенны, что хотите закрыть приложение", "Закрытие", MessageBoxButton.YesNo,
				    MessageBoxImage.Question) == MessageBoxResult.Yes)
			{
				
			}
			else
			{
				args.Cancel = true;
			}*/
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