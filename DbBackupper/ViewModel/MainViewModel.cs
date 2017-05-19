using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Devart.Data.PostgreSql;
using DevExpress.Mvvm;
using Microsoft.Win32;
using NLog;
using Swsu.Tools.DbBackupper.Model;
using Swsu.Tools.DbBackupper.Service;

namespace Swsu.Tools.DbBackupper.ViewModel
{
    

    internal enum ObjectType
    {
        DataOnly, SchemeOnly, Default
    }

    public class MainViewModel : ViewModelBase
    {
        #region Fields
        private PgSqlConnectionStringBuilder _dumpConnectionStringBuilder;
        private PgSqlConnectionStringBuilder _restoreConnectionStringBuilder;
        private string _selectedEncoding;
        private bool _allObjects;
        private bool _isShemeChecked;
        private bool _isTableChecked;
        private bool _isViewChecked;
        private bool _isTypeChecked;
        private bool _isTriggerChecked;
        private bool _isProcedureChecked;
        private bool _isUserChecked;
        private string _outDumpFileName;
        private string _inDumpFileName;

        private bool _onlyDumpData;
        private bool _onlyDumpScheme;

        private bool _onlyRestoreData;
        private bool _onlyRestoreScheme;

        private bool _isBlobs;
        private FileFormat _dumpFileFormat;
        private FileFormat _restoreFileFormat;
        #endregion

        public IListBoxService DumpLogsListBoxService => GetService<IListBoxService>("DumpLogsListBoxService");
        public IListBoxService RestoreLogsListBoxService => GetService<IListBoxService>("RestoreLogsListBoxService");

        #region Properties

        public string InDumpFileName
        {
            get { return _inDumpFileName; }
            set { SetProperty(ref _inDumpFileName, value, nameof(InDumpFileName)); }
        }

        public string OutDumpFileName
        {
            get { return _outDumpFileName; }
            set { SetProperty(ref _outDumpFileName, value, nameof(OutDumpFileName)); }
        }

        public string SelectedEncoding
        {
            get { return _selectedEncoding; }
            set { SetProperty(ref _selectedEncoding, value, nameof(SelectedEncoding)); }
        }

        public PgSqlConnectionStringBuilder DumpConnectionStringBuilder
        {
            get { return _dumpConnectionStringBuilder; }
            set { SetProperty(ref _dumpConnectionStringBuilder, value, nameof(DumpConnectionStringBuilder)); }
        }

        public PgSqlConnectionStringBuilder RestoreConnectionStringBuilder
        {
            get { return _restoreConnectionStringBuilder; }
            set { SetProperty(ref _restoreConnectionStringBuilder, value, nameof(RestoreConnectionStringBuilder)); }
        }

        public bool AllObjects
        {
            get { return _allObjects; }
            set { SetProperty(ref _allObjects, value, nameof(AllObjects), OnAllObjectsChecked); }
        }

        public bool IsShemeChecked
        {
            get { return _isShemeChecked; }
            set { SetProperty(ref _isShemeChecked, value, nameof(IsShemeChecked), OnObjectChecked); }
        }

        public bool IsTableChecked
        {
            get { return _isTableChecked; }
            set { SetProperty(ref _isTableChecked, value, nameof(IsTableChecked), OnObjectChecked); }
        }
        public bool IsViewChecked
        {
            get { return _isViewChecked; }
            set { SetProperty(ref _isViewChecked, value, nameof(IsViewChecked), OnObjectChecked); }
        }
        public bool IsTypeChecked
        {
            get { return _isTypeChecked; }
            set { SetProperty(ref _isTypeChecked, value, nameof(IsTypeChecked), OnObjectChecked); }
        }
        public bool IsTriggerChecked
        {
            get { return _isTriggerChecked; }
            set { SetProperty(ref _isTriggerChecked, value, nameof(IsTriggerChecked), OnObjectChecked); }
        }
        public bool IsProcedureChecked
        {
            get { return _isProcedureChecked; }
            set { SetProperty(ref _isProcedureChecked, value, nameof(IsProcedureChecked), OnObjectChecked); }
        }
        public bool IsUserChecked
        {
            get { return _isUserChecked; }
            set { SetProperty(ref _isUserChecked, value, nameof(IsUserChecked), OnObjectChecked); }
        }

        public bool OnlyDumpData
        {
            get { return _onlyDumpData; }
            set { SetProperty(ref _onlyDumpData, value, nameof(OnlyDumpData)); }
        }

        public bool OnlyDumpScheme
        {
            get { return _onlyDumpScheme; }
            set { SetProperty(ref _onlyDumpScheme, value, nameof(OnlyDumpScheme)); }
        }

        public bool OnlyRestoreData
        {
            get { return _onlyRestoreData; }
            set { SetProperty(ref _onlyRestoreData, value, nameof(OnlyRestoreData)); }
        }

        public bool OnlyRestoreScheme
        {
            get { return _onlyRestoreScheme; }
            set { SetProperty(ref _onlyRestoreScheme, value, nameof(OnlyRestoreScheme)); }
        }

        public bool IsBlobs
        {
            get { return _isBlobs; }
            set { SetProperty(ref _isBlobs, value, nameof(IsBlobs)); }
        }

        public FileFormat DumpFileFormat
        {
            get { return _dumpFileFormat; }
            set { SetProperty(ref _dumpFileFormat, value, nameof(DumpFileFormat)); }
        }

        public FileFormat RestoreFileFormat
        {
            get { return _restoreFileFormat; }
            set { SetProperty(ref _restoreFileFormat, value, nameof(RestoreFileFormat)); }
        }

        public ObservableCollection<Node> DumpSchemes { get; }
        public ObservableCollection<Node> RestoreSchemes { get; }
        public ObservableCollection<string> DbEncodings { get; }
        public ObservableCollection<string> DumpLogs { get; }
        public ObservableCollection<string> RestoreLogs { get; }

        #endregion

        #region Events

        public delegate void ProcessFinishHandler();

        public event ProcessFinishHandler OnProcessFinished;
        #endregion

        #region Commands
        public ICommand GetSchemesForDumpCommand { get; }
        public ICommand GetSchemesForRestoreCommand { get; }
        public ICommand RestoreBackupCommand { get; }
        public ICommand MakeBackupCommand { get; }
        public ICommand SaveDialogCommand { get; }
        public ICommand OpenDialogCommand { get; }
        public ICommand DumpPingCommand { get; }
        public ICommand RestorePingCommand { get; }
        #endregion

        #region Constructors
        public MainViewModel()
        {
            try
            {
                DumpLogs = new ObservableCollection<string>();
                RestoreLogs = new ObservableCollection<string>();

                DumpSchemes = new ObservableCollection<Node>();
                RestoreSchemes = new ObservableCollection<Node>();

                DbEncodings = new ObservableCollection<string>();

                //OutDumpFileName = Environment.CurrentDirectory + "\\" + DateTime.Today.ToString("dd-MM-yy") + ".dump";

                var encodings = Encoding.GetEncodings();

                foreach (var encoding in encodings)
                {
                    DbEncodings.Add(encoding.Name);
                }

                DumpConnectionStringBuilder = new PgSqlConnectionStringBuilder
                {
                    Host = "127.0.0.1",
                    Port = 5432,
                    Database = "db_name",
                    UserId = "user",
                    Password = "password",
                    Charset = "WIN1251"
                };

                RestoreConnectionStringBuilder = new PgSqlConnectionStringBuilder
                {
                    Host = "127.0.0.1",
                    Port = 5432,
                    Database = "db_name_rocovery",
                    UserId = "user",
                    Password = "password",
                    Charset = "WIN1251"
                };

                GetSchemesForDumpCommand = new DelegateCommand(GetSchemesForDumpMethod);
                GetSchemesForRestoreCommand = new DelegateCommand(GetSchemesForRestoreMethod);

                MakeBackupCommand = new DelegateCommand(OnMakeDumpClick, CanMakeDump);
                RestoreBackupCommand = new DelegateCommand(OnRestoreDumpClick, CanRestoreDump);

                SaveDialogCommand = new DelegateCommand(OnSaveDialogClick);
                OpenDialogCommand = new DelegateCommand(OnOpenDialogClick);

                DumpPingCommand = new DelegateCommand(DumpPingTestMethod);
                RestorePingCommand = new DelegateCommand(RestorePingTestMethod);

                Helper.Logger.Log(LogLevel.Info, "Start programme success");
            }
            catch (Exception e)
            {
                Helper.Logger.Log(LogLevel.Error, e);
            }
        }
        #endregion

        #region Methods

        private void MakeDump(string exeFileName, string host, string userName, string password, string dataBase,
            IReadOnlyCollection<string> schemes, ObjectType objectType, FileFormat fileFormat, string dumpFileName)
        {
            try
            {
                var arguments = new StringBuilder();

                arguments.Append($"-h {host}");
                arguments.Append($" -U {userName}");
                arguments.Append($" -d {dataBase}");
                arguments.Append($" -f \"{dumpFileName}\"");

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

                if (IsBlobs)
                {
                    arguments.Append(" -b");
                }

                if (schemes.Count > 0)
                {
                    foreach (var scheme in schemes)
                    {
                        arguments.Append($" -n {scheme}");
                    }
                }

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
                    EnvironmentVariables = { { "PGPASSWORD", password } },
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

                    OutConcurrentText(DumpLogs, text.ToString());
                }
            }
            catch (Exception e)
            {
                Helper.Logger.Log(LogLevel.Error, e);
                throw;
            }
        }

        private async void OutConcurrentText(ICollection<string> logs, string text)
        {
            var dispatcher = Application.Current.Dispatcher;
            await dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)(() =>
            {
                logs.Add(text);
                
                DumpLogsListBoxService.ScrollToEnd();
                RestoreLogsListBoxService.ScrollToEnd();
            }));

            //await Task.Run(()=> { OnProcessFinished?.Invoke(); });
        }
        
        private void OnDumpOutputDataReceived(object sender, DataReceivedEventArgs args)
        {
            OutConcurrentText(DumpLogs, args.Data);
        }
        private void OnRestoreOutputDataReceived(object sender, DataReceivedEventArgs args)
        {
            OutConcurrentText(RestoreLogs, args.Data);
        }

        private bool CanMakeDump()
        {
            return !string.IsNullOrEmpty(OutDumpFileName);
        }

        private async void OnMakeDumpClick()
        {
            DumpLogs.Clear();
            
            var schemes = new List<string>();

            foreach (var scheme in DumpSchemes)
            {
                if (scheme.IsChecked)
                {
                    schemes.Add(scheme.Name);
                }
            }
            
            var dumpExeFilePath = $"{Environment.CurrentDirectory}\\Tools\\pg_dump.exe";
//            var dumpExeFilePath = @"C:\Program Files\PostgreSQL\9.5\bin\pg_dump.exe";

            await Task.Run(() =>
            {
                MakeDump(dumpExeFilePath,
                    DumpConnectionStringBuilder.Host, DumpConnectionStringBuilder.UserId,
                    DumpConnectionStringBuilder.Password,
                    DumpConnectionStringBuilder.Database, schemes, OnlyDumpData
                        ? ObjectType.DataOnly
                        : OnlyDumpScheme ? ObjectType.SchemeOnly : ObjectType.Default, DumpFileFormat, OutDumpFileName);
            });
        }

        private void Restore(string exeFileName, string host, string userName, string password, string dataBase,
            ObjectType objectType, FileFormat fileFormat, string dumpFileName)
        {
            try
            {
                var arguments = new StringBuilder();

                arguments.Append($"-h {host}");
                arguments.Append($" -U {userName}");
                arguments.Append($" -d {dataBase}");

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
                    EnvironmentVariables = { { "PGPASSWORD", password } },
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

                    OutConcurrentText(RestoreLogs, text.ToString());
                }
            }
            catch (Exception e)
            {
                Helper.Logger.Log(LogLevel.Error, e);
            }
        }

        private bool CanRestoreDump()
        {
            return !string.IsNullOrEmpty(InDumpFileName);
        }

        private void OnRestoreDumpClick()
        {
            RestoreLogs.Clear();

            Task.Run(() =>
            {
//                var restoreFileName = @"C:\Program Files\PostgreSQL\9.5\bin\pg_restore.exe";
                var restoreFileName = $"{Environment.CurrentDirectory}\\Tools\\pg_restore.exe";

                if (RestoreFileFormat == FileFormat.Plain)
                {
//                    restoreFileName = @"C:\Program Files\PostgreSQL\9.5\bin\psql.exe";
                    restoreFileName = $"{Environment.CurrentDirectory}\\Tools\\psql.exe";
                }

                Restore(restoreFileName,
                    IPAddress.Loopback.ToString(), RestoreConnectionStringBuilder.UserId, RestoreConnectionStringBuilder.Password,
                    RestoreConnectionStringBuilder.Database, OnlyRestoreData
                        ? ObjectType.DataOnly
                        : OnlyRestoreScheme ? ObjectType.SchemeOnly : ObjectType.Default, RestoreFileFormat,
                    InDumpFileName);
            });
        }

        private void OnAllObjectsChecked()
        {
            if (AllObjects)
            {
                IsProcedureChecked = IsShemeChecked = IsTableChecked = IsTriggerChecked = IsTypeChecked = IsUserChecked = IsViewChecked = true;
                AllObjects = true;
            }
            /*else
            {
                IsProcedureChecked =
                    IsShemeChecked =
                        IsTableChecked = IsTriggerChecked = IsTypeChecked = IsUserChecked = IsViewChecked = false;
            }*/
        }

        private void OnObjectChecked()
        {
            AllObjects = false;
        }
        
        private void GetSchemesForDumpMethod()
        {
            var error = "";
            var schemes = GetSchemas(DumpConnectionStringBuilder, ref error).ToArray();

            DumpSchemes.Clear();

            foreach (var scheme in schemes)
            {
                DumpSchemes.Add(scheme);
            }

            DumpLogs.Clear();
            DumpLogs.Add(error);
        }

        private void GetSchemesForRestoreMethod()
        {
            var error = "";
            var schemes = GetSchemas(RestoreConnectionStringBuilder, ref error).ToArray();

            RestoreSchemes.Clear();

            foreach (var scheme in schemes)
            {
                RestoreSchemes.Add(scheme);
            }

            RestoreLogs.Clear();
            RestoreLogs.Add(error);
        }

        private IEnumerable<Node> GetSchemas(DbConnectionStringBuilder connectionStringBuilder, ref string errorLog)
        {
            var dbSchemes = new ObservableCollection<Node>();
            var connectionString = connectionStringBuilder.ConnectionString;

            using (var connection = new PgSqlConnection(connectionString))
            {
                var schemes = PgSqlUtils.GetSchemas(connectionString);

                Debug.WriteLine(PgSqlMetaDataCollectionNames.Schemas);

                try
                {
                    connection.Open();

                    foreach (var scheme in schemes)
                    {
                        var schemeName = PgSqlUtils.UnQuoteName(scheme);

                        var tables = new ObservableCollection<string>(GetTablesByScheme(connectionString, schemeName));
                        var children = new ObservableCollection<Node>();

                        foreach (var table in tables)
                        {
                            children.Add(new Node(table, null));
                        }

                        dbSchemes.Add(new Node(schemeName, children));
                    }
                }
                catch (Exception e)
                {
                    
                    errorLog = $"Ошибка подключения:\t{e.Message}";
                }
            }

            return dbSchemes;
        }

        private IEnumerable<string> GetTablesByScheme(string connectionString, string schemeName)
        {
            var tables = new List<string>();

            using (var connection = new PgSqlConnection(connectionString))
            {
                connection.Open();

                var tableNumber = connection.GetSchema("Tables", new[] {schemeName, "%"}).Rows.Count;

                for (var i = 0; i < tableNumber; i++)
                {
                    var tableName = connection.GetSchema("Tables", new[] {schemeName, "%"}).Rows[i].ItemArray[2].ToString();
                    tables.Add(tableName);
                }

                connection.Close();
            }

            return tables;
        }

        private void OnMakeBackupClick()
        {
            using (var connection = new PgSqlConnection(DumpConnectionStringBuilder.ConnectionString))
            {
                connection.Open();

                var directory = $"d:\\{DumpConnectionStringBuilder.Database}";
                var tables = new StringBuilder();

                try
                {
                    Directory.CreateDirectory(directory);

                    var dump = new PgSqlDump(connection);

                    foreach (var scheme in DumpSchemes)
                    {
                        if (!scheme.IsChecked)
                        {
                            continue;
                        }

                        foreach (var table in scheme.Children)
                        {
                            if (!table.IsChecked)
                            {
                                continue;
                            }

                            tables.Append($"{scheme.Name}.{table.Name};");
                        }

                        dump.IncludeDrop = true;
                    }
                    dump.Tables = tables.ToString();

                    if (!AllObjects)
                    {
                        dump.ObjectTypes = SetFlag(dump.ObjectTypes, PgSqlDumpObjects.Schemas, IsShemeChecked);
                        dump.ObjectTypes = SetFlag(dump.ObjectTypes, PgSqlDumpObjects.Tables, IsTableChecked);
                        dump.ObjectTypes = SetFlag(dump.ObjectTypes, PgSqlDumpObjects.Views, IsViewChecked);
                        dump.ObjectTypes = SetFlag(dump.ObjectTypes, PgSqlDumpObjects.Types, IsTypeChecked);
                        dump.ObjectTypes = SetFlag(dump.ObjectTypes, PgSqlDumpObjects.Triggers, IsTriggerChecked);
                        dump.ObjectTypes = SetFlag(dump.ObjectTypes, PgSqlDumpObjects.Users, IsUserChecked);
                    }
                    else
                    {
                        dump.ObjectTypes = PgSqlDumpObjects.All;
                    }

                    OutDumpFileName = OutDumpFileName.Trim();

                    if (string.IsNullOrEmpty(OutDumpFileName))
                    {
                        MessageBox.Show("Не удалось сохранить файл дампа. Указано неверное имя файла");
                        return;
                    }

                    using (var stream = new FileStream(OutDumpFileName, FileMode.Create))
                    {
                        dump.Backup(stream);
                        stream.Close();
                        MessageBox.Show("Файл дампа успешно создан");
                    }
                }
                catch (IOException e)
                {
                    Debug.WriteLine(e.StackTrace);
                }

                connection.Close();
            }
        }

        private void OnSaveDialogClick()
        {
            var dialog = new SaveFileDialog();

            var date = DateTime.Today.Date;

            if (DumpFileFormat == FileFormat.Plain)
            {
                dialog.FileName = date.ToString("dd-MM-yy") + ".sql";
                dialog.Filter = dialog.DefaultExt = "Файлы запросов (*.sql)|*.sql";
            }
            else
            {
                dialog.FileName = date.ToString("dd-MM-yy") + ".dump";
                dialog.Filter = dialog.DefaultExt = "Файлы резервных копий (*.dump)|*.dump";
            }
            

            if ((bool) dialog.ShowDialog())
            {
                OutDumpFileName = dialog.FileName;
            }
        }

        private void OnOpenDialogClick()
        {
            var dialog = new OpenFileDialog();

            if (RestoreFileFormat == FileFormat.Plain)
            {
                dialog.Filter = dialog.DefaultExt = "Файлы запросов (*.sql)|*.sql";
            }
            else
            {
                dialog.Filter = dialog.DefaultExt = "Файлы резервных копий (*.dump)|*.dump";
            }

            if ((bool) dialog.ShowDialog())
            {
                InDumpFileName = dialog.FileName;
            }
        }

        private void DumpPingTestMethod()
        {
            MessageBox.Show(PingToHost(IPAddress.Parse(DumpConnectionStringBuilder.Host)).ToString());
        }

        private void RestorePingTestMethod()
        {
            MessageBox.Show(PingToHost(IPAddress.Parse(RestoreConnectionStringBuilder.Host)).ToString());
        }

        private static IPStatus? PingToHost(IPAddress host)
        {
            return new Ping().Send(host)?.Status;
        }

        private static PgSqlDumpObjects SetFlag(PgSqlDumpObjects flags, PgSqlDumpObjects flag, bool value)
        {
            if (value)
            {
                flags |= flag;
            }
            else
            {
                flags &= ~flag;
            }

            return flags;
        }

        #endregion
    }
}