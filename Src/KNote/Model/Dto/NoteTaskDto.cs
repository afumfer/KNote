using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto
{
    public class NoteTaskDto : SmartModelDtoBase
    {        
        #region Property definitions

        private Guid _noteTaskId;        
        public Guid NoteTaskId
        {
            get { return _noteTaskId; }
            set
            {
                if (_noteTaskId != value)
                {
                    _noteTaskId = value;
                    OnPropertyChanged("NoteTaskId");
                }
            }
        }

        private Guid _noteId;        
        public Guid NoteId
        {
            get { return _noteId; }
            set
            {
                if (_noteId != value)
                {
                    _noteId = value;
                    OnPropertyChanged("NoteId");
                }
            }
        }

        private Guid _userId;        
        public Guid UserId
        {
            get { return _userId; }
            set
            {
                if (_userId != value)
                {
                    _userId = value;
                    OnPropertyChanged("UserId");
                }
            }
        }

        private DateTime _creationDateTime;
        public DateTime CreationDateTime
        {
            get { return _creationDateTime; }
            set
            {
                if (_creationDateTime != value)
                {
                    _creationDateTime = value;
                    OnPropertyChanged("CreationDateTime");
                }
            }
        }


        private DateTime _modificationDateTime;
        public DateTime ModificationDateTime
        {
            get { return _modificationDateTime; }
            set
            {
                if (_modificationDateTime != value)
                {
                    _modificationDateTime = value;
                    OnPropertyChanged("ModificationDateTime");
                }
            }
        }

        private string _description;
        [Required(ErrorMessage = KMSG)]
        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged("Description");
                }
            }
        }

        private string _tags;
        [MaxLength(1024)]
        public string Tags
        {
            get { return _tags; }
            set
            {
                if (_tags != value)
                {
                    _tags = value;
                    OnPropertyChanged("Tags");
                }
            }
        }

        private int _priority;
        public int Priority
        {
            get { return _priority; }
            set
            {
                if (_priority != value)
                {
                    _priority = value;
                    OnPropertyChanged("Priority");
                }
            }
        }

        private bool _resolved;
        public bool Resolved
        {
            get { return _resolved; }
            set
            {
                if (_resolved != value)
                {
                    _resolved = value;
                    OnPropertyChanged("Resolved");
                }
            }
        }

        private double? _estimatedTime;
        public double? EstimatedTime
        {
            get { return _estimatedTime; }
            set
            {
                if (_estimatedTime != value)
                {
                    _estimatedTime = value;
                    OnPropertyChanged("EstimatedTime");
                }
            }
        }

        private double? _spentTime;
        public double? SpentTime
        {
            get { return _spentTime; }
            set
            {
                if (_spentTime != value)
                {
                    _spentTime = value;
                    OnPropertyChanged("SpentTime");
                }
            }
        }

        private double? _difficultyLevel;
        public double? DifficultyLevel
        {
            get { return _difficultyLevel; }
            set
            {
                if (_difficultyLevel != value)
                {
                    _difficultyLevel = value;
                    OnPropertyChanged("DifficultyLevel");
                }
            }
        }

        private DateTime? _expectedStartDate;
        public DateTime? ExpectedStartDate
        {
            get { return _expectedStartDate; }
            set
            {
                if (_expectedStartDate != value)
                {
                    _expectedStartDate = value;
                    OnPropertyChanged("ExpectedStartDate");
                }
            }
        }

        private DateTime? _expectedEndDate;
        public DateTime? ExpectedEndDate
        {
            get { return _expectedEndDate; }
            set
            {
                if (_expectedEndDate != value)
                {
                    _expectedEndDate = value;
                    OnPropertyChanged("ExpectedEndDate");
                }
            }
        }

        private DateTime? _startDate;
        public DateTime? StartDate
        {
            get { return _startDate; }
            set
            {
                if (_startDate != value)
                {
                    _startDate = value;
                    OnPropertyChanged("StartDate");
                }
            }
        }

        private DateTime? _endDate;
        public DateTime? EndDate
        {
            get { return _endDate; }
            set
            {
                if (_endDate != value)
                {
                    _endDate = value;
                    OnPropertyChanged("EndDate");
                }
            }
        }

        public string UserFullName { get; set; }

        #endregion 

        #region Validations

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // ---
            // Capture the validations implemented with attributes.
            // ---

            Validator.TryValidateProperty(this.Description,
               new ValidationContext(this, null, null) { MemberName = "Description" },
               results);

            Validator.TryValidateProperty(this.Tags,
               new ValidationContext(this, null, null) { MemberName = "Tags" },
               results);

            //----
            // Specific validations
            //----


            if (ModificationDateTime < CreationDateTime)
            {
                results.Add(new ValidationResult
                 ("KMSG: Modification date cannot be greater than creation date "
                 , new[] { "ModificationDateTime", "CreationDateTime" }));
            }

            if (ExpectedStartDate != null && ExpectedEndDate != null)
                if (ExpectedStartDate > ExpectedEndDate)
                {
                    results.Add(new ValidationResult
                     ("KMSG: The end date cannot be higher than the start date"
                     , new[] { "ExpectedStartDate", "ExpectedEndDate" }));
                }

            if (StartDate != null && EndDate != null)
                if (StartDate > EndDate)
                {
                    results.Add(new ValidationResult
                     ("KMSG: The start date cannot be higher than the resolution date "
                     , new[] { "StartDate", "EndDate" }));
                }

            if (Resolved == true && EndDate == null)
            {
                results.Add(new ValidationResult
                 ("KMSG: If the resolved indicator is active, you must enter a resolution date "
                 , new[] { "Resolved", "EndDate" }));
            }

            // ---
            // Return List<ValidationResult>()
            // ---           

            return results;
        }

        #endregion 
    }
}
