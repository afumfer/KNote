using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

// cancellationToken (all of them): the caller no longer wants the answer; see BaseService.SendAsync.
public interface IKAttributeWebApiService
{
    Task<Result<List<KAttributeInfoDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<List<KAttributeInfoDto>>> GetAllAsync(Guid? typeId, CancellationToken cancellationToken = default);
    Task<Result<KAttributeDto>> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<KAttributeDto>> SaveAsync(KAttributeDto kattribute, CancellationToken cancellationToken = default);
    Task<Result<KAttributeInfoDto>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<List<KAttributeTabulatedValueDto>>> GetKAttributeTabulatedValuesAsync(Guid id, CancellationToken cancellationToken = default);
}
