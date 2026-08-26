using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model;

public abstract class ResultBase
{
    #region Properties


    private List<string> _listErrorMessage;
    public List<string> ListErrorMessage
    {
        get { return  _listErrorMessage.Select( err => err).ToList(); }
        // A setter (not just AddErrorMessage/AddListErrorMessage) is what makes this round-trip
        // through System.Text.Json: a getter-only property is skipped on deserialization, so a
        // Result read back from an HTTP response body always had an empty _listErrorMessage - IsValid
        // came back true regardless of what the server actually sent. ErrorMessage/IsValid stay
        // getter-only and derived, so they're automatically correct on whichever side (server after
        // AddErrorMessage, or client after this setter runs) reads them.
        set { _listErrorMessage = value ?? new List<string>(); }
    }
    
    public string ErrorMessage {
        get { return string.Join(" ", _listErrorMessage); }            
    }

    public virtual bool IsValid
    {
        get { return (_listErrorMessage.Count == 0); }
    }
    
    #endregion

    #region Constructor

    public ResultBase()
    {
        _listErrorMessage = new List<string>();            
    }

    #endregion 

    #region Methods

    public void AddErrorMessage(string errorMessage)
    {
        _listErrorMessage.Add(errorMessage);
    }

    public void AddListErrorMessage(IEnumerable<string> listErrorMessage)
    {
        foreach(var errMsg in listErrorMessage)
            _listErrorMessage.Add(errMsg);
    }

    #endregion
}
