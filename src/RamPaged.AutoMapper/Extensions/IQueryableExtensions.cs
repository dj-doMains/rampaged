using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace RamPaged
{
    public static class IQueryableExtensions
    {
        public static async Task<PagedList<TDestination>> ToPagedListAsync<TSource, TDestination>(this IQueryable<TSource> query, Pageable pageable, IMapper mapper)
        {
            return await GetPagedListAsync<TSource, TDestination>(query, pageable, mapper);
        }

        public static PagedList<TDestination> ToPagedList<TSource, TDestination>(this IQueryable<TSource> query, Pageable pageable, IMapper mapper)
        {
            return GetPagedList<TSource, TDestination>(query, pageable, mapper);
        }

        public static async Task<PagedList<TDestination>> ToPagedListAsync<TSource, TDestination>(this IQueryable<TSource> query, Pageable pageable, IMapper mapper, Action<IMappingOperationOptions> opts)
        {
            return await GetPagedListAsync<TSource, TDestination>(query, pageable, mapper, opts);
        }

        public static PagedList<TDestination> ToPagedList<TSource, TDestination>(this IQueryable<TSource> query, Pageable pageable, IMapper mapper, Action<IMappingOperationOptions> opts)
        {
            return GetPagedList<TSource, TDestination>(query, pageable, mapper, opts);
        }

        private static async Task<PagedList<TDestination>> GetPagedListAsync<TSource, TDestination>(IQueryable<TSource> query, Pageable pageable, IMapper mapper, Action<IMappingOperationOptions> opts = null)
        {
            var count = await query?.CountAsync();

            var queryResult = await query?
                .Paged(pageable)?
                .ToListAsync();

            List<TDestination> items;

            if (opts == null)
                items = mapper.Map<List<TDestination>>(queryResult);
            else
                items = mapper.Map<List<TDestination>>(queryResult, opts);

            return new PagedList<TDestination>(items, count, pageable);
        }

        private static PagedList<TDestination> GetPagedList<TSource, TDestination>(IQueryable<TSource> query, Pageable pageable, IMapper mapper, Action<IMappingOperationOptions> opts = null)
        {
            var count = query?.Count() ?? 0;

            var queryResult = query?
                .Paged(pageable)?
                .ToList();

            List<TDestination> items;

            if (opts == null)
                items = mapper.Map<List<TDestination>>(queryResult);
            else
                items = mapper.Map<List<TDestination>>(queryResult, opts);

            return new PagedList<TDestination>(items, count, pageable);
        }
    }
}