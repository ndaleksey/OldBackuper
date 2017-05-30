using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Devart.Data.PostgreSql;
using DevExpress.Mvvm;
using NLog;
using Swsu.Tools.DbBackupper.Model;
using Swsu.Tools.DbBackupper.Properties;
using Swsu.Tools.DbBackupper.Service;

namespace Swsu.Tools.DbBackupper.ViewModel
{
    

    public class MainViewModel : CustomViewModel
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
        private FileFormat _restoreFileFormat;

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

			ChangeCultureCommand = new DelegateCommand(ChangeCulture, CanChangeCulture);

			try
            {
                DumpSchemes = new ObservableCollection<Node>();
                RestoreSchemes = new ObservableCollection<Node>();

                DbEncodings = new ObservableCollection<string>();

                //OutDumpFileName = Environment.CurrentDirectory + "\\" + DateTime.Today.ToString("dd-MM-yy") + ".dump";

                var encodings = Encoding.GetEncodings();

                foreach (var encoding in encodings)
                {
                    DbEncodings.Add(encoding.Name);
                }
				
                Helper.Logger.Log(LogLevel.Info, "Start programme success");
            }
            catch (Exception e)
            {
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
		#endregion

		#region Methods

		private async void OutConcurrentText(ICollection<string> logs, string text)
        {
            var dispatcher = Application.Current.Dispatcher;
            await dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart)(() =>
            {
                logs.Add(text);
                
//                DumpLogsListBoxService.ScrollToEnd();
//                RestoreLogsListBoxService.ScrollToEnd();
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
                            children.Add(new Node(table));
                        }

//                        dbSchemes.Add(new Node(schemeName, children));
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

		
        #endregion
    }
}