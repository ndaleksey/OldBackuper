using System.ComponentModel;
using Swsu.Tools.DbBackupper.Infrastructure;

namespace Swsu.Tools.DbBackupper.Model
{
    [TypeConverter(typeof(EnumValuesToStringNameConverter))]
	public enum FileFormat
    {
        [LocalizedDescription("PlainFormat", typeof(Properties.Resources))]
        Plain,

        [LocalizedDescription("CustomFormat", typeof(Properties.Resources))]
        Custom,

        [LocalizedDescription("TarFormat", typeof(Properties.Resources))]
        Tar
    }
}