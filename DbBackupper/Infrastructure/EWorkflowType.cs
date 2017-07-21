using System.ComponentModel;

namespace Swsu.Tools.DbBackupper.Infrastructure
{
	public enum EWorkflowType
	{
		NormalWork = 0,
		WorkWithDb = 1,
		LoadFromDb = 2,
		Backup = 3,
		Restore = 4
	}
}