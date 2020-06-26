using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto.Info
{
    public class NoteTaskInfoDto : KntModelBase
    {
        public Guid NoteTaskId { get; set; }
        public Guid NoteId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreationDateTime { get; set; }
        public DateTime ModificationDateTime { get; set; }
        public string Description { get; set; }
        public string Tags { get; set; }
        public int Priority { get; set; }
        public bool Resolved { get; set; }
        public double? EstimatedTime { get; set; }
        public double? SpentTime { get; set; }
        public double? DifficultyLevel { get; set; }
        public DateTime? ExpectedStartDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
