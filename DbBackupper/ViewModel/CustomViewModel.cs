using DevExpress.Mvvm;
using Swsu.Tools.DbBackupper.Infrastructure;

namespace Swsu.Tools.DbBackupper.ViewModel
{
	public class CustomViewModel : ViewModelBase
	{
		#region Fields
		private EWorkflowType _workflowType;
		#endregion
		#region Properties
		public EWorkflowType WorkflowType
		{
			get { return _workflowType; }
			set { SetProperty(ref _workflowType, value, nameof(WorkflowType)); }
		}
		#endregion
	}
}