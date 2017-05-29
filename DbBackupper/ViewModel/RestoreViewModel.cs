using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm;
using Microsoft.Win32;
using Npgsql;
using Swsu.Tools.DbBackupper.Infrastructure;
using Swsu.Tools.DbBackupper.Model;
using Swsu.Tools.DbBackupper.Service;

namespace Swsu.Tools.DbBackupper.ViewModel
{
	public class RestoreViewModel : TabViewModel
	{
		#region Commands
		public ICommand CreateRestoreFileNameCommand { get; }
		public ICommand RestoreBackupCommand { get; }
		public ICommand CreateDbCommand { get; }
		#endregion

		#region Constructor

		public RestoreViewModel()
		{
			CreateRestoreFileNameCommand = new DelegateCommand(CreateRestoreFileName);
			RestoreBackupCommand = new DelegateCommand(RestoreBackup);
			CreateDbCommand = new DelegateCommand(CreateDatabase);
		}

		private async void CreateDatabase()
		{
			try
			{
				var builder = GetConnectionBuilder();

				WorkflowType = EWorkflowType.LoadFromDb;

				var databases = await DbService.GetDatabasesAsync(builder);

				if (
					databases.Any(
						d => string.Equals(d, builder.Database.Trim(), StringComparison.CurrentCultureIgnoreCase)))
					if (
						MessageBox.Show(
							$"База данных '{builder.Database}' уже существует. Вы хотите заменить ее?",
							"Создание новой БД", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
						return;

				WorkflowType = EWorkflowType.WorkWithDb;
				await DbService.CreateDatabaseAsync(builder, Database);

				MessageBox.Show("База данных успешно создана", "Создание БД", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
				Helper.Logger.Error(e);
				MessageBox.Show("Ошибка создания БД", "Создание БД", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				WorkflowType = EWorkflowType.NormalWork;
			}
		}

		#endregion

		#region Methods' execute
		private void CreateRestoreFileName()
		{
			var dialog = new OpenFileDialog();

			if (FileFormat == FileFormat.Plain)
			{
				dialog.Filter = dialog.DefaultExt = "Файлы запросов (*.sql)|*.sql";
			}
			else
			{
				dialog.Filter = dialog.DefaultExt = "Файлы резервных копий (*.dump)|*.dump";
			}

			if ((bool)dialog.ShowDialog())
			{
				DumpFileName = dialog.FileName;
			}
		}

		private async void RestoreBackup()
		{
			try
			{
				var restoreFileName = $"{Environment.CurrentDirectory}\\Tools\\pg_restore.exe";

				if (FileFormat == FileFormat.Plain)
					restoreFileName = $"{Environment.CurrentDirectory}\\Tools\\psql.exe";

				var objectsType = DataOnly ? ObjectType.DataOnly : SchemasOnly ? ObjectType.SchemeOnly : ObjectType.Default;

				WorkflowType = EWorkflowType.LoadFromDb;

				await RestoreAsync(restoreFileName, objectsType, FileFormat, DumpFileName);

				MessageBox.Show("Данные успешно восстановлены из резрвной копии", "Восстановление", MessageBoxButton.OK,
					MessageBoxImage.Information);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
				Helper.Logger.Error(e);
			}
			finally
			{
				WorkflowType = EWorkflowType.NormalWork;
			}
		}
		#endregion
	}
}