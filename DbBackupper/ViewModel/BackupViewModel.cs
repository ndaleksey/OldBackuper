using System;
using System.Collections.Generic;
using System.Diagnostics;
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
			MakeBackupCommand = new DelegateCommand(MakeBackup);
		}
		
		#endregion

		#region Methods' execute
		private void CreateBackupFileName()
		{
			var dialog = new SaveFileDialog();
			var date = DateTime.Today.Date;

			if (FileFormat == FileFormat.Plain)
			{
				dialog.FileName = date.ToString("dd-MM-yy") + ".sql";
				dialog.Filter = dialog.DefaultExt = "Файлы запросов (*.sql)|*.sql";
			}
			else
			{
				dialog.FileName = date.ToString("dd-MM-yy") + ".dump";
				dialog.Filter = dialog.DefaultExt = "Файлы резервных копий (*.dump)|*.dump";
			}

			if ((bool)dialog.ShowDialog())
			{
				DumpFileName = dialog.FileName;
			}
		}

		private async void MakeBackup()
		{
			try
			{
				Logs.Clear();

				var schemes = new List<string>();

				foreach (var scheme in DbObjects)
					if (scheme.IsChecked)
						schemes.Add(scheme.Name);

				var dumpExeFilePath = $"{Environment.CurrentDirectory}\\Tools\\pg_dump.exe";
				//            var dumpExeFilePath = @"C:\Program Files\PostgreSQL\9.5\bin\pg_dump.exe";
				var objectTypes = DataOnly ? ObjectType.DataOnly : SchemasOnly ? ObjectType.SchemeOnly : ObjectType.Default;

				WorkflowType = EWorkflowType.WorkWithDb;

				await MakeDumpAsync(dumpExeFilePath, schemes, objectTypes, FileFormat, IsBlobs);

				MessageBox.Show("Резервная копия успешно создана", "Создание резервной копии", MessageBoxButton.OK,
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