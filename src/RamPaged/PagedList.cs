using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace RamPaged
{
    public class PagedList<T> : List<T>
    {
        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            AddRange(items);
        }

        public PagedList(List<T> items, int count, Pageable query)
        {
            TotalCount = count;
            PageSize = query.PageSize;
            CurrentPage = query.PageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)query.PageSize);
            AddRange(items);
        }

        public int CurrentPage { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; set; }

        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;

        public static PagedList<T> Create(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }

        public async static Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }

        public static PagedList<T> Create(IQueryable<T> source, Pageable pageable)
        {
            return Create(source, pageable.PageNumber, pageable.PageSize);
        }

        public async static Task<PagedList<T>> CreateAsync(IQueryable<T> source, Pageable pageable)
        {
            return await CreateAsync(source, pageable.PageNumber, pageable.PageSize);
        }
    }
}