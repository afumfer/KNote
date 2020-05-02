﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Shared.Dto
{
    public class KAttributeTabulatedValueDto : KntModelBase
    {
        public Guid KAttributeTabulatedValueId { get; set; }

        public Guid KAttributeId { get; set; }

        //[Required(ErrorMessage = "* Attribute {0} is required ")]
        //[MaxLength(32)]
        //public string Key { get; set; }

        [Required(ErrorMessage = "* Attribute {0} is required ")]
        public string Value { get; set; }

        public int Order { get; set; }
    }
}
