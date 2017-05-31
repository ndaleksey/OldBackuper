using System.Collections.ObjectModel;
using DevExpress.Mvvm;

namespace Swsu.Tools.DbBackupper.ViewModel
{
    public class Node : BindableBase
    {
        #region Fields

        private bool _isChecked;

        #endregion

        #region Properties

        public string Name { get; set; }

        public bool IsChecked
        {
            get { return _isChecked; }
            set { SetProperty(ref _isChecked, value, nameof(IsChecked), OnCheckedChanged); }
        }

        public ObservableCollection<Node> Children { get; } = new ObservableCollection<Node>();
        #endregion


        #region Constructors

	    public Node(string name)
	    {
		    Name = name;
	    }

	    #endregion

        #region Methods
        private void OnCheckedChanged()
        {
            if (Children == null) return;

            foreach (var child in Children)
                child.IsChecked = IsChecked;
        }

        #endregion
    }
}