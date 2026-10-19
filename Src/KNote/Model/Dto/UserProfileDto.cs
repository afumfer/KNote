using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KNote.Model.Dto;

/// <summary>
/// What a user sees and changes of their own account ("My profile"), through Users.GetProfileAsync and
/// Users.SaveProfileAsync. UserName is only shown: it is the user's identity (the token, the Windows account,
/// the same user in every repository) and only an Admin changes it. Changing the email asks for the current
/// password (CurrentPassword), as the email is where a forgotten password is reset.
/// </summary>
public class UserProfileDto : SmartModelDtoBase
{
    #region Property definitions

    private string _userName;
    public string UserName
    {
        get { return _userName; }
        set
        {
            if (_userName != value)
            {
                _userName = value;
                OnPropertyChanged("UserName");
            }
        }
    }

    private string _eMail;
    [Required(ErrorMessage = KMSG)]
    [MaxLength(256)]
    public string EMail
    {
        get { return _eMail; }
        set
        {
            if (_eMail != value)
            {
                _eMail = value;
                OnPropertyChanged("EMail");
            }
        }
    }

    private string _fullName;
    [Required(ErrorMessage = KMSG)]
    [MaxLength(256)]
    public string FullName
    {
        get { return _fullName; }
        set
        {
            if (_fullName != value)
            {
                _fullName = value;
                OnPropertyChanged("FullName");
            }
        }
    }

    // Only needed when the email changes. Not part of the profile: typing it doesn't make the profile dirty.
    public string CurrentPassword { get; set; }

    #endregion

    #region Validations

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

        Validator.TryValidateProperty(this.EMail,
           new ValidationContext(this, null, null) { MemberName = "EMail" },
           results);

        Validator.TryValidateProperty(this.FullName,
           new ValidationContext(this, null, null) { MemberName = "FullName" },
           results);

        return results;
    }

    #endregion
}
