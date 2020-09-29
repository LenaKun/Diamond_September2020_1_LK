using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace System
{
	public static class ExceptionsExtensions
	{
		public static bool IsUqViolation(this Exception ex)
		{
			if (ex is System.Data.UpdateException)
			{
				if (ex.InnerException != null)
				{
					return ex.InnerException.Message.StartsWith("Violation of UNIQUE KEY constraint", StringComparison.CurrentCultureIgnoreCase);
				}
			}
			return false;
		}

		public static IEnumerable<Exception> UnfoldInner(this Exception ex)
		{
			List<Exception> result = new List<Exception>();
			
			result.AddRange(GetInnerExceptions(ex).Take(5));
			return result;
		}

		public static IEnumerable<Exception> GetInnerExceptions(Exception ex)
		{
			var exceptions = new List<Exception>();
			while (ex != null && !exceptions.Contains(ex))
			{
				exceptions.Add(ex);
				yield return ex;
				ex = ex.InnerException;
			}
		}
	}

}