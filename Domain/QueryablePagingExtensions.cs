using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BeautyAppointments.Api.Domain
{
    public static class QueryablePagingExtensions
    {
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query, int page, int pageSize)
        {
            if(page <=0) page = 1;

            if(pageSize <=0 || pageSize >100) pageSize = 20;

            var total = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>
            {
                Items=items,
                Page=page,
                PageSize = pageSize,
                TotalCount =total
            };
        }
    }
}
