using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm;
using Microsoft.Win32;
using Swsu.Tools.DbBackupper.Infrastructure;
using Swsu.Tools.DbBackupper.Model;
using Swsu.Tools.DbBackupper.Resources;
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

		public RestoreViewModel(Process process, Action<EWorkflowType> workflowType) : base(process, workflowType)
		{
			CreateRestoreFileNameCommand = new DelegateCommand(CreateRestoreFileName);
			RestoreBackupCommand = new DelegateCommand(RestoreBackup, CanRestoreBackup);
			CreateDbCommand = new DelegateCommand(CreateDatabase, CanCreateDatabase);
		}

		#endregion

		#region Commands' methods

		private void CreateRestoreFileName()
		{
			try
			{
				var dialog = new OpenFileDialog
				{
					Filter = $"{Messages.SqlTypeFiles} (*.sql)|*.sql"
					         + $"|{Messages.BinaryTypeFiles} (*.bin)|*.bin"
					         + $"|{Messages.TarTypeFiles} (*.tar)|*.tar"
				};

				var showDialog = dialog.ShowDialog();

				if (showDialog == null || !(bool) showDialog) return;

				FileFormat = (FileFormat)dialog.FilterIndex;
				DumpFileName = dialog.FileName;
			}
			catch (Exception e)
			{
				Helper.LogError(e);
			}
		}

		private bool CanRestoreBackup()
		{
			return !string.IsNullOrEmpty(Host) && !string.IsNullOrEmpty(User) && !string.IsNullOrEmpty(Database) && !string.IsNullOrEmpty(DumpFileName);
		}

		private async void RestoreBackup()
		{
			try
			{
				var cb = GetConnectionBuilder();

				WorkflowTypeChangedHandler?.Invoke(EWorkflowType.LoadFromDb);

				var databases = await DbService.GetDatabasesAsync(cb);

				if (databases.Contains(cb.Database))
					if (MessageBox.Show(Messages.SuchDbAlreadyExists, Properties.Resources.Restore, MessageBoxButton.YesNo,
						    MessageBoxImage.Question) == MessageBoxResult.No)
						return;

				var restoreFileName = $"{Environment.CurrentDirectory}\\Tools\\pg_restore.exe";

				if (FileFormat == FileFormat.Plain)
					restoreFileName = $"{Environment.CurrentDirectory}\\Tools\\psql.exe";

				var objectsType = DataOnly ? ObjectType.DataOnly : SchemaOnly ? ObjectType.SchemeOnly : ObjectType.Default;

				WorkflowTypeChangedHandler?.Invoke(EWorkflowType.Restore);

				Logs.Clear();

				await RestoreAsync(restoreFileName, objectsType, FileFormat, DumpFileName, CreateDb, CleanDb);
			}
			catch (Exception e)
			{
				Helper.LogError(e);
				/*MessageBox.Show(Messages.RestoreFailed, Messages.Restoring, MessageBoxButton.OK,
					MessageBoxImage.Error);*/
			}
			finally
			{
				WorkflowTypeChangedHandler?.Invoke(EWorkflowType.NormalWork);
			}
		}

		private bool CanCreateDatabase()
		{
			return ValidateConnectionBuilder();
		}

		private async void CreateDatabase()
		{
			try
			{
				var builder = GetConnectionBuilder();

				WorkflowTypeChangedHandler?.Invoke(EWorkflowType.LoadFromDb);

				var databases = await DbService.GetDatabasesAsync(builder);

				if (
					databases.Any(
						d => string.Equals(d, builder.Database, StringComparison.CurrentCulture)))
				{
					if (
						MessageBox.Show(Messages.DbAlreadyExistsWarning, Messages.NewDbCreating,
							MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
						return;
					
					var connections = (await DbService.GetActiveConnectionsAsync(builder)).ToList();

					if (connections.Count > 0)
						if (MessageBox.Show(Messages.ActiveConnectionsAbortingRequest, Messages.NewDbCreating,
							    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
							return;

					await DbService.StopActiveConnectionAsync(builder, connections);
				}

				WorkflowTypeChangedHandler?.Invoke(EWorkflowType.WorkWithDb);

				await DbService.CreateDatabaseAsync(builder, Database);

//				GetDbStructure();

				MessageBox.Show(Messages.CreateDbSucceed, Messages.NewDbCreating, MessageBoxButton.OK,
					MessageBoxImage.Information);
			}
			catch (Exception e)
			{
				Helper.LogError(e);
				MessageBox.Show(Messages.CreateDbFailed, Messages.NewDbCreating, MessageBoxButton.OK,
					MessageBoxImage.Error);
			}
			finally
			{
				WorkflowTypeChangedHandler?.Invoke(EWorkflowType.NormalWork);
			}
		}

		#endregion

		#region Methods

		private void ProcessRestoreExitCode(int code)
		{
			switch (code)
			{
				case 0:
					MessageBox.Show(Messages.RestoreSucceed, Messages.Restoring, MessageBoxButton.OK,
						MessageBoxImage.Information);
					return;
				case 1:
					MessageBox.Show(Messages.RestoreProcessFailed, Messages.Restoring, MessageBoxButton.OK,
						MessageBoxImage.Warning);
					return;
				default:
					MessageBox.Show(Messages.RestoreFailed, Messages.Restoring, MessageBoxButton.OK,
						MessageBoxImage.Error);
					return;
			}
		}

		#endregion
	}
}