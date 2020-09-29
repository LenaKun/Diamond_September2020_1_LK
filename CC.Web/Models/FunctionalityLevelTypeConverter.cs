using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace CC.Web.Models
{
	class FunctionalityScoreTypeConverter:CsvHelper.TypeConversion.DecimalConverter
	{
        private List<CC.Data.FunctionalityLevel> funclevels;
        public FunctionalityScoreTypeConverter()
        {
            using (var db = new CC.Data.ccEntities())
            {
                funclevels = db.FunctionalityLevels.ToList();
            }
        }
		public override object ConvertFromString(System.Globalization.CultureInfo culture, string text)
		{
			var obj=  base.ConvertFromString(culture, text);
			if (obj == null) return obj;
			else
			{
				var id = (decimal)obj;
                CC.Data.FunctionalityLevel list;
                if (HttpContext.Current == null)
                {
                    list = funclevels.Where(s => s.Id == id).SingleOrDefault();
                }
                else
                {
                    list = Cache.GetCachedList<CC.Data.FunctionalityLevel>().Where(s => s.Id == id).SingleOrDefault();
                }
				if (list != null)
				{
					return list.Id;
				}
				else
				{
					return null;
				}
			
			}

		}
	}
}
