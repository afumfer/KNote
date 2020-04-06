using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Shared.Dto
{
    public class NoteTaskDto : KntModelBase
    {
        public Guid NoteTaskId { get; set; }

        public Guid NoteId { get; set; }

        public Guid UserId { get; set; }

        [Required(ErrorMessage = "* Attribute {0} is required ")]
        public string Description { get; set; }

        [MaxLength(1024)]
        public string Tags { get; set; }

        public int Priority { get; set; }

        public bool Resolved { get; set; }

        public double? EstimatedTime { get; set; }

        public double? SpentTime { get; set; }

        public double? DifficultyLevel { get; set; }

        public DateTime? ExpectedStartDate { get; set; }

        public DateTime? ExpectedEndDate { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? ResolvedDate { get; set; }

        public NoteDto NoteDto { get; set; }
        public UserDto UserDto { get; set; }
    }
}
