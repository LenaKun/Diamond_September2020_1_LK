using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CC.Web.Models
{
	/// <summary>
	/// Class that encapsulates most common parameters sent by DataTables plugin
	/// </summary>
	/// 
	[CustomBinder(typeof(jQueryDataTableParamModelBinder))]
	public class jQueryDataTableParamModel
	{
		/// <summary>
		/// Request sequence number sent by DataTable,
		/// same value must be returned in response
		/// </summary>       
		public string sEcho { get; set; }

		/// <summary>
		/// Text used for filtering
		/// </summary>
		public string sSearch { get; set; }

		/// <summary>
		/// Number of records that should be shown in table
		/// </summary>
		public int iDisplayLength { get; set; }

		/// <summary>
		/// First record that should be shown(used for paging)
		/// </summary>
		public int iDisplayStart { get; set; }

		/// <summary>
		/// Number of columns in table
		/// </summary>
		public int iColumns { get; set; }

		/// <summary>
		/// Number of columns that are used in sorting
		/// </summary>
		public int iSortingCols { get; set; }

		/// <summary>
		/// Comma separated list of column names
		/// </summary>
		public string sColumns { get; set; }

		/// <summary>
		/// as of cycle1, most of the data is prefiltered with clientid
		/// </summary>
		public int ClientId { get; set; }

		public int iSortCol_0 { get; set; }
		public int iSortCol_1 { get; set; }
		public int iSortCol_2 { get; set; }
		public string sSortDir_0 { get; set; }
		public string sSortDir_1 { get; set; }
		public string sSortDir_2 { get; set; }

		public string mDataProp_0 { get; set; }
		public string mDataProp_1 { get; set; }
		public string mDataProp_2 { get; set; }
		public string mDataProp_3 { get; set; }
		public string mDataProp_4 { get; set; }
		public string mDataProp_5 { get; set; }
		public string mDataProp_6 { get; set; }
		public string mDataProp_7 { get; set; }
		public string mDataProp_8 { get; set; }
		public string mDataProp_9 { get; set; }

		public Dictionary<int, string> mDataProps
		{
			get
			{
				var res = new Dictionary<int, string>();
				res.Add(0, this.mDataProp_0);
				res.Add(1, this.mDataProp_1);
				res.Add(2, this.mDataProp_2);
				res.Add(3, this.mDataProp_3);
				res.Add(4, this.mDataProp_4);
				res.Add(5, this.mDataProp_5);
				res.Add(6, this.mDataProp_6);
				res.Add(7, this.mDataProp_7);
				res.Add(8, this.mDataProp_8);
				res.Add(9, this.mDataProp_9);

				return res;
			}
		}




		public bool bSortDir_0 { get { return this.sSortDir_0 == "asc"; } }
		public string sSortCol_0 { get; set; }
		public bool bSortDir_1 { get { return this.sSortDir_1 == "asc"; } }
		public string sSortCol_1 { get; set; }

	}

	public class jQueryDataTableColumn
	{
		public string sType { get; set; }
		public string sName { get; set; }
		public string sTitle { get; set; }
		public string format { get; set; }
	}
	/// <summary>
	/// Class that contains the data to be returned to the client
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class jQueryDataTableParamsWithData<T> : jQueryDataTableParamModel where T : class
	{
		public jQueryDataTableParamsWithData()
		{

		}
		public int iTotalRecords { get; set; }
		public int iTotalDisplayRecords { get; set; }
		public IEnumerable<T> aaData { get; set; }


	}

	public class jQueryDataTableResult
	{
		public string sEcho { get; set; }
		public int iTotalRecords { get; set; }
		public int iTotalDisplayRecords { get; set; }
		public object aaData { get; set; }
	}
	public class jQueryDataTableResult<T>
	{
		public string sEcho { get; set; }
		public int iTotalRecords { get; set; }
		public int iTotalDisplayRecords { get; set; }
		public IEnumerable<T> aaData { get; set; }
	}
}