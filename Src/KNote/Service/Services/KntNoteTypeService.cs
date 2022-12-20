using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using System.Linq.Expressions;
using KNote.Repository;
using KNote.Service.Interfaces;
using KNote.Service.Core;
using KNote.Service.ServicesCommands;

namespace KNote.Service.Services;

public class KntNoteTypeService : KntServiceBase, IKntNoteTypeService
{
    #region Constructor

    public KntNoteTypeService(IKntService service) : base(service)
    {

    }

    #endregion

    #region IKntNoteTypes

    public async Task<Result<List<NoteTypeDto>>> GetAllAsync()
    {            
        var command = new KntNoteTypeGetAllAsyncCommand(Service);
        return await ExecuteCommand(command);
    }

    public async Task<Result<NoteTypeDto>> GetAsync(Guid id)
    {            
        var command = new KntNoteTypeGetAsyncCommand(Service, id);
        return await ExecuteCommand(command);
    }

    public async Task<Result<NoteTypeDto>> SaveAsync(NoteTypeDto entity)
    {
        var command = new KntNoteTypeSaveAsyncCommand(Service, entity);
        return await ExecuteCommand(command);
    }

    public async Task<Result<NoteTypeDto>> DeleteAsync(Guid id)
    {
        var command = new KntNoteTypeDeleteAsyncCommand(Service, id);
        return await ExecuteCommand(command);
    }

    #endregion

}
