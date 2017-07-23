using System.ComponentModel;
using Swsu.Tools.DbBackupper.Infrastructure;

namespace Swsu.Tools.DbBackupper.Model
{
    [TypeConverter(typeof(EnumValuesToStringNameConverter))]
	public enum FileFormat
    {
        [LocalizedDescription("PlainFormat", typeof(Properties.Resources))]
        Plain = 1,

        [LocalizedDescription("BinaryFormat", typeof(Properties.Resources))]
        Binary = 2,

        [LocalizedDescription("TarFormat", typeof(Properties.Resources))]
        Tar = 3
    }
}