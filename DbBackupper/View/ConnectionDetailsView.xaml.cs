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
	/// Логика взаимодействия для ConnectionDetailsView.xaml
	/// </summary>
	public partial class ConnectionDetailsView : UserControl
	{
		public ConnectionDetailsView()
		{
			InitializeComponent();
		}

		private void Port_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if (!char.IsDigit(e.Text, 0))
				e.Handled = true;
		}

		private void Host_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if (!char.IsDigit(e.Text, 0) && e.Text[0] != '.')
				e.Handled = true;
		}
	}
}
