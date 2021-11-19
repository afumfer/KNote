using KNote.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Service
{
    public interface IKntService: IDisposable
    {
        Guid IdServiceRef { get; }        
        IKntUserService Users { get; }
        IKntKAttributeService KAttributes { get; }
        IKntSystemValuesService SystemValues { get; }
        IKntFolderService Folders { get; }
        IKntNoteService Notes { get; }
        IKntNoteTypeService NoteTypes { get; }
        Task<bool> TestDbConnection();
        Task<bool> CreateDataBase(string newOwner = null);
        RepositoryRef RepositoryRef { get; }
    }
}
