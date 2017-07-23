using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm;
using Microsoft.Win32;
using Swsu.Tools.DbBackupper.Infrastructure;
using Swsu.Tools.DbBackupper.Model;
using Swsu.Tools.DbBackupper.Service;

namespace Swsu.Tools.DbBackupper.ViewModel
{
	public class BackupViewModel : TabViewModel
	{
		#region Commands

		public ICommand CreateBackupFileNameCommand { get; }
		public ICommand MakeBackupCommand { get; }

		#endregion

		#region Constructor

		public BackupViewModel()
		{
			CreateBackupFileNameCommand = new DelegateCommand(CreateBackupFileName);
			MakeBackupCommand = new DelegateCommand(MakeBackup, CanMakeBackup);
		}

		#endregion

		#region Commands' methods

		private bool CanMakeBackup()
		{
			return !string.IsNullOrEmpty(Host) && !string.IsNullOrEmpty(User) && !string.IsNullOrEmpty(Database) &&
			       !string.IsNullOrEmpty(DumpFileName);
		}

		private void CreateBackupFileName()
		{
			try
			{
				var dialog = new SaveFileDialog();
				var date = DateTime.Today.Date;

				/*switch (FileFormat)
				{
					case FileFormat.Plain:
						dialog.FileName = date.ToString("dd-MM-yy") + ".sql";
						dialog.Filter = dialog.DefaultExt = $"{Resources.Messages.SqlTypeFiles} (*.sql)|*.sql";
						break;

					case FileFormat.Binary:
						dialog.FileName = date.ToString("dd-MM-yy") + ".bin";
						dialog.Filter = dialog.DefaultExt = $"{Resources.Messages.BinaryTypeFiles} (*.bin)|*.bin";
						break;

					case FileFormat.Tar:
						dialog.FileName = date.ToString("dd-MM-yy") + ".tar";
						dialog.Filter = dialog.DefaultExt = $"{Resources.Messages.TarTypeFiles} (*.tar)|*.tar";
						break;
				}*/
				

				dialog.FileName = date.ToString("dd-MM-yy") + ".sql";
				dialog.Filter = 
					$"{Resources.Messages.SqlTypeFiles} (*.sql)|*.sql"
					+ $"|{Resources.Messages.BinaryTypeFiles} (*.bin)|*.bin"
					+ $"|{Resources.Messages.TarTypeFiles} (*.tar)|*.tar";

				var showDialog = dialog.ShowDialog();

				if (showDialog == null || !(bool) showDialog) return;

				FileFormat = (FileFormat) dialog.FilterIndex;
				DumpFileName = dialog.FileName;
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
				Helper.Logger.Error(e);
			}
		}

		private async void MakeBackup()
		{
			try
			{
				var schemes = DbObjects.Where(s => s != null && s.IsChecked).Select(scheme => scheme.Name).ToList();

				if (schemes.Count == 0)
				{
					MessageBox.Show(Resources.Messages.InvalidObjectsCountForBackuping, Resources.Messages.Backuping,
						MessageBoxButton.OK, MessageBoxImage.Information);
					return;
				}

				var dumpExeFilePath = $"{Environment.CurrentDirectory}\\Tools\\pg_dump.exe";
				var objectTypes = DataOnly ? ObjectType.DataOnly : SchemaOnly ? ObjectType.SchemeOnly : ObjectType.Default;

				WorkflowType = EWorkflowType.Backup;

				Logs.Clear();

				var process = await MakeDumpAsync(dumpExeFilePath, schemes, objectTypes, FileFormat, CreateDb, CleanDb, IsBlobs);

				ProcessBackupExitCode(process.ExitCode);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
				Helper.Logger.Error(e);
				MessageBox.Show(Resources.Messages.BackupFailed, Resources.Messages.Restoring, MessageBoxButton.OK,
					MessageBoxImage.Error);
			}
			finally
			{
				WorkflowType = EWorkflowType.NormalWork;
			}
		}

		#endregion

		#region Methods
		private void ProcessBackupExitCode(int code)
		{
			switch (code)
			{
				case 0:
					MessageBox.Show(Resources.Messages.BackupSucceed, Resources.Messages.Backuping, MessageBoxButton.OK,
						MessageBoxImage.Information);
					return;
				case 1:
					MessageBox.Show(Resources.Messages.BackupProcessFailed, Resources.Messages.Backuping, MessageBoxButton.OK,
						MessageBoxImage.Warning);
					return;
				default:
					MessageBox.Show(Resources.Messages.BackupFailed, Resources.Messages.Backuping, MessageBoxButton.OK,
						MessageBoxImage.Error);
					return;
			}
		}
		#endregion
	}
}