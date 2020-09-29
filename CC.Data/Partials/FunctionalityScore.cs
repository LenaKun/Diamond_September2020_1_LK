using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using CC.Data.MetaData;

namespace CC.Data
{
    [MetadataType(typeof(FunctionalityScoreMetaData))]
    partial class FunctionalityScore : IValidatableObject
    {
        public FunctionalityScore()
        {
            StartDate = DateTime.Now;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {			
            //1.4.2.7.1.6.3.	Validations- the application will not allow to enter any new records where the Functionality Level score start date is included in one or more reports that the client is included in.
            //1.4.2.7.1.6.4.	Remarks: In cases where the score for a client has been revised during a report period, the user will first have to insert the client score/s and the related starting dates, and only then to report the services received.


			if (this.ClientId == default(int))
			{
				yield return new ValidationResult("Client not found");
			}
			if (this.FunctionalityLevelId == default(int))
			{
				yield return new ValidationResult("No matching functionality level was found");
			}
            if (this.StartDate > DateTime.Now)
            {
                yield return new ValidationResult(string.Format("Start date must be less than or equal to {0}.", DateTime.Now.Date.ToShortDateString()));
            }
        }
    }	
}
