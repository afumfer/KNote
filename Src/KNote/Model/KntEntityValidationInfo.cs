using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model;

public class KntEntityValidationInfo
{
    public string EntityDescription { get;  }
    public List<ValidationResult> ValidationResults { get; }
    
    public KntEntityValidationInfo (string entityDescription, List<ValidationResult> validationResults)
    {
        EntityDescription = entityDescription;
        ValidationResults = validationResults;
    }
}
