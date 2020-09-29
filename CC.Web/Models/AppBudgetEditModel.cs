using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC.Data;

namespace CC.Web.Models
{
    public class AppBudgetEditModel:AppBudgetDetailsModel 
    {
        public AppBudget AppBudget { get; set; }
        public bool  Updateable{get{return this.User.RoleId==(int)FixedRoles.Admin;}}
        
    }
}