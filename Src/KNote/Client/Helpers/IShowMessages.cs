using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KNote.Client.Helpers
{
    public interface IShowMessages
    {
        Task ShowErrorMessage(string mensaje);
        Task ShowOkMessage(string mensaje);
    }
}
