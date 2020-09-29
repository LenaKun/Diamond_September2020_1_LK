using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CC.Web.Models
{
    public class HistoryDataTablesModel:jQueryDataTableParamModel
    {
        public string FieldName { get; set; }
        public DateTime? fromDate { get; set; }
        public DateTime? toDate { get; set; }

    }
}