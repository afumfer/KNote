using KNote.Model;
using KNote.Model.Entities;
using KNote.Service.Infrastructure;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Service.Repositories
{
    public interface IKntRepository: IDisposable
    {
        IRepository<KntDbContext, User> Users { get; }
        IRepository<KntDbContext, Folder> Folders { get; }
        IRepository<KntDbContext, Note> Notes { get; }
        IRepository<KntDbContext, NoteKAttribute> NoteKAttributes { get; }
        IRepository<KntDbContext, NoteTask> NoteTasks { get; }
        IRepository<KntDbContext, Window> Windows { get; }
        IRepository<KntDbContext, Resource> Resources { get; }
        IRepository<KntDbContext, KAttribute> KAttributes { get; }
        IRepository<KntDbContext, KAttributeTabulatedValue> KAttributeTabulatedValues { get; }
        IRepository<KntDbContext, SystemValue> SystemValues { get; }
        IRepository<KntDbContext, TraceNote> TraceNotes { get; }
        IRepository<KntDbContext, KEvent> KEvents { get; }
        IRepository<KntDbContext, KMessage> KMessages { get; }
        IRepository<KntDbContext, KLog> KLogs { get; }
        IRepository<KntDbContext, NoteType> NoteTypes { get; }
        IRepository<KntDbContext, TraceNoteType> TraceNoteTypes { get; }
        
        void RefresDbContext();
    }
}
