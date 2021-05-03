using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KNote.Server.Helpers
{
    public static class HttpContextExtensions
    {
        public static void InsertPaginationParamInResponse(this HttpContext context,
            double count, int numRecordsToShow)
        {
            if (context == null) { throw new ArgumentNullException(nameof(context)); }
            
            double totalPages = Math.Ceiling(count / numRecordsToShow);
            context.Response.Headers.Add("count", count.ToString());
            context.Response.Headers.Add("totalPages", totalPages.ToString());
        }

        public async static Task InsertPaginationParamInResponseFromQuery<T>(this HttpContext context,
            IQueryable<T> queryable, int numRecordsToShow)
        {
            if (context == null) { throw new ArgumentNullException(nameof(context)); }

            double count = await queryable.CountAsync();
            double totalPages = Math.Ceiling(count / numRecordsToShow);
            context.Response.Headers.Add("count", count.ToString());
            context.Response.Headers.Add("totalPages", totalPages.ToString());
        }

    }
}
