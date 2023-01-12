using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Repository;

public class KntRepositoryException : Exception
{
    public KntRepositoryException() : base() { }

    public KntRepositoryException(string message) : base(message) { }

    public KntRepositoryException(string message, Exception inner) : base(message, inner) { }
}
