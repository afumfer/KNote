using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Shared
{
    public abstract class DomainActionBase
    {
        private bool _throwKntException = false;
        public bool ThrowKntException
        {
            get { return _throwKntException; }
            set { _throwKntException = value; }
        }

        public DomainActionBase(bool throwKntException = false)
        {
            _throwKntException = throwKntException;
        }

        protected void AddDBEntityErrorsToErrorsList(KntEntityValidationException ex, List<string> errList)
        {
            foreach (var errEntity in ex.ValidationResults)
                foreach (var err in errEntity.ValidationResults)
                    errList.Add($"{errEntity.ToString()} - {err.ErrorMessage}");
        }
       
        protected void AddExecptionsMessagesToErrorsList(Exception ex, List<string> errList)
        {            
            Exception tmpEx = ex;
            string tmpStr = "";
            while (tmpEx != null)
            {
                if (tmpEx.Message != tmpStr)
                {
                    errList.Add(tmpEx.Message);
                    tmpStr = tmpEx.Message;
                }
                tmpEx = tmpEx.InnerException;
            }
        }

        protected void CopyErrorList(List<string> listSource, List<string> listTarget)
        {
            foreach (string message in listSource)
                listTarget.Add(message);
        }
        
        protected Result<T> ResultDomainAction<T>(Result<T> resultRepositoryAction)
        {
            if (resultRepositoryAction.IsValid == false)
                if (_throwKntException == true)
                    throw new Exception(resultRepositoryAction.Message);
            return resultRepositoryAction;
        }


    }
}
