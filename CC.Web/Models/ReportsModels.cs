using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CC.Web.Models
{
    public class ApprovalStatusRow
    {
        [Display(Name = "App #")]
        public string AppName { get; set; }

        [Display(Name = "Month From")]
        public string MonthFrom { get; set; }

        [Display(Name = "Month To")]
        public string MonthTo { get; set; }

        [Display(Name = "Agency Name")]
        public string AgencyName { get; set; }

        [Display(Name = "Current Approval Status")]
        public string CurrentApprovalStatus { get; set; }

        [Display(Name = "Date of Current Approval Status")]
        public DateTime DateOfCurrentStatus { get; set; }

        [Display(Name = "Previous Approval Status")]
        public string PreviousApprovalStatus { get; set; }

        [Display(Name = "Date of Previous Approval Status")]
        public DateTime? DateOfPreviousStatus { get; set; }

        [Display(Name = "Days Elapsed (full days)")]
        public string DaysElapsed { get; set; }

        [Display(Name = "Hours Elapsed (full hours)")]
        public string HoursElapsed { get; set; }
    }

    public class FunctionalityChangeRow
    {
        [Display(Name = "CC ID")]
        public int ClientId { get; set; }

        [Display(Name = "Agency")]
        public string AgencyName { get; set; }

        [Display(Name = "Active Score")]
        public decimal? ActiveScore { get; set; }

        [Display(Name = "Date of Active Score")]
        public DateTime? DateOfActiveScore { get; set; }

        [Display(Name = "Prior Score")]
        public decimal? PriorScore { get; set; }

        [Display(Name = "Date of Prior Score")]
        public DateTime? DateOfPriorScore { get; set; }

        [Display(Name = "Prior Prior Score")]
        public decimal? PriorPriorScore { get; set; }

        [Display(Name = "Date of Prior Prior Score")]
        public DateTime? DateOfPriorPriorScore { get; set; }
    }

    public class DeceasedDateEntryRow
    {
        [Display(Name = "CC ID")]
        public int ClientId { get; set; }

        [Display(Name = "Agency")]
        public string AgencyName { get; set; }

        [Display(Name = "Deceased Date")]
        public DateTime DeceasedDate { get; set; }

        [Display(Name = "Deceased Date Entered")]
        public DateTime DeceasedDateEntered { get; set; }

        [Display(Name = "Days from Actual to Entry (rounded up)")]
        public string Diff { get; set; }
    }
}