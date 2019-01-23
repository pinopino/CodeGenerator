using System.IO;

namespace Generator.Template
{
    public class ModelTemplate
    {
        public readonly static string CLASS_TEMPLATE = File.ReadAllText(Path.Combine("Templates", "Model", "CLASS_TEMPLATE.txt"));
    }
}
