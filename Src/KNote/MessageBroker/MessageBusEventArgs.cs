using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.MessageBroker;

public class MessageBusEventArgs<T> : EventArgs
{
    // TODO: New metadata here
    public T Entity { get; set; }

    public MessageBusEventArgs(T entity)
        : base()
    {
        this.Entity = entity;
    }
}

