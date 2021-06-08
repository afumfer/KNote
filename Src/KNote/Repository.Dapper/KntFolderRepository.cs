using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using System.Data.Common;
using Dapper;

using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Repository.Dapper
{
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

        public async Task<Result<List<FolderDto>>> GetAllAsync()
        {            
            var result = new Result<List<FolderDto>>();
            try
            {
                var db = GetOpenConnection();

                var sql = @"SELECT FolderId, FolderNumber, CreationDateTime, ModificationDateTime, [Name], Tags, PathFolder, [Order], OrderNotes, Script, ParentId ";
                sql += "FROM Folders ORDER BY [Order];";

                var entity = await db.QueryAsync<FolderDto>(sql.ToString(), new { });
                result.Entity = entity.ToList();

                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<FolderDto>> GetHomeAsync()
        {
            var result = new Result<FolderDto>();
            try
            {                
                var db = GetOpenConnection();

                var sql = @"SELECT FolderId, FolderNumber, CreationDateTime, ModificationDateTime, [Name], Tags, PathFolder, [Order], OrderNotes, Script, ParentId ";
                sql += "FROM Folders WHERE FolderNumber = 1;";
                
                result.Entity = await db.QueryFirstOrDefaultAsync<FolderDto>(sql.ToString(), new { });

                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }
        public async Task<Result<List<FolderDto>>> GetTreeAsync(Guid? partenId = null)
        {            
            var result = new Result<List<FolderDto>>();

            var treeFolders = new List<FolderDto>();

            try
            {
                var allFoldersInfo = (await GetAllAsync()).Entity;

                treeFolders = allFoldersInfo.Where(fi => fi.ParentId == partenId)
                    .OrderBy(f => f.Order).ThenBy(f => f.Name).ToList();

                foreach (FolderDto f in treeFolders)
                    LoadChilds(f, allFoldersInfo);

                result.Entity = treeFolders;

            }
            catch (KntEntityValidationException ex)
            {
                AddDBEntityErrorsToErrorsList(ex, result.ErrorList);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }

            return ResultDomainAction<List<FolderDto>>(result);
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
            var result = new Result<FolderDto>();
            try
            {
                var db = GetOpenConnection();

                var sql = @"SELECT FolderId, FolderNumber, CreationDateTime, ModificationDateTime, [Name], Tags, PathFolder, [Order], OrderNotes, Script, ParentId ";
                if(folderId != null)
                    sql += "FROM Folders WHERE FolderId = @FolderId;";
                else
                    sql += "FROM Folders WHERE FolderNumber = @FolderNumber;";

                var entity = await db.QueryFirstOrDefaultAsync<FolderDto>(sql.ToString(), new { FolderId = folderId, FolderNumber = folderNumber });

                if (entity == null)
                    result.AddErrorMessage("Entity not found.");
                else
                {
                    if (entity.ParentId != null)
                        entity.ParentFolderDto = await db.QueryFirstOrDefaultAsync<FolderDto>(sql.ToString(), new { Id = entity.ParentId });
                }

                result.Entity = entity;

                if (result.IsValid)
                {
                    var resultChilds = await GetTreeAsync(result.Entity.FolderId);
                    result.Entity.ChildFolders = resultChilds.Entity;
                }

                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }


        public async Task<Result<FolderDto>> AddAsync(FolderDto entity)
        {
            var result = new Result<FolderDto>();
            try
            {
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
                    result.ErrorList.Add("Entity not inserted");

                result.Entity = entity;
                
                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<FolderDto>> UpdateAsync(FolderDto entity)
        {
            var result = new Result<FolderDto>();
            try
            {
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
                    result.ErrorList.Add("Entity not updated");

                result.Entity = entity;

                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            var result = new Result();
            try
            {
                var db = GetOpenConnection();

                var sql = @"DELETE FROM Folders WHERE FolderId = @Id";

                var r = await db.ExecuteAsync(sql.ToString(), new { Id = id });

                if (r == 0)
                    result.AddErrorMessage("Entity not deleted");

                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<int>> GetNextFolderNumber()
        {
            var result = new Result<int>();
            try
            {
                var db = GetOpenConnection();
                result.Entity = GetNextFolderNumber(db);
                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        #region Private methods

        private int GetNextFolderNumber(DbConnection db)
        {
            var sql = "SELECT MAX(FolderNumber) FROM Folders";
            var result = db.ExecuteScalar<int>(sql);

            return result + 1;
        }

        // TODO: Pendiente de refactorizar (este código está repetido en el repositorio EF)
        private void LoadChilds(FolderDto folder, List<FolderDto> allFolders)
        {
            folder.ChildFolders = allFolders.Where(fi => fi.ParentId == folder.FolderId)
                .OrderBy(f => f.Order).ThenBy(f => f.Name).ToList();

            foreach (FolderDto f in folder.ChildFolders)
                LoadChilds(f, allFolders);
        }

        #endregion
    }
}
