using KNote.Model;
using KNote.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KNote.Client.ClientDataServices
{
    public interface IUserWebApiService
    {
        Task<Result<List<UserDto>>> GetAll();
    }
}
