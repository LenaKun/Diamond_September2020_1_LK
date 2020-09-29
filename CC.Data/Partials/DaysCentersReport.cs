using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CC.Data
{
    public partial class DaysCentersReport : IValidatableObject
    {

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {

            using (var db = new ccEntities())
            {
                var client = (from c in db.Clients
                              where c.Id == this.ClientId
                              select new
                              {
                                  JoinDate = c.JoinDate,
                                  LeaveDate = c.LeaveDate,
                                  DeceasedDate = c.DeceasedDate,


                              }).SingleOrDefault();


                //it should be included in mainreport's period in case exists for all report types
                if (this.SubReport != null & client != null)
                {


                    if (this.SubReport.MainReport.End < client.JoinDate)
                    {
                        yield return new ValidationResult("Join Date must be before End of the report");


                    }
                    if (this.SubReport.MainReport.Start > client.LeaveDate)
                    {
                        yield return new ValidationResult("Leave Date must be after Start of the report");


                    }


                }



            }

        }

    }


}
