using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Shared.Dto
{
    public class SystemValueDto : KntModelBase
    {
        public Guid SystemValueId { get; set; }

        [Required(ErrorMessage = "* Attribute {0} is required ")]
        [MaxLength(256)]
        public string Key { get; set; }

        public string Value { get; set; }
    }
}
