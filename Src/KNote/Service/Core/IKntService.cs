using KNote.Model;
using KNote.Repository;
using KNote.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Service.Core
{
    public interface IKntService : IDisposable
    {
        Guid IdServiceRef { get; }
        IKntRepository Repository { get; }
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
