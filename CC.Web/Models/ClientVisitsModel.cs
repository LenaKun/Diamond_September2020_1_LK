using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CC.Data;


namespace CC.Web.Models
{
    public class ClientsVisitsModel : ModelBase
    {
        /// <summary>
        /// current data
        /// </summary>
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int IsraelId { get; set; }

		public string ClientName { get; set; }

        public string ClientLastName { get; set; }

		public string ClientFirstName { get; set; }
		public string ClientApprovalStatus { get; set; }

        public List<bool> dlist = new List<bool>();

        public int VisitsCount { get; set; }
		public decimal? Amount { get; set; }
    }

    public class ClientsVisitsByDays : ModelBase
    {
        /// <summary>
        /// current data
        /// </summary>
        public int SubReportId { get; set; }/// 
        public int ClientId { get; set; }
        public int Id { get; set; }
        public int IsraelId { get; set; }

        public int selMonth { get; set; }

        public int selYear { get; set; }
        public string ClientName { get; set; }

        public bool dlist1 { get; set;}
        public bool dlist2 { get; set;}

        public bool dlist3 { get; set; }
        public bool dlist4 { get; set; }

        public bool dlist5 { get; set; }
        public bool dlist6 { get; set; }

        public bool dlist7 { get; set; }
        public bool dlist8 { get; set; }

        public bool dlist9{ get; set; }
        public bool dlist10 { get; set; }

        public bool dlist11 { get; set; }
        public bool dlist12 { get; set; }

        public bool dlist13 { get; set; }
        public bool dlist14 { get; set; }

        public bool dlist15 { get; set; }
        public bool dlist16 { get; set; }

        public bool dlist17 { get; set; }
        public bool dlist18 { get; set; }

        public bool dlist19 { get; set; }
        public bool dlist20 { get; set; }

        public bool dlist21 { get; set; }
        public bool dlist22 { get; set; }

        public bool dlist23 { get; set; }
        public bool dlist24 { get; set; }

        public bool dlist25 { get; set; }
        public bool dlist26 { get; set; }

        public bool dlist27 { get; set; }
        public bool dlist28 { get; set; }

        public bool dlist29 { get; set; }
        public bool dlist30 { get; set; }

        public bool dlist31 { get; set; }

        public int VisitsCount { get; set; }

    }

}