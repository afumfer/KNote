using System.ComponentModel.DataAnnotations;
using KNote.Model;

namespace KNote.ClientWin.Core;

/// <summary>
/// Edit model of the sign-in dialog (LoginCtrl), used when the user signs in with a KNote user name and
/// password instead of the Windows account (AppAuthenticationMode.Credentials).
/// </summary>
public class LoginModel : SmartModelDtoBase
{
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

    private string _password;
    public string Password
    {
        get { return _password; }
        set
        {
            if (_password != value)
            {
                _password = value;
                OnPropertyChanged("Password");
            }
        }
    }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

        if (string.IsNullOrWhiteSpace(UserName))
            results.Add(new ValidationResult("KMSG: The user name is required.", new[] { "UserName" }));

        if (string.IsNullOrWhiteSpace(Password))
            results.Add(new ValidationResult("KMSG: The password is required.", new[] { "Password" }));

        return results;
    }
}
