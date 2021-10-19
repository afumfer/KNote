using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KNote.Model.Dto
{
    public class NoteKAttributeDto : NoteKAttributeInfoDto
    {
        public string Name { get; set; }

        public string Description { get; set; }
        public EnumKAttributeDataType KAttributeDataType { get; set; }
        public Guid? KAttributeNoteTypeId { get; set; }
        public bool RequiredValue { get; set; }
        public int Order { get; set; }
        public string Script { get; set; }
        public bool Disabled { get; set; }

        // For view        
        public string ValueString { get; set; }        
        public DateTime? ValueDateTime { get; set; }        
        public int? ValueInt { get; set; }        
        public double? ValueDouble { get; set; }        
        public bool ValueBool { get; set; }        
        public string ValueTabulate { get; set; }        
        public string ValueTags { get; set; }

        public List<KAttributeTabulatedValueDto> TabulatedValues = new List<KAttributeTabulatedValueDto>();
        public List<MultiSelectListDto> TagsValues = new List<MultiSelectListDto>();

        #region Validations

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // ---
            // Capture the validations implemented with attributes.
            // ---            

            //Validator.TryValidateProperty(this.Name,
            //   new ValidationContext(this, null, null) { MemberName = "Name" },
            //   results);

            //----
            // Specific validations
            //----

            //Check datatypes
            if (!string.IsNullOrEmpty(Value))
            {
                switch (KAttributeDataType)
                {
                    case EnumKAttributeDataType.Int:
                        int outputInt;
                        if (!int.TryParse(Value, out outputInt))
                            results.Add(new ValidationResult
                                ("KMSG: The value must be integer type."
                                , new[] { "Value" }));
                        break;
                    case EnumKAttributeDataType.Double:
                        double outputDbl;
                        if (!double.TryParse(Value, out outputDbl))
                            results.Add(new ValidationResult
                             ("KMSG: The value must be double type. "
                             , new[] { "Value" }));
                        break;
                    case EnumKAttributeDataType.DateTime:
                        DateTime outputDateTime;
                        if (!DateTime.TryParse(Value, out outputDateTime))
                            results.Add(new ValidationResult
                             ("KMSG: The value must be Date Time type. "
                             , new[] { "Value" }));
                        break;
                    case EnumKAttributeDataType.Bool:
                        bool outputBool;
                        if (!bool.TryParse(Value, out outputBool))
                            results.Add(new ValidationResult
                             ("KMSG: The value must be boolean type. "
                             , new[] { "Value" }));
                        break;
                    default:
                        break;
                }
            }

            if (RequiredValue && string.IsNullOrEmpty(Value))
                results.Add(new ValidationResult
                         ("KMSG: Value is required. "
                         , new[] { "Value" }));

            // ---
            // Return List<ValidationResult>()
            // ---           

            return results;
        }


        #endregion

    }
}