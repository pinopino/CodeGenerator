using System;
using System.IO;

namespace Generator.Template
{
    public class EnumTemplate
    {
        private readonly static string _Enum_TEMPLATE = @"
        /// <summary>
	    /// {0}枚举
	    /// </summary>
	    public enum {1}
	    {{
            {2}
	    }}";

        public readonly static string Enum_TEMPLATE = File.ReadAllText(Path.Combine("Templates", "Enum", "ENUM_TEMPLATE.txt"));
    }
}
