using KNote.Model;
using KNote.Repository.Entities;
using KNote.Repository.EntityFramework;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Repository
{
    public interface IKntRepository: IDisposable
    {
                
        IKntNoteTypeRepository NoteTypes { get; }                
        IKntSystemValuesRepository SystemValues { get; }        
        IKntFolderRepository Folders { get; }
        IKntKAttributeRepository KAttributes { get; }

        IKntNoteRepository Notes { get; }



        /// ------------------

        IGenericRepositoryEF<KntDbContext, User> Users { get; }





        // -------------------------------
        // Basura

        // IGenericRepositoryEF<KntDbContext, KAttributeTabulatedValue> KAttributeTabulatedValues { get; }

        //IGenericRepositoryEF<KntDbContext, NoteKAttribute> NoteKAttributes { get; }
        //IGenericRepositoryEF<KntDbContext, Resource> Resources { get; }
        //IGenericRepositoryEF<KntDbContext, NoteTask> NoteTasks { get; }


        // -------------------------------

        //IGenericRepositoryEF<KntDbContext, Window> Windows { get; }
        //IGenericRepositoryEF<KntDbContext, TraceNote> TraceNotes { get; }
        //IGenericRepositoryEF<KntDbContext, KEvent> KEvents { get; }
        //IGenericRepositoryEF<KntDbContext, KMessage> KMessages { get; }
        //IGenericRepositoryEF<KntDbContext, KLog> KLogs { get; }
        //IGenericRepositoryEF<KntDbContext, TraceNoteType> TraceNoteTypes { get; }


        void RefresDbContext();
    }
}
