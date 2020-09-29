using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using CC.Data.MetaData;

namespace CC.Data
{
	[MetadataType(typeof(FunctionalityLevelMetaData))]
	public partial class FunctionalityLevel
	{
		public static IQueryable<FunctionalityLevel> GetLevelsByScore(IQueryable<FunctionalityLevel> fls, FunctionalityScore score)
		{
			return fls.OrderByDescending(f => f.HcHoursLimit)
				.Where(f=>score.StartDate >= f.StartDate)
				.Where(f => 
							(score.DiagnosticScore >= f.MinScore && score.DiagnosticScore <= f.MaxScore));
		}
		public static FunctionalityLevel GetLevelByScore(IQueryable<FunctionalityLevel> fls, FunctionalityScore score)
		{
			return GetLevelsByScore(fls, score).SingleOrDefault();
		}
		public static FunctionalityLevel GetLevelByScore(IEnumerable<FunctionalityLevel> fls, FunctionalityScore score)
		{
			return GetLevelByScore(fls.AsQueryable(), score);
		}

	}
}
