using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CC.Web.Models
{
    public class EditableDataTableUpdateModel
    {
        public int columnId { get; set; }
        public string columnName { get; set; }
        public int columnPosition { get; set; }
        public int id { get; set; }
        public int rowId { get; set; }
        public string value { get; set; }
    }
}