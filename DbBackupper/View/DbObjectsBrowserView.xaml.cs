using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Swsu.Tools.DbBackupper.View
{
	/// <summary>
	/// Логика взаимодействия для DbObjectsBrowserView.xaml
	/// </summary>
	public partial class DbObjectsBrowserView : UserControl
	{
		public static readonly DependencyProperty IsBackupProperty = DependencyProperty.Register(nameof(IsBackup),
			typeof (bool), typeof (DbObjectsBrowserView), new PropertyMetadata(null));

		public bool IsBackup
		{
			get { return (bool) GetValue(IsBackupProperty); }
			set { SetValue(IsBackupProperty, value); }
		}

		public DbObjectsBrowserView()
		{
			InitializeComponent();
		}
	}
}