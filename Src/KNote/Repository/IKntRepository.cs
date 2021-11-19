using KNote.Model;

namespace KNote.Repository;

public interface IKntRepository: IDisposable
{                
    IKntNoteTypeRepository NoteTypes { get; }                
    IKntSystemValuesRepository SystemValues { get; }        
    IKntFolderRepository Folders { get; }
    IKntKAttributeRepository KAttributes { get; }
    IKntNoteRepository Notes { get; }              
    IKntUserRepository Users { get; }
    Task<bool> TestDbConnection();
    RepositoryRef RespositoryRef { get;  }
}
