﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using Microsoft.Data.SqlClient;
using Dapper;

using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Repository.Dapper
{
    public class KntFolderRepository : DomainActionBase, IKntFolderRepository
    {
        protected SqlConnection _db;

        public KntFolderRepository(SqlConnection db, bool throwKntException)
        {
            _db = db;
            ThrowKntException = throwKntException;
        }

        public async Task<Result<List<FolderDto>>> GetAllAsync()
        {            
            var result = new Result<List<FolderDto>>();
            try
            {
                var sql = @"SELECT FolderId, FolderNumber, CreationDateTime, ModificationDateTime, [Name], Tags, PathFolder, [Order], OrderNotes, Script, ParentId ";
                sql += "FROM Folders ORDER BY [Order];";

                var entity = await _db.QueryAsync<FolderDto>(sql.ToString(), new { });
                result.Entity = entity.ToList();
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
                var sql = @"SELECT FolderId, FolderNumber, CreationDateTime, ModificationDateTime, [Name], Tags, PathFolder, [Order], OrderNotes, Script, ParentId ";
                sql += "FROM Folders WHERE FolderNumber = 1;";

                result.Entity = await _db.QueryFirstOrDefaultAsync<FolderDto>(sql.ToString(), new { });                 
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }
        public async Task<Result<List<FolderDto>>> GetTreeAsync()
        {            
            var result = new Result<List<FolderDto>>();

            var treeFolders = new List<FolderDto>();

            try
            {
                var allFoldersInfo = (await GetAllAsync()).Entity;

                treeFolders = allFoldersInfo.Where(fi => fi.ParentId == null)
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

        public async Task<Result<FolderDto>> GetAsync(Guid id)
        {            
            var result = new Result<FolderDto>();
            try
            {
                var sql = @"SELECT FolderId, FolderNumber, CreationDateTime, ModificationDateTime, [Name], Tags, PathFolder, [Order], OrderNotes, Script, ParentId ";
                sql += "FROM Folders WHERE FolderId = @Id;";

                var entity = await _db.QueryFirstOrDefaultAsync<FolderDto>(sql.ToString(), new { Id = id });

                if (entity == null)
                    result.AddErrorMessage("Entity not found.");
                else
                {
                    if(entity.ParentId != null)
                        entity.ParentFolderDto = await _db.QueryFirstOrDefaultAsync<FolderDto>(sql.ToString(), new { Id = entity.ParentId });
                }

                result.Entity = entity;
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
                entity.CreationDateTime = DateTime.Now;
                entity.ModificationDateTime = DateTime.Now;

                var sql = @"INSERT INTO Folders (FolderId, FolderNumber, CreationDateTime, ModificationDateTime, [Name], Tags, 
                                PathFolder, [Order], OrderNotes, Script, ParentId )
                            VALUES (@FolderId, @FolderNumber, @CreationDateTime, @ModificationDateTime, @Name, @Tags, 
                                    @PathFolder, @Order, @OrderNotes, @Script, @ParentId)";

                var r = await _db.ExecuteAsync(sql.ToString(),
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

                var r = await _db.ExecuteAsync(sql.ToString(),
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
                var sql = @"DELETE FROM Folders WHERE FolderId = @Id";

                var r = await _db.ExecuteAsync(sql.ToString(), new { Id = id });

                if (r == 0)
                    result.AddErrorMessage("Entity not deleted");

            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public void Dispose()
        {
            //
        }

        #region Private methods

        // TODO: Pendiente de refactorizar (este código está repetido en el repositorio EF)
        private void LoadChilds(FolderDto folder, List<FolderDto> allFolders)
        {
            folder.ChildFolders = allFolders.Where(fi => fi.ParentId == folder.FolderId)
                .OrderBy(f => f.Order).ThenBy(f => f.Name).ToList();

            foreach (FolderDto f in folder.ChildFolders)
                LoadChilds(f, allFolders);
        }

        private int GetNextFolderNumber()
        {
            var sql = "SELECT MAX(FolderNumber) FROM Folders";
            var result = _db.ExecuteScalar(sql);

            return (result == null) ? 1 : ((int)result) + 1;
        }

        #endregion


    }
}