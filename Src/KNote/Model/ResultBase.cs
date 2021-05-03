using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model
{
    public abstract class ResultBase
    {
        #region Fields

        protected List<string> _errorList;

        #endregion

        #region Properties

        public string Message {
            get { return GetErrorMessageString(); }
            protected set { }
        }

        // TODO: Pendiente de refactorizar esta propiedad (sólo debe ser de lectura ??)
        public List<string> ErrorList
        {
            get { return _errorList; }
            set { _errorList = value; }
        }

        public virtual bool IsValid
        {
            get { return (_errorList.Count == 0); }
        }

        public string Tag { get; set; }

        #endregion

        #region Constructor

        public ResultBase()
        {
            _errorList = new List<string>();
            Message = "";
        }

        #endregion 

        #region Methods

        public void AddErrorMessage(string errorMessage)
        {
            _errorList.Add(errorMessage);
        }

        private string GetErrorMessageString()
        {
            return string.Join(" ", _errorList);
        }

        #endregion
    }
}
