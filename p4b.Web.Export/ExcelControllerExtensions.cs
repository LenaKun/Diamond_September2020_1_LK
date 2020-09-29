//------------------------------------------------------------------------------
// <copyright file="ExcelControllerExtensions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>The Export to Excel controller extension class.</summary>
namespace System.Web.Mvc
{
    using System.Linq;
	using System.Reflection;
    using System.Web.Mvc;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Collections.Generic;
    using p4b.Web.Export;

    /// <summary>
    /// Excel controller extensions class.
    /// </summary>
    public static class ExcelControllerExtensions
    {
        /// <summary>
        /// Controller Extension: Returns an Excel result constructor for returning values from rows. 
        /// </summary>
        /// <param name="controller">This controller.</param>
        /// <param name="fileName">Excel file name.</param>
        /// <param name="excelWorkSheetName">Excel worksheet name: default: sheet1.</param>
        /// <param name="rows">Excel row values.</param>
        /// <returns>Action result.</returns>
        public static ActionResult Excel<T>(this Controller controller, string fileName, string excelWorkSheetName, IEnumerable<T> rows)where T:class
        {
        
            return new ExcelResult<T>(fileName, rows);
        }

		public static ActionResult Csv<T>(this Controller controller, IEnumerable<T> records) where T : class
		{
			return new CsvResult<T>(records);
		}
		public static ActionResult Csv<T>(this Controller controller, IEnumerable<T> records, IEnumerable<PropertyInfo> properties) where T : class
		{
			return new CsvResult<T>(records, properties);
		}

      
    }
}