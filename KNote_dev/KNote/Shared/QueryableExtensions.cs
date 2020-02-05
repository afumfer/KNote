using KNote.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KNote.Shared
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> Pagination<T>(this IQueryable<T> queryable, PaginationDto pagination)
        {            
            return queryable
                .Skip((pagination.Page - 1) * pagination.NumRecords)
                .Take(pagination.NumRecords);
        }
    }
}
