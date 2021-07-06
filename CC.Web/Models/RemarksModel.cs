using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using CC.Data;
using CC.Data.Services;

namespace CC.Web.Models
{
	public class CommentsModelBase : ModelBase
	{
		public int Id { get; set; }
		[Display(Name = "Ser")]
		public string AgenencyGroupName { get; set; }
		[Display(Name = "Financial Report Period")]
		[UIHint("MonthsRange")]
		public Range<DateTime> Period { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }

		public IEnumerable<CommentViewModel> Comments { get; set; }
		[Required]
		[DisplayName("Add New Remark:")]
		public MainReport MainReport { get; set; }

		[DataType(DataType.Html)]
		[System.Web.Mvc.AllowHtml]
		[Display(Name = "New Comment")]
		public virtual string NewComment { get; set; }
		public virtual string Title { get; set; }

		[Display(Name = "Post Approval Comment")]
		public virtual bool PostApprovalComment { get; set; }

		public virtual void Load(ccEntities db, IPermissionsBase permissions, int id)
		{
			this.Id = id;
			this.MainReport = db.MainReports.Where(permissions.MainReportsFilter).Single(f => f.Id == this.Id);
			this.AgenencyGroupName = MainReport.AppBudget.App.AgencyGroup.DisplayName;
			this.Start = MainReport.Start;
			this.End = MainReport.End;
			this.Period = new Range<DateTime>() { Start = MainReport.Start, End = MainReport.End };

		}
		public bool CanAddRemarks { get; set; }
	}

	public class CommentViewModel
	{


		[ScaffoldColumn(false)]
		public string Content { get; set; }
		[Display(Name="Content")]
		public System.Web.Mvc.MvcHtmlString HtmlContent { get { return System.Web.Mvc.MvcHtmlString.Create(this.Content); } }
		public string UserName { get; set; }
		public DateTime Date { get; set; }
		public bool IsFile { get; set; }
		public int Id { get; internal set; }
        public int Link { get; set; }

    }

	public class MainReportAgencyCommentsModel : CommentsModelBase
	{
		public MainReportAgencyCommentsModel()
			: base()
		{
			this.Title = "Agency Remarks";
		}
		public override void Load(ccEntities db, IPermissionsBase permissions, int id)
		{
			base.Load(db, permissions, id);

			List<CommentViewModel> commentsList = db.Comments.Where(f => f.MainReportAgencyComments.Any(m => m.Id == id)).OrderByDescending(f => f.Date)
				.Select(f => new CommentViewModel()
				{
					Content = f.Content,
					UserName = f.User.UserName,
					Date = f.Date,
					IsFile = f.IsFile,
                    Link = 0
				}).ToList();

            if ((FixedRoles)this.User.RoleId == FixedRoles.BMF)
            {
                for (var i = 0; i < commentsList.Count; i++ )
                {
                    if (commentsList[i].Content.Contains("Minutes") || commentsList[i].Content.Contains("Program Overview"))
                    {
                        commentsList.Remove(commentsList[i]);
                        i--;
                    }
                }
            }
            var count = 0;
            for (var i = 0; i < commentsList.Count; i++)

            {
                //LenaPDF
               
                if (commentsList[i].Content.Contains("pdf"))
                {
                    // commentsList[i].Content = commentsList[i].Content + "Link";
                    commentsList[i].Link = 7;
                    count = count + 1;

                    if (count == 2)
                    { break; }
                       
                }
            }


            this.Comments = commentsList;

			switch ((FixedRoles)this.User.RoleId)
			{
				case FixedRoles.Admin:
					this.CanAddRemarks = true;
					break;
				case FixedRoles.AgencyUser:
				case FixedRoles.Ser:
				case FixedRoles.AgencyUserAndReviewer:
				case FixedRoles.SerAndReviewer:
					this.CanAddRemarks = MainReport.EditableStatuses.Contains(this.MainReport.Status);
					break;
				default:
					this.CanAddRemarks = false;
					break;
			}
		}
	}

	public class MainReportPoCommentsModel : CommentsModelBase
	{
		public MainReportPoCommentsModel()
			: base()
		{
			this.Title = "PO Remarks";
		}
		public override void Load(ccEntities db, IPermissionsBase permissions, int id)
		{
			base.Load(db, permissions, id);

			this.Comments = db.Comments.Where(f => f.MainReportPoComments.Any(m => m.Id == id)).OrderByDescending(f => f.Date)
				.Select(f => new CommentViewModel()
				{
					Content = f.Content,
					UserName = f.User.UserName,
					Date = f.Date
				}).OrderByDescending(f => f.Date).ToList();

			switch ((FixedRoles)this.User.RoleId)
			{
				case FixedRoles.Admin:
					this.CanAddRemarks = true;
					break;
				case FixedRoles.RegionOfficer:
					this.CanAddRemarks = this.MainReport.Status == CC.Data.MainReport.Statuses.AwaitingProgramOfficerApproval;
					break;
				case FixedRoles.RegionAssistant:
					this.CanAddRemarks = this.MainReport.Status == CC.Data.MainReport.Statuses.AwaitingProgramAssistantApproval;
					break;
				case FixedRoles.GlobalOfficer:
					this.CanAddRemarks = this.MainReport.Status == CC.Data.MainReport.Statuses.AwaitingProgramOfficerApproval
						|| this.MainReport.Status == CC.Data.MainReport.Statuses.AwaitingProgramAssistantApproval;
					break;
				default:
					this.CanAddRemarks = false;
					break;
			}

		}
	}


	public class MainReportInternalCommentsModel : CommentsModelBase
	{
		public MainReportInternalCommentsModel()
			: base()
		{
			this.Title = "Internal Remarks";
		}
		public override void Load(ccEntities db, IPermissionsBase permissions, int id)
		{
			base.Load(db, permissions, id);

			this.Comments = db.Comments.Where(f => f.MainReportInternalComments.Any(m => m.Id == id)).OrderByDescending(f => f.Date)
				.Select(f => new CommentViewModel()
				{
					Content = f.Content,
					UserName = f.User.UserName,
					Date = f.Date
				}).OrderByDescending(f => f.Date).ToList();

			switch ((FixedRoles)this.User.RoleId)
			{
				case FixedRoles.Admin:
				case FixedRoles.RegionOfficer:
				case FixedRoles.GlobalOfficer:
					this.CanAddRemarks = true;
					break;
				default:
					this.CanAddRemarks = false;
					break;
			}

			
		}

	}

	public class MainReportPostApprovalCommentsModel : CommentsModelBase
	{
		public MainReportPostApprovalCommentsModel()
			: base()
		{
			this.Title = "Post Approval Remarks";
			this.CanAddRemarks = true;
			this.PostApprovalComment = true;
		}
		public override void Load(ccEntities db, IPermissionsBase permissions, int id)
		{
			base.Load(db, permissions, id);

			this.Comments = db.Comments.Where(f => f.MainReportPostApprovalComments.Any(m => m.Id == id)).OrderByDescending(f => f.Date)
				.Select(f => new CommentViewModel()
				{
					Id = f.Id,
					IsFile = f.IsFile,
					Content = f.Content,
					UserName = f.User.UserName,
					Date = f.Date
				}).OrderByDescending(f => f.Date).ToList();

            switch ((FixedRoles)this.User.RoleId)
            {
                case FixedRoles.Ser:
                case FixedRoles.SerAndReviewer:
                case FixedRoles.AgencyOfficer:
                case FixedRoles.AgencyUser:
                case FixedRoles.AgencyUserAndReviewer:
                    this.CanAddRemarks = false;
                    break;
            }
            }

	}
}