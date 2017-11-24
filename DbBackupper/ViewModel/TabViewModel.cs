using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using DevExpress.Mvvm;
using Npgsql;
using Swsu.Tools.DbBackupper.Infrastructure;
using Swsu.Tools.DbBackupper.Model;
using Swsu.Tools.DbBackupper.Resources;
using Swsu.Tools.DbBackupper.Service;

namespace Swsu.Tools.DbBackupper.ViewModel
{
	public abstract class TabViewModel : ViewModelBase
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
		private bool _schemaOnly;
		private bool _isBlobs;
		private bool _createDb;
		private bool _cleanDb;
		private bool _selectAll;

		#endregion

		#region Properties
		private Process Process { get; set; }
		public IListBoxService LogsListBoxService => GetService<IListBoxService>("LogsListBoxService");

		public Action<EWorkflowType> WorkflowTypeChangedHandler { get; protected set; }

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

		public bool SchemaOnly
		{
			get { return _schemaOnly; }
			set { SetProperty(ref _schemaOnly, value, nameof(SchemaOnly)); }
		}

		public bool CreateDb
		{
			get { return _createDb; }
			set { SetProperty(ref _createDb, value, nameof(CreateDb)); }
		}

		public bool CleanDb
		{
			get { return _cleanDb; }
			set { SetProperty(ref _cleanDb, value, nameof(CleanDb)); }
		}

		public bool IsBlobs
		{
			get { return _isBlobs; }
			set { SetProperty(ref _isBlobs, value, nameof(IsBlobs)); }
		}

		public bool SelectAll
		{
			get { return _selectAll; }
			set { SetProperty(ref _selectAll, value, nameof(SelectAll)); }
		}

		public ObservableCollection<Node> DbObjects { get; } = new ObservableCollection<Node>();
		public ObservableCollection<string> Logs { get; } = new ObservableCollection<string>();

		#endregion

		#region Commands

		public ICommand GetDbStructureCommand { get; }
		public ICommand PingHostCommand { get; }
		public ICommand SelectAllObjectsCommand { get; }

		#endregion

		#region Constructors

		protected TabViewModel(Process process,Action<EWorkflowType> workflowTypeChangedHandler)
		{
			Process = process;
			WorkflowTypeChangedHandler = workflowTypeChangedHandler;

			GetDbStructureCommand = new DelegateCommand(GetDbStructure, CanGetDbStructure);
			PingHostCommand = new DelegateCommand(PingHost, CanPingHost);
			SelectAllObjectsCommand = new DelegateCommand(SelectAllObjects, CanSelectAllObjects);
		}

		#endregion

		#region Commands' methods

		private bool CanSelectAllObjects()
		{
			return DbObjects != null && DbObjects.Any();
		}

		protected void SelectAllObjects()
		{
			foreach (var schema in DbObjects.Where(s => s != null).ToList())
				schema.IsChecked = SelectAll;
		}

		private bool CanPingHost()
		{
			IPAddress hostIp;

			return IPAddress.TryParse(Host, out hostIp);
		}

		private void PingHost()
		{
			try
			{
				var ping = new Ping().Send(Host);

				if (ping == null) throw new NullReferenceException(Messages.ServerIsNotAvailable);

				if (ping.Status == IPStatus.Success)
					MessageBox.Show(Messages.ServerIsAvailable, Messages.ConnectionCheck, MessageBoxButton.OK,
						MessageBoxImage.Information);
				else
					MessageBox.Show(Messages.ServerIsNotAvailable, Messages.ConnectionCheck, MessageBoxButton.OK,
						MessageBoxImage.Warning);
			}
			catch (Exception e)
			{
				Helper.LogError(e);
				MessageBox.Show(Messages.ServerConnectionError, Messages.ConnectionCheck, MessageBoxButton.OK,
					MessageBoxImage.Error);
			}
		}

		protected virtual bool CanGetDbStructure()
		{
			return ValidateConnectionBuilder();
		}

		protected async void GetDbStructure()
		{
			try
			{
				WorkflowTypeChangedHandler?.Invoke(EWorkflowType.LoadFromDb);

				var objects = await DbService.GetDbSchemaObjectsAsync(GetConnectionBuilder(), Database);

				DbObjects.Clear();

				foreach (var obj in objects)
					DbObjects.Add(obj);

				if (objects.Count == 0)
					MessageBox.Show(Messages.EmptyDb, Messages.DbStructureGetting, MessageBoxButton.OK,
						MessageBoxImage.Information);
			}
			catch (PostgresException dbe)
			{
				var errorMessage = Helper.ParseErrorCode(dbe);
				Helper.LogError(errorMessage, dbe);
				MessageBox.Show(errorMessage, Messages.DbStructureGetting, MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (SocketException se)
			{
				Helper.LogError(Messages.ConnectionDenied, se);
				MessageBox.Show(Messages.ConnectionDenied, Messages.DbStructureGetting, MessageBoxButton.OK,
					MessageBoxImage.Error);
			}
			catch (Exception e)
			{
				Helper.LogError(Messages.GetDbStructureError, e);
				MessageBox.Show(Messages.GetDbStructureError, Messages.DbStructureGetting, MessageBoxButton.OK,
					MessageBoxImage.Error);
			}
			finally
			{
				WorkflowTypeChangedHandler?.Invoke(EWorkflowType.NormalWork);
			}
		}

		#endregion

		#region Methods

		protected bool ValidateConnectionBuilder()
		{
			IPAddress hostIp;

			if (!IPAddress.TryParse(Host, out hostIp))
			{
				Helper.LogWarn(Messages.IpAddressIsInvalid);
				return false;
			}

			if (Port >= ushort.MaxValue)
			{
				Helper.LogWarn(Messages.PortIsInvalid);
				return false;
			}

			return !string.IsNullOrEmpty(Database) && !string.IsNullOrEmpty(User) && !string.IsNullOrEmpty(Password);
		}

		protected NpgsqlConnectionStringBuilder GetConnectionBuilder()
		{
			if (!ValidateConnectionBuilder()) throw new ArgumentException(Messages.ConnectionStringBuildingError);

			return new NpgsqlConnectionStringBuilder
			{
				Host = Host,
				Port = Port,
				Database = Database,
				Username = User,
				Password = Password,
				Pooling = false
			};
		}

		protected Task<Process> MakeDumpAsync(string exeFileName, IReadOnlyCollection<string> schemes, ObjectType objectType,
			FileFormat fileFormat, bool createDb, bool cleanDb, bool isBlobs)
		{
			return Task.Run(() => MakeDump(exeFileName, schemes, objectType, fileFormat, createDb, cleanDb, isBlobs));
		}

		protected Process MakeDump(string exeFileName, IReadOnlyCollection<string> schemes, ObjectType objectType,
			FileFormat fileFormat, bool createDb, bool cleanDb, bool isBlobs)
		{
			Helper.LogInfo(Messages.DumpProcessStarted);

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

			if (createDb)
				arguments.Append(" -C");

			if (cleanDb)
				arguments.Append(" -c");


			if (isBlobs)
				arguments.Append(" -b");

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

			if (schemes.Count > 0)
				foreach (var scheme in schemes)
					arguments.Append($" -n {scheme}");

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

			Process = Process.Start(info);

			if (Process == null) throw new NullReferenceException("Can't execute restore process");

			Process.BeginErrorReadLine();
			Process.Exited += DumpProcessOnExited;
			Process.OutputDataReceived += BackupProcess_OutputDataReceived;
			Process.ErrorDataReceived += BackupProcess_ErrorDataReceived;
			Process.WaitForExit();

			if (Process.ExitCode == 0)
				ClearLogs();

			var text = new StringBuilder();
			text.Append($"{DateTime.Now:HH:mm:ss}\n{Messages.ResultCode}:\t{Process.ExitCode}");
			text.Append(Process.ExitCode == 0
				? $"\n\n{Messages.BackupProcessSucceed}"
				: $"\n\n{Messages.BackupProcessFailed}");

			OutConcurrentText(Logs, text.ToString());

			return Process;
		}

		private void DumpProcessOnExited(object o, EventArgs args)
		{
			Helper.LogInfo(Messages.DumpProcessFinished);
		}

		private void BackupProcess_OutputDataReceived(object sender, DataReceivedEventArgs args)
		{
			OutConcurrentText(Logs, args.Data);
		}

		private void BackupProcess_ErrorDataReceived(object sender, DataReceivedEventArgs args)
		{
			OutConcurrentText(Logs, args.Data);
		}

		protected Task<Process> RestoreAsync(string exeFileName, ObjectType objectType, FileFormat fileFormat,
			string dumpFileName, bool createDb, bool cleanDb)
		{
			return Task.Run(() => Restore(exeFileName, objectType, fileFormat, dumpFileName, CreateDb, CleanDb));
		}

		protected Process Restore(string exeFileName, ObjectType objectType, FileFormat fileFormat, string dumpFileName,
			bool createDb, bool cleanDb)
		{
			Helper.LogInfo(Messages.RestoreProcessStarted);

			var arguments = new StringBuilder();

			arguments.Append($"-h {Host}");
			arguments.Append($" -U {User}");
			arguments.Append($" -d {Database}");

			if (fileFormat != FileFormat.Plain)
				arguments.Append(" -v");

			if (fileFormat != FileFormat.Plain)
			{
				if (createDb)
					arguments.Append(" -C");

				if (cleanDb)
					arguments.Append(" -c");
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

			Process = Process.Start(info);

			if (Process == null) throw new NullReferenceException("Can't execute restore process");

			Process.BeginErrorReadLine();
			Process.Exited += RestoreProcess_Exited;
			Process.OutputDataReceived += RestoreProcess_OutputDataReceived;
			Process.ErrorDataReceived += RestoreProcess_ErrorDataReceived;
			Process.WaitForExit();

			if (Process.ExitCode == 0)
				ClearLogs();

			var text = new StringBuilder();

			text.Append($"{DateTime.Now:HH:mm:ss}\n{Messages.ResultCode}:\t{Process.ExitCode}");
			text.Append(Process.ExitCode == 0
				? $"\n\n{Messages.RestoreProcessSucceed}"
				: $"\n\n{Messages.RestoreProcessFailed}");

			OutConcurrentText(Logs, text.ToString());

			return Process;
		}

		private void RestoreProcess_Exited(object sender, EventArgs args)
		{
			Helper.LogInfo(Messages.RestoreProcessFinished);
		}

		private void RestoreProcess_ErrorDataReceived(object sender, DataReceivedEventArgs args)
		{
			OutConcurrentText(Logs, args.Data);
		}

		private void RestoreProcess_OutputDataReceived(object sender, DataReceivedEventArgs args)
		{
			OutConcurrentText(Logs, args.Data);
		}

		private void OutConcurrentText(ICollection<string> logs, string text)
		{
			if (Application.Current == null) return;

			var dispatcher = Application.Current.Dispatcher;
			dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart) (() =>
			{
				logs.Add(text);

				LogsListBoxService.ScrollToEnd();
			}));
		}

		private void ClearLogs()
		{
			var dispatcher = Application.Current.Dispatcher;
			dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart) (() =>
			{
				Logs.Clear();
			}));
		}

		#endregion
	}
}