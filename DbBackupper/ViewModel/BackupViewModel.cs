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
			return !string.IsNullOrEmpty(DumpFileName);
		}

		private void CreateBackupFileName()
		{
			try
			{
				var dialog = new SaveFileDialog();
				var date = DateTime.Today.Date;

				if (FileFormat == FileFormat.Plain)
				{
					dialog.FileName = date.ToString("dd-MM-yy") + ".sql";
					dialog.Filter = dialog.DefaultExt = $"{Resources.Messages.SqlTypeFiles} (*.sql)|*.sql";
				}
				else
				{
					dialog.FileName = date.ToString("dd-MM-yy") + ".backup";
					dialog.Filter = dialog.DefaultExt = $"{Resources.Messages.DumpTypeFiles} (*.backup)|*.backup";
				}

				if ((bool)dialog.ShowDialog())
				{
					DumpFileName = dialog.FileName;
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
				Helper.Logger.Error(e);
			}
		}

		#endregion

		#region Methods

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
				
				await MakeDumpAsync(dumpExeFilePath, schemes, objectTypes, FileFormat, CreateDb, CleanDb, IsBlobs);

				MessageBox.Show(Resources.Messages.BackupSucceed, Resources.Messages.Backuping, MessageBoxButton.OK,
					MessageBoxImage.Information);
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
	}
}