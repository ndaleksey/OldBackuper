using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DevExpress.Mvvm.UI;

namespace Swsu.Tools.DbBackupper.Service
{
    public class ListBoxService : ServiceBase, IListBoxService
    {
        public static readonly DependencyProperty ControlProperty = DependencyProperty.Register(
            nameof(Control),
            typeof (ListBox),
            typeof (ListBoxService),
            new PropertyMetadata());
        
        public ListBox Control
        {
            get { return (ListBox) GetValue(ControlProperty); }
            set { SetValue(ControlProperty, value); }
        }

        public void ScrollToEnd()
        {
            if (Control.Items.Count <= 0) return;

            var border = VisualTreeHelper.GetChild(Control, 0) as Decorator;

            var scroll = border?.Child as ScrollViewer;
            scroll?.ScrollToEnd();
        }
    }
}