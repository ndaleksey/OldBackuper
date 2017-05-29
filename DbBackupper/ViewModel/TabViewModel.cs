using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm;
using NLog;
using Npgsql;
using Swsu.Tools.DbBackupper.Infrastructure;
using Swsu.Tools.DbBackupper.Model;
using Swsu.Tools.DbBackupper.Service;

namespace Swsu.Tools.DbBackupper.ViewModel
{
	public abstract class TabViewModel : CustomViewModel
	{
		#region Fields

		private string _host;
		private int _port;
		private string _database;
		private string _user;
		private string _password;
		private FileFormat _fileFormat;
		private string _dumpFileName;
		private bool _dataOnly;
		private bool _schemasOnly;
		private bool _isBlobs;

		#endregion

		#region Properties

		public string Host
		{
			get { return _host; }
			set { SetProperty(ref _host, value, nameof(Host)); }
		}

		public int Port
		{
			get { return _port; }
			set { SetProperty(ref _port, value, nameof(Port)); }
		}

		public string Database
		{
			get { return _database; }
			set { SetProperty(ref _database, value, nameof(Database)); }
		}

		public string User
		{
			get { return _user; }
			set { SetProperty(ref _user, value, nameof(User)); }
		}

		public string Password
		{
			get { return _password; }
			set { SetProperty(ref _password, value, nameof(Password)); }
		}

		public FileFormat FileFormat
		{
			get { return _fileFormat; }
			set { SetProperty(ref _fileFormat, value, nameof(FileFormat)); }
		}

		public string DumpFileName
		{
			get { return _dumpFileName; }
			set { SetProperty(ref _dumpFileName, value, nameof(DumpFileName)); }
		}

		public bool DataOnly
		{
			get { return _dataOnly; }
			set { SetProperty(ref _dataOnly, value, nameof(DataOnly)); }
		}

		public bool SchemasOnly
		{
			get { return _schemasOnly; }
			set { SetProperty(ref _schemasOnly, value, nameof(SchemasOnly)); }
		}

		public bool IsBlobs
		{
			get { return _isBlobs; }
			set { SetProperty(ref _isBlobs, value, nameof(IsBlobs)); }
		}

		public ObservableCollection<Node> DbObjects { get; } = new ObservableCollection<Node>();
		public ObservableCollection<string> Logs { get; } = new ObservableCollection<string>();
		#endregion

		#region Commands
		public ICommand GetDbStructureCommand { get; }
		public ICommand PingHostCommand { get; }
		#endregion

		#region Constructors

		protected TabViewModel()
		{
			GetDbStructureCommand = new DelegateCommand(GetDbStructure);
			PingHostCommand = new DelegateCommand(PingHost);
		}

		#endregion

		#region Commands' methods
		private void PingHost()
		{
			try
			{
				var ping = new Ping().Send(Host);

				if (ping == null) throw new NullReferenceException("Сервер БД не доступен");

				if (ping.Status == IPStatus.Success)
					MessageBox.Show("Сервер БД доступен", "Проверка соединения", MessageBoxButton.OK, MessageBoxImage.Information);
				else
					MessageBox.Show("Сервер БД не доступен", "Проверка соединения", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
				Helper.Logger.Error(e);
				MessageBox.Show("Ошибка соединения с сервером БД", "Проверка соединения", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		
		protected async void GetDbStructure()
		{
			try
			{
				WorkflowType = EWorkflowType.LoadFromDb;

				var objects = await DbService.GetDbSchemaObjectsAsync(GetConnectionBuilder(), Database);

				DbObjects.Clear();

				foreach (var obj in objects)
					DbObjects.Add(obj);

				if (objects.Count == 0)
					MessageBox.Show("Список объектов БД пуст", "Получение структуры БД", MessageBoxButton.OK,
						MessageBoxImage.Information);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
				Helper.Logger.Error(e.Message);
			}
			finally
			{
				WorkflowType = EWorkflowType.NormalWork;
			}
		}
		#endregion

		#region Methods

		protected NpgsqlConnectionStringBuilder GetConnectionBuilder()
		{
			return new NpgsqlConnectionStringBuilder()
			{
				Host = Host,
				Port = Port,
				Database = Database,
				Username = User,
				Password = Password
			};
		}

		protected Task MakeDumpAsync(string exeFileName, IReadOnlyCollection<string> schemes, ObjectType objectType,
			FileFormat fileFormat, bool isBlobs)
		{
			return Task.Run(() => MakeDump(exeFileName, schemes, objectType, fileFormat, isBlobs));
		}

		protected void MakeDump(string exeFileName, IReadOnlyCollection<string> schemes, ObjectType objectType,
			FileFormat fileFormat, bool isBlobs)
		{
			var arguments = new StringBuilder();

			arguments.Append($"-h {Host}");
			arguments.Append($" -U {User}");
			arguments.Append($" -d {Database}");
			arguments.Append($" -f \"{DumpFileName}\"");

			switch (objectType)
			{
				case ObjectType.DataOnly:
					arguments.Append(" -a");
					break;
				case ObjectType.SchemeOnly:
					arguments.Append(" -s");
					break;
				case ObjectType.Default:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(objectType), objectType, null);
			}

			if (isBlobs)
				arguments.Append(" -b");

			if (schemes.Count > 0)
				foreach (var scheme in schemes)
					arguments.Append($" -n {scheme}");

			switch (fileFormat)
			{
				case FileFormat.Plain:
					arguments.Append(" -Fp");
					break;
				case FileFormat.Tar:
					arguments.Append(" -Ft");
					break;
				default:
					arguments.Append(" -Fc");
					break;
			}

			arguments.Append(" -v");

			var info = new ProcessStartInfo
			{
				FileName = exeFileName,
				Arguments = arguments.ToString(),
				EnvironmentVariables = {{"PGPASSWORD", Password}},
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardError = true
			};

			using (var process = Process.Start(info))
			{
				process.BeginErrorReadLine();
				process.ErrorDataReceived += OnDumpOutputDataReceived;
				process.WaitForExit();

				var text = new StringBuilder();
				text.Append($"{DateTime.Now:HH:mm:ss}\nКод завершения:\t{process.ExitCode}");
				text.Append(process.ExitCode == 0
					? "\n\nРезервирование завершилось успешно!"
					: "\n\nПри резервировании произошли ошибки");

//					OutConcurrentText(Logs, text.ToString());
			}
		}

		private void OnDumpOutputDataReceived(object sender, DataReceivedEventArgs e)
		{
		}

		protected Task RestoreAsync(string exeFileName, ObjectType objectType, FileFormat fileFormat, string dumpFileName)
		{
			return Task.Run(() => Restore(exeFileName, objectType, fileFormat, dumpFileName));
		}

		protected void Restore(string exeFileName, ObjectType objectType, FileFormat fileFormat, string dumpFileName)
		{
			var arguments = new StringBuilder();

			arguments.Append($"-h {Host}");
			arguments.Append($" -U {User}");
			arguments.Append($" -d {Database}");

			if (fileFormat != FileFormat.Plain)
			{
				arguments.Append(" -v");
			}

			switch (objectType)
			{
				case ObjectType.DataOnly:
					arguments.Append(" -a");
					break;
				case ObjectType.SchemeOnly:
					arguments.Append(" -s");
					break;
				case ObjectType.Default:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(objectType), objectType, null);
			}

			arguments.Append(fileFormat != FileFormat.Plain ? $" {dumpFileName}" : $" -f \"{dumpFileName}\"");

			var info = new ProcessStartInfo
			{
				StandardErrorEncoding = Encoding.Default,
				FileName = exeFileName,
				Arguments = arguments.ToString(),
				EnvironmentVariables = {{"PGPASSWORD", Password}},
				RedirectStandardError = true,
				CreateNoWindow = true,
				UseShellExecute = false
			};

			using (var process = Process.Start(info))
			{
				process.BeginErrorReadLine();
				process.ErrorDataReceived += OnRestoreOutputDataReceived;

				process.WaitForExit();

				var text = new StringBuilder();

				text.Append($"{DateTime.Now:HH:mm:ss}\nКод завершения:\t{process.ExitCode}");
				text.Append(process.ExitCode == 0
					? "\n\nВосстановление завершилось успешно!"
					: "\n\nПри восстановлении произошли ошибки");

//				OutConcurrentText(Logs, text.ToString());
			}
		}

		private void OnRestoreOutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			
		}

		#endregion
	}
}