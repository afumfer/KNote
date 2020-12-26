using System;
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
        IKntUserRepository Users { get; }        
    }
}
