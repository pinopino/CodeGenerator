using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console
{
    public class FieldViewModel
    {
        public string Field { get; set; }
        public string Type { get; set; }
        public dynamic  Collation { get; set; }
        public string Null { get; set; }
        public string Key { get; set; }
        public dynamic Default { get; set; }
        public string Extra { get; set; }
        public string Privileges { get; set; }
        public string Comment { get; set; }
    }
}
