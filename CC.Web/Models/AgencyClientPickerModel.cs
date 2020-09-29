using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CC.Web.Models
{
    public class AgencyClientPickerModel
    {
        public int? AgencyGroupId { get; set; }
        public int? AgencyId { get; set; }
        public int? ClientId { get; set; }
    }
}