﻿using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

public interface IKAttributeWebApiService
{
    Task<Result<List<KAttributeInfoDto>>> GetAllAsync();
    Task<Result<List<KAttributeInfoDto>>> GetAllAsync(Guid? typeId);
    Task<Result<KAttributeDto>> GetAsync(Guid id);
    Task<Result<KAttributeDto>> SaveAsync(KAttributeDto kattribute);
    Task<Result<KAttributeInfoDto>> DeleteAsync(Guid id);
    Task<Result<List<KAttributeTabulatedValueDto>>> GetKAttributeTabulatedValuesAsync(Guid id);
}

