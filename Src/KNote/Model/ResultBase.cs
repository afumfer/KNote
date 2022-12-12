using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model;

public abstract class ResultBase
{
    #region Properties

    public string ErrorMessage {
        get { return string.Join(" ", _errorList); }            
    }

    public List<string> ListErrorMessage
    {
        get { return  _errorList.Select( err => err).ToList(); }            
    }
    
    private List<string> _errorList;
    protected List<string> ErrorList
    {
        get { return _errorList; }            
    }

    public virtual bool IsValid
    {
        get { return (_errorList.Count == 0); }
    }
    
    #endregion

    #region Constructor

    public ResultBase()
    {
        _errorList = new List<string>();            
    }

    #endregion 

    #region Methods

    public void AddErrorMessage(string errorMessage)
    {
        _errorList.Add(errorMessage);
    }

    public void AddListErrorMessage(IEnumerable<string> listErrorMessage)
    {
        foreach(var errMsg in listErrorMessage)
            _errorList.Add(errMsg);
    }

    #endregion
}
