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
