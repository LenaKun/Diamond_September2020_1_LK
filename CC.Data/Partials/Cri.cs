using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using CC.Data.Partials;

namespace CC.Data
{

	[MetadataType(typeof(CriMetadata))]
	public partial class Cri :ICri
	{
		public Cri()
		{
			this.Id = Guid.NewGuid();
		}

	}

}
