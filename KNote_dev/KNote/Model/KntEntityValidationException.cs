using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model
{
    public class KntEntityValidationException :  Exception
    {
        public List<KntEntityValidationInfo> ValidationResults { get;  }

        public KntEntityValidationException()
            : base()
        {
        }

        public KntEntityValidationException(string message)
            : base(message)
        {
        }

        public KntEntityValidationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public KntEntityValidationException(List<KntEntityValidationInfo> validationResults) 
            : base ("ValidationException, see ValidationResults property")
        {
            ValidationResults = validationResults;
        }
    }
}
