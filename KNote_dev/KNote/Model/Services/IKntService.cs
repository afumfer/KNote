﻿using KNote.Model;
using KNote.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Services
{
    public interface IKntService: IDisposable
    {
        IKntUserService Users { get; }
        IKntKAttributeService KAttributes { get; }
        IKntSystemValuesService SystemValues { get; }
        IKntKMessageService KMessages { get; }
        IKntKEventService KEvents { get; }
        IKntFolderService Folders { get; }
        IKntNoteService Notes { get; }
        IKntNoteTypeService NoteTypes { get; }
    }
}
