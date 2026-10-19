using System.ComponentModel.DataAnnotations;
using KNote.Model;

namespace KNote.ClientWin.Core;

/// <summary>
/// Edit model of the "Change password" dialog (ChangePasswordCtrl). The current password may be empty: a user
/// without a password yet (signed in with the Windows account) sets the first one. The password policy, which
/// depends on the user name, is checked by the controller.
/// </summary>
public class ChangePasswordModel : SmartModelDtoBase
{
    private string _currentPassword;
    public string CurrentPassword
    {
        get { return _currentPassword; }
        set
        {
            if (_currentPassword != value)
            {
                _currentPassword = value;
                OnPropertyChanged("CurrentPassword");
            }
        }
    }

    private string _newPassword;
    public string NewPassword
    {
        get { return _newPassword; }
        set
        {
            if (_newPassword != value)
            {
                _newPassword = value;
                OnPropertyChanged("NewPassword");
            }
        }
    }

    private string _confirmPassword;
    public string ConfirmPassword
    {
        get { return _confirmPassword; }
        set
        {
            if (_confirmPassword != value)
            {
                _confirmPassword = value;
                OnPropertyChanged("ConfirmPassword");
            }
        }
    }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

        if (string.IsNullOrWhiteSpace(NewPassword))
            results.Add(new ValidationResult("The new password is required.", new[] { "NewPassword" }));
        else if (NewPassword != ConfirmPassword)
            results.Add(new ValidationResult("The new password and its confirmation are not the same.", new[] { "ConfirmPassword" }));

        return results;
    }
}
