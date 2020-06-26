﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using KNote.Model.Dto.Info;
using KNote.Model.Dto;
using KNote.Model;

namespace KNote.Model.Services
{
    public interface IKntFolderService
    {
        Result<List<FolderDto>> GetAll();
        Result<List<FolderDto>> GetRoots();
        Result<List<FolderDto>> GetTree();
        Result<FolderDto> Get(int folerNumber);
        Result<FolderDto> Get(Guid folderId);
        Result<FolderDto> New(FolderInfoDto entity = null);
        Result<FolderDto> Save(FolderDto entityInfo);
        Task<Result<FolderDto>> SaveAsync(FolderDto entityInfo);
                
        int GetNextFolderNumber();
        
        Result<FolderDto> Delete(Guid id);
        Task<Result<FolderDto>> DeleteAsync(Guid id);

        // TODO: 
        // Result<List<Folder>> GetAllFull(Expression<Func<Folder, bool>> predicate)
    }
}