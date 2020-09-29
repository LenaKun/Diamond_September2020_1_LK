using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CC.Data
{
	public partial class Currency
	{
		public static string[] ConvertableCurrencies { get { return new string[] { "USD", "EUR", "ILS", "AUD", "CAD", "GBP" }; } }
	}
}
