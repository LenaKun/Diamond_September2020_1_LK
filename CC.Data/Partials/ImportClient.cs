using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CC.Data
{
	partial class ImportClient
	{
		public ImportClient()
		{
			this.UpdatedAt = CreatedAt = DateTime.Now;
		}

	}
}
