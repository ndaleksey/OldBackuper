using System.ComponentModel;

namespace Swsu.Tools.DbBackupper.Infrastructure
{
	public enum EWorkflowType
	{
		[Description("Нормальная работа")]
		NormalWork = 0,

		[Description("Работа с БД")]
		WorkWithDb = 1,
		LoadFromDb = 2,
		SaveToDb = 3
	}
}