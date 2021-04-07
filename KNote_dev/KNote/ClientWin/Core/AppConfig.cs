using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.Model;

namespace KNote.ClientWin.Core
{
    [Serializable]
    public class AppConfig : DtoModelBase
	{
        #region Properties 

        private DateTime _lastDateTimeStart;
        public DateTime LastDateTimeStart
        {
            get { return _lastDateTimeStart; }
            set
            {
                if (_lastDateTimeStart != value)
                {
                    _lastDateTimeStart = value;
                    OnPropertyChanged("LastDateTimeStart");
                }
            }
        }

        private int _runCounter;
        public int RunCounter
        {
            get { return _runCounter; }
            set
            {
                if (_runCounter != value)
                {
                    _runCounter = value;
                    OnPropertyChanged("RunCounter");
                }
            }
        }

        private string _logFile;
        public string LogFile
        {
            get { return _logFile; }
            set
            {
                if (_logFile != value)
                {
                    _logFile = value;
                    OnPropertyChanged("LogFile");
                }
            }
        }

        private bool _logActivated;
        public bool LogActivated
        {
            get { return _logActivated; }
            set
            {
                if (_logActivated != value)
                {
                    _logActivated = value;
                    OnPropertyChanged("LogActivated");
                }
            }
        }

        private bool _alarmActivated;
        public bool AlarmActivated
        {
            get { return _alarmActivated; }
            set
            {
                if (_alarmActivated != value)
                {
                    _alarmActivated = value;
                    OnPropertyChanged("AlarmActivated");
                }
            }
        }

        private bool _autoSaveActivated;
        public bool AutoSaveActivated
        {
            get { return _autoSaveActivated; }
            set
            {
                if (_autoSaveActivated != value)
                {
                    _autoSaveActivated = value;
                    OnPropertyChanged("AutoSaveActivated");
                }
            }
        }

        private int _alarmSeconds;
        public int AlarmSeconds
        {
            get { return _alarmSeconds; }
            set
            {
                if (_alarmSeconds != value)
                {
                    _alarmSeconds = value;
                    OnPropertyChanged("AlarmSeconds");
                }
            }
        }

        private int _atoSaveSeconds;
        public int AutoSaveSeconds
        {
            get { return _atoSaveSeconds; }
            set
            {
                if (_atoSaveSeconds != value)
                {
                    _atoSaveSeconds = value;
                    OnPropertyChanged("AutoSaveSeconds");
                }
            }
        }

        private int _managmentLocX;
        public int ManagmentLocX
        {
            get { return _managmentLocX; }
            set
            {
                if (_managmentLocX != value)
                {
                    _managmentLocX = value;
                    OnPropertyChanged("ManagmentLocX");
                }
            }
        }

        private int _managmentLocY;
        public int ManagmentLocY
        {
            get { return _managmentLocY; }
            set
            {
                if (_managmentLocY != value)
                {
                    _managmentLocY = value;
                    OnPropertyChanged("ManagmentLocY");
                }
            }
        }

        private int _managmentWidth;
        public int ManagmentWidth
        {
            get { return _managmentWidth; }
            set
            {
                if (_managmentWidth != value)
                {
                    _managmentWidth = value;
                    OnPropertyChanged("ManagmentWidth");
                }
            }
        }

        private int _managmentHeight;
        public int ManagmentHeight
        {
            get { return _managmentHeight; }
            set
            {
                if (_managmentHeight != value)
                {
                    _managmentHeight = value;
                    OnPropertyChanged("ManagmentHeight");
                }
            }
        }

        private List<RepositoryRef> _respositoryRef;
        public List<RepositoryRef> RespositoryRefs
        {
            get
            {
                if (_respositoryRef == null)
                    _respositoryRef = new List<RepositoryRef>();
                return _respositoryRef;
            }
            set 
            {
                if(_respositoryRef != value)
                {
                    _respositoryRef = value;
                    OnPropertyChanged("RespositoryRefs");
                }
            }
        }

        #endregion 

        #region TODO: ... other params

        // KNoteManagmentForm: minimized (?), maximized (?), visible (?), hide note number (?)

        // PostIts: always top, style, ....

        // Path initial treefolder 

        #endregion

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // ---
            // Capture the validations implemented with attributes.
            // TODO: apply reflection??
            // ---

            //Validator.TryValidateProperty(this.Xxproperty,
            //   new ValidationContext(this, null, null) { MemberName = "Xxproperty" },
            //   results);


            // ---
            // Specific validations
            // ----

            //if (YyProperty != "xxxxxx" )
            //{
            //    results.Add(new ValidationResult
            //     ("KMSG: xxxxxxxx."
            //     , new[] { "YyProperty" }));
            //}

            return results;
        }

    }
}
