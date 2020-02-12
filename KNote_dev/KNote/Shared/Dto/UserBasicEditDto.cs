﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KNote.Shared.Dto
{
    public class UserBasicEditDto : KntModelBase
    {
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "* Attribute {0} is required ")]
        [MaxLength(32)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "* Attribute {0} is required ")]
        [MaxLength(256)]
        public string EMail { get; set; }

        [Required(ErrorMessage = "* Attribute {0} is required ")]
        [MaxLength(256)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "* Attribute {0} is required ")]
        public string RoleDefinition { get; set; }

        // TODO: descomentar y probar. 
        //public bool Disabled { get; set; }

    }
}