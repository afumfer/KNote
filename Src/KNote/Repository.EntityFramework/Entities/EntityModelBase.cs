using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using KNote.Model;

namespace KNote.Repository.EntityFramework.Entities;

public abstract class EntityModelBase: ModelBase
{

    [Timestamp]
    public Byte[] Timestamp { get; set; }
}
