using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto
{
    public class KLogDto : KntModelBase
    {
        public Guid KLogId { get; set; }

        public Guid EntityId { get; set; }

        [Required(ErrorMessage = "* Attribute {0} is required ")]
        [MaxLength(64)]
        public string EntityName { get; set; }

        [Required(ErrorMessage = "* Attribute {0} is required ")]
        public DateTime RegistryDateTime { get; set; }

        [Required(ErrorMessage = "* Attribute {0} is required ")]
        public string RegistryMessage { get; set; }
    }
}