using System.Data.Common;
using System.Reflection;
using Dapper;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Repository.Dapper;

public class KntFolderRepository : KntRepositoryBase, IKntFolderRepository
{        
    public KntFolderRepository(DbConnection singletonConnection, RepositoryRef repositoryRef) 
        : base(singletonConnection, repositoryRef)
    {
    }

    public KntFolderRepository(RepositoryRef repositoryRef)
        : base(repositoryRef)
    {
    }

    public async Task<Result<List<FolderInfoDto>>> GetAllAsync()
    {            
        try
        {
            var result = new Result<List<FolderInfoDto>>();

            var db = GetOpenConnection();

            var sql = @"SELECT FolderId, FolderNumber, CreationDateTime, ModificationDateTime, [Name], Tags, PathFolder, [Order], OrderNotes, Script, ParentId ";
            sql += "FROM Folders ORDER BY [Order];";

            var entity = await db.QueryAsync<FolderInfoDto>(sql.ToString(), new { });
            result.Entity = entity.ToList();

            await CloseIsTempConnection(db);
            
            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<FolderDto>> GetHomeAsync()
    {
        try
        {                
            var result = new Result<FolderDto>();

            var db = GetOpenConnection();

            var sql = @"SELECT FolderId, FolderNumber, CreationDateTime, ModificationDateTime, [Name], Tags, PathFolder, [Order], OrderNotes, Script, ParentId ";
            sql += "FROM Folders WHERE FolderNumber = 1;";
                
            result.Entity = await db.QueryFirstOrDefaultAsync<FolderDto>(sql.ToString(), new { });

            await CloseIsTempConnection(db);
            
            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<List<FolderDto>>> GetTreeAsync(Guid? partenId = null)
    {            

        try
        {
            var result = new Result<List<FolderDto>>();
            var treeFolders = new List<FolderDto>();

            var allFoldersInfo = (await GetAllAsync()).Entity;

            treeFolders = allFoldersInfo.Where(fi => fi.ParentId == partenId).Select( f => f.GetSimpleDto<FolderDto>())
                .OrderBy(f => f.Order).ThenBy(f => f.Name).ToList();

            foreach (FolderDto f in treeFolders)
                LoadChilds(f, allFoldersInfo);

            result.Entity = treeFolders;

            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }

        //return ResultDomainAction<List<FolderDto>>(result);
    }

    public async Task<Result<FolderDto>> GetAsync(Guid folderId)
    {
        return await GetAsync(folderId, null);
    }

    public async Task<Result<FolderDto>> GetAsync(int folderNumber)
    {
        return await GetAsync(null, folderNumber);
    }

    private async Task<Result<FolderDto>> GetAsync(Guid? folderId, int? folderNumber)
    {
        try
        {        
            var result = new Result<FolderDto>();

            var db = GetOpenConnection();

            var sql = @"SELECT FolderId, FolderNumber, CreationDateTime, ModificationDateTime, [Name], Tags, PathFolder, [Order], OrderNotes, Script, ParentId ";
            var sqlEntity = sql;

            if(folderId != null)
                sqlEntity += " FROM Folders WHERE FolderId = @FolderId;";
            else
                sqlEntity += "FROM Folders WHERE FolderNumber = @FolderNumber;";

            var entity = await db.QueryFirstOrDefaultAsync<FolderDto>(sqlEntity.ToString(), new { FolderId = folderId, FolderNumber = folderNumber });

            if (entity == null)
                result.AddErrorMessage("Entity not found.");
            else
            {
                if (entity.ParentId != null)
                {
                    var sqlParnet = sql + " FROM Folders WHERE FolderId = @FolderId;";
                    entity.ParentFolderDto = await db.QueryFirstOrDefaultAsync<FolderDto>(sqlParnet.ToString(), new { FolderId = entity.ParentId });
                }
            }

            result.Entity = entity;

            if (result.IsValid)
            {
                var resultChilds = await GetTreeAsync(result.Entity.FolderId);
                result.Entity.ChildFolders = resultChilds.Entity;
            }

            await CloseIsTempConnection(db);
        
            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<FolderDto>> AddAsync(FolderDto entity)
    {
        try
        {
            var result = new Result<FolderDto>();

            var db = GetOpenConnection();

            entity.CreationDateTime = DateTime.Now;
            entity.ModificationDateTime = DateTime.Now;
            entity.FolderNumber = GetNextFolderNumber(db);

            var sql = @"INSERT INTO Folders (FolderId, FolderNumber, CreationDateTime, ModificationDateTime, [Name], Tags, 
                            PathFolder, [Order], OrderNotes, Script, ParentId )
                        VALUES (@FolderId, @FolderNumber, @CreationDateTime, @ModificationDateTime, @Name, @Tags, 
                                @PathFolder, @Order, @OrderNotes, @Script, @ParentId)";

            var r = await db.ExecuteAsync(sql.ToString(),
                new {
                    entity.FolderId,
                    entity.FolderNumber,
                    entity.CreationDateTime,
                    entity.ModificationDateTime,
                    entity.Name,
                    entity.Tags,
                    entity.PathFolder,
                    entity.Order,
                    entity.OrderNotes,
                    entity.Script,
                    entity.ParentId
                });

            if (r == 0)
                result.AddErrorMessage("Entity not inserted");

            result.Entity = entity;
                
            await CloseIsTempConnection(db);
        
            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<FolderDto>> UpdateAsync(FolderDto entity)
    {
        try
        {
            var result = new Result<FolderDto>();

            var db = GetOpenConnection();

            entity.ModificationDateTime = DateTime.Now;

            var sql = @"UPDATE Folders SET                     
                    FolderNumber = @FolderNumber, 
                    CreationDateTime = @CreationDateTime, 
                    ModificationDateTime = @ModificationDateTime, 
                    [Name] = @Name, 
                    Tags = @Tags,
                    PathFolder = @PathFolder, 
                    [Order] = @Order, 
                    OrderNotes = @OrderNotes, 
                    Script = @Script, 
                    ParentId = @ParentId
                WHERE FolderId = @FolderId";

            var r = await db.ExecuteAsync(sql.ToString(),
                new {
                    entity.FolderId,
                    entity.FolderNumber,
                    entity.CreationDateTime,
                    entity.ModificationDateTime,
                    entity.Name,
                    entity.Tags,
                    entity.PathFolder,
                    entity.Order,
                    entity.OrderNotes,
                    entity.Script,
                    entity.ParentId
                });

            if (r == 0)
                result.AddErrorMessage("Entity not updated");

            result.Entity = entity;

            await CloseIsTempConnection(db);
        
            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var result = new Result();

            var db = GetOpenConnection();

            var sql = @"DELETE FROM Folders WHERE FolderId = @Id";

            var r = await db.ExecuteAsync(sql.ToString(), new { Id = id });

            if (r == 0)
                result.AddErrorMessage("Entity not deleted");

            await CloseIsTempConnection(db);
            
            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<int>> GetNextFolderNumber()
    {
        try
        {        
            var result = new Result<int>();

            var db = GetOpenConnection();
            result.Entity = GetNextFolderNumber(db);
            await CloseIsTempConnection(db);
            
            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    #region Private methods

    private int GetNextFolderNumber(DbConnection db)
    {
        var sql = "SELECT MAX(FolderNumber) FROM Folders";
        var result = db.ExecuteScalar<int>(sql);

        return result + 1;
    }

    // TODO: !!! Pendiente de refactorizar (este código está repetido en el repositorio EF)
    private void LoadChilds(FolderDto folder, List<FolderInfoDto> allFolders)
    {
        folder.ChildFolders = allFolders.Where(fi => fi.ParentId == folder.FolderId).Select(f => f.GetSimpleDto<FolderDto>())
            .OrderBy(f => f.Order).ThenBy(f => f.Name).ToList();

        foreach (FolderDto f in folder.ChildFolders)
            LoadChilds(f, allFolders);
    }

    #endregion
}

