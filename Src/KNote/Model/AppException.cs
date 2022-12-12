using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model;

public class AppException : Exception
{
    public AppException() : base() { }

    public AppException(string message) : base(message) { }

    public AppException(string message, Exception inner) : base(message, inner) { }
    
    // TODO: eliminate next constructor ??
    //public AppException(string message, params object[] args)
    //    : base(String.Format(CultureInfo.CurrentCulture, message, args))
    //{
    //}

}
