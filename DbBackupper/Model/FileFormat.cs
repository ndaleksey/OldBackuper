using System.ComponentModel;

namespace Swsu.Tools.DbBackupper.Model
{
    [TypeConverter(typeof(EnumValuesToStringNameConverter))]
    public enum FileFormat
    {
        [Description("Простой")]
        Plain,
        [Description("Настраиваемый")]
        Custom,
        [Description("Tar")]
        Tar
    }
}