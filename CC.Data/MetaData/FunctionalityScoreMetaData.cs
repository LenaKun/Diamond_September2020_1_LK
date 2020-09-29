using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CC.Data.MetaData
{
    public class FunctionalityScoreMetaData
    {

        [DisplayName("Start Date")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        [Required]
        public DateTime StartDate { get; set; }

        [DisplayName("Diagnostic Score")]
        public decimal DiagnosticScore { get; set; }
    }
}
