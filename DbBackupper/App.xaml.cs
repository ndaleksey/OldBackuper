﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows;
using Swsu.Tools.DbBackupper.Properties;
using Swsu.Tools.DbBackupper.Resources;
using Swsu.Tools.DbBackupper.Service;
using Swsu.Tools.DbBackupper.ViewModel;

namespace Swsu.Tools.DbBackupper
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
		private Mutex _mutex;

		protected override void OnStartup(StartupEventArgs e)
		{
			bool createdNew;
			_mutex = new Mutex(true, Process.GetCurrentProcess().ProcessName, out createdNew);

			if (!createdNew)
				Shutdown();

			CultureInfo.DefaultThreadCurrentCulture = Settings.Default.Culture;
			CultureInfo.DefaultThreadCurrentUICulture = Settings.Default.Culture;

			Thread.CurrentThread.CurrentCulture = Settings.Default.Culture;
			Thread.CurrentThread.CurrentUICulture = Settings.Default.Culture;

			AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

			AppDomain.CurrentDomain.ProcessExit += OnCurrentDomainProcessExit;
			
			Helper.LogInfo(Messages.StartApplication);
		}

	    private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs args)
	    {
		    Helper.LogInfo(((Exception)args.ExceptionObject).Message);
		    Helper.LogInfo(((Exception)args.ExceptionObject).StackTrace);
	    }

	    private void OnCurrentDomainProcessExit(object sender, EventArgs e)
		{
			// если была изменена культура
			if (MainViewModel.IsCultureChanged)
			{
				var process = Process.Start(Assembly.GetEntryAssembly().Location);
				process?.Dispose();
			}
		}
		

		protected override void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);
			Helper.LogInfo(Messages.StopApplication);
		}
	}
}
