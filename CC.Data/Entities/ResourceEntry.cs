using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resources.Entities
{
    
    public class ResourceEntry
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Culture { get; set; }
        public string Type { get; set; }


        public ResourceEntry() {
            Type = "string";  
        }
    }
}
