using System;
using System.IO;

namespace Generator.Template
{
    public class ModelTemplate
    {
        private readonly static string _CLASS_TEMPLATE = @"
	    /// <summary>
	    /// {0}实体
	    /// </summary>
	    [Serializable]
	    public partial class {1}{2}{3}{4}
	    {{
		    public {5}()
		    {{}}
                
            {6}
                
            {7}
	    }}";

        public readonly static string CLASS_TEMPLATE = File.ReadAllText(Path.Combine("Templates", "Model", "CLASS_TEMPLATE.txt"));
    }
}
