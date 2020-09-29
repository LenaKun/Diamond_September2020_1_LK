using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC.Data;
using System.ComponentModel.DataAnnotations;

namespace CC.Web.Models
{
	public class DeletedDafIndexModel:DafIndexModelFilter
	{
		[Display(Name="DeletedAtFrom", ResourceType=typeof(global::Resources.Resource))]
		[UIHint("Date")]
		public DateTime? DeletedFrom { get; set; }

		[Display(Name="DeletedAtTo", ResourceType=typeof(global::Resources.Resource))]
		[UIHint("Date")]
		public DateTime? DeletedTo { get; set; }
	}
}