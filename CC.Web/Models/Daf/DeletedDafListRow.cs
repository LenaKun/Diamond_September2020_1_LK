using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CC.Web.Models
{
	public class DeletedDafListRowModel:DafIndexRowModel
	{
		public DateTime DeletedAt { get; set; }

		public DateTime CreatedAt { get; set; }

		public DateTime? UpdatedAt { get; set; }
	}
}