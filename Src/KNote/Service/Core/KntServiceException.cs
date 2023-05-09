using System;

namespace KNote.Service.Core;

public class KntServiceException : Exception
{
    public KntServiceException() : base() { }

    public KntServiceException(string message) : base(message) { }

    public KntServiceException(string message, Exception inner) : base(message, inner) { }
}
