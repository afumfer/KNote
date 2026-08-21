using KNote.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KNote.Model;

public static class QueryableExtensions
{
    public static IQueryable<T> Pagination<T>(this IQueryable<T> queryable, PageIdentifier pagination)
    {            
        return queryable
            .Skip(pagination.Offset)
            .Take(pagination.PageSize);
    }
}
