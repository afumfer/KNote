using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model
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

        protected Result ResultDomainAction(Result resultRepositoryAction)
        {
            if (resultRepositoryAction.IsValid == false)
                if (_throwKntException == true)
                    throw new Exception(resultRepositoryAction.Message);
            return resultRepositoryAction;
        }

        protected int ExtractNoteNumberSearch(string textSearch)
        {
            if (string.IsNullOrEmpty(textSearch.Trim()))
                return 0;

            var n = 0;
            string strStartNumber = "";

            if (textSearch[0] == '#')
            {
                var i = textSearch.IndexOf(' ', 0);
                if (i > 0)
                    strStartNumber = textSearch.Substring(1, i - 1);
                else
                    strStartNumber = textSearch.Substring(1, textSearch.Length - 1);
                int.TryParse(strStartNumber, out n);
            }

            return n;
        }

        protected List<string> ExtractListTokensSearch(string textIn)
        {
            List<string> tokens = new List<string>();
            int i = 0, lenString = 0;
            int state = 0;
            char c;
            string word = "";
            char action = 'a';
            string especialToken = "";

            if (textIn == null)
                return tokens;

            lenString = textIn.Length;

            while (i < lenString)
            {
                c = textIn[i];

                switch (c)
                {
                    case '\"':
                        if (state == 0)
                        {
                            state = 1;
                            action = 'p';
                        }
                        else
                        {
                            state = 0;
                            action = 'p';
                        }
                        break;
                    case ' ':
                        if (state == 0 || state == 2)
                            action = 'p';
                        break;
                    default:
                        action = 'a';
                        break;
                }

                if (action == 'p')
                {
                    if (word != "")
                    {
                        // Si es un Token especial => va con la siguiente palabra
                        if (word == "!")
                            especialToken = word;
                        else
                        {
                            word = especialToken + word;
                            especialToken = "";
                            tokens.Add(word);
                        }
                    }
                    word = "";
                }
                if (action == 'a')
                    word += c;

                i++;
            }

            if (word != "")
                tokens.Add(word);

            return tokens;
        }

    }
}
