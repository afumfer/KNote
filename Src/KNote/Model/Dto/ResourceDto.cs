using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto
{
    public class ResourceDto : ResourceInfoDto
    {
        #region Property definitions

        private bool _contentInDB;
        public bool ContentInDB
        {
            get { return _contentInDB; }
            set
            {
                if (_contentInDB != value)
                {
                    _contentInDB = value;
                    OnPropertyChanged("ContentInDB");
                }
            }
        }

        private byte[] _contentArrayBytes;
        public byte[] ContentArrayBytes
        {
            get 
            {
                if(_contentArrayBytes == null)
                {
                    if (_contentBase64 != null)
                        _contentArrayBytes = Convert.FromBase64String(_contentBase64); 
                    else
                        _contentArrayBytes = null;
                }
                return _contentArrayBytes; 
            }
            set
            {
                if (_contentArrayBytes != value)
                {
                    _contentArrayBytes = value;
                    _contentBase64 = null;
                    OnPropertyChanged("ContentArrayBytes");
                }
            }
        }

        private string _contentBase64;        
        public override string ContentBase64 {            
            get
            {
                if (_contentBase64 == null)
                {
                    if (_contentArrayBytes != null)
                        _contentBase64 = Convert.ToBase64String(_contentArrayBytes);
                    else
                        _contentBase64 = null;
                }
                return _contentBase64;
            }
            set
            {
                if(_contentBase64 != value)
                {
                    _contentBase64 = value;
                    _contentArrayBytes = null;
                    OnPropertyChanged("ContentBase64");
                }
            } 
        }

        #endregion 

        #region Validations

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // ---
            // Capture the validations implemented with attributes.
            // ---

            Validator.TryValidateProperty(this.Name,
               new ValidationContext(this, null, null) { MemberName = "Name" },
               results);

            Validator.TryValidateProperty(this.Container,
               new ValidationContext(this, null, null) { MemberName = "Container" },
               results);


            //----
            // Specific validations
            //----

            if (ContentInDB == true && ContentArrayBytes == null)
            {
                results.Add(new ValidationResult
                 ("KMSG: If the content is in the database, you must enter its content "
                 , new[] { "ContentInDB", "ContentArrayBytes" }));
            }

            return results;
        }

        #endregion
    }
}