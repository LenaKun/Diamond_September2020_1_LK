using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CC.Web.Areas.Admin.Models
{
    public class AdminModelBase
    {
        public AdminModelBase()
        {
            Messages = new List<string>();
        }
        public Exception Exception { get; set; }
        public List<string> Messages { get; set; }
    }
}