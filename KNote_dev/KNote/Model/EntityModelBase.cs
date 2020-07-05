using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KNote.Model
{
    public abstract class EntityModelBase: ModelBase
    {

        [Timestamp]
        public Byte[] Timestamp { get; set; }
    }
}
