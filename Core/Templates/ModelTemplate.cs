using System.IO;

namespace Generator.Template
{
    public class ModelTemplate
    {
        public readonly static string CLASS = File.ReadAllText(Path.Combine("Templates", "Model", "CLASS.txt"));
        public readonly static string CLASS_WITH_TRACE = File.ReadAllText(Path.Combine("Templates", "Model", "CLASS_WITH_TRACE.txt"));
        public readonly static string ENTITY_CLASS = File.ReadAllText(Path.Combine("Templates", "Model", "ENTITY_CLASS.txt"));
        public readonly static string JOINED_CLASS = File.ReadAllText(Path.Combine("Templates", "Model", "JOINED_CLASS.txt"));
    }
}
