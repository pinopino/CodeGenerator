using System.IO;

namespace Generator.Template
{
    public class EnumTemplate
    {
        public readonly static string Enum_TEMPLATE = File.ReadAllText(Path.Combine("Templates", "Enum", "ENUM_TEMPLATE.txt"));
    }
}
