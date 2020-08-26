using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace RamPaged
{
    public static class IQueryableExtensions
    {
        public static IQueryable<TSource> Paged<TSource>(this IQueryable<TSource> source, Pageable query)
        {
            return source
                .Skip(query.SkipCount)
                .Take(query.PageSize);
        }

        public static IQueryable<TSource> Paged<TSource>(this IQueryable<TSource> source, int skipCount, int pageSize)
        {
            return source
                .Skip(skipCount)
                .Take(pageSize);
        }

        public static async Task<PagedList<T>> ToPagedListAsync<T>(this IQueryable<T> query, Pageable pageable)
        {
            var count = await query?.CountAsync();

            var queryResult = await query?
                .Paged(pageable)?
                .ToListAsync();

            return new PagedList<T>(queryResult, count, pageable);
        }

        public static PagedList<T> ToPagedList<T>(this IQueryable<T> query, Pageable pageable)
        {
            var count = query?.Count() ?? 0;

            var queryResult = query?
                .Paged(pageable)?
                .ToList();

            return new PagedList<T>(queryResult, count, pageable);
        }

        public static IQueryable<TSource> WhereIf<TSource>(this IQueryable<TSource> source, bool condition, Expression<Func<TSource, bool>> predicate)
        {
            if (condition)
                return source.Where(predicate);
            else
                return source;
        }

        public static IQueryable<TSource> WhereIf<TSource>(this IQueryable<TSource> source, bool condition, Expression<Func<TSource, int, bool>> predicate)
        {
            if (condition)
                return source.Where(predicate);
            else
                return source;
        }

        public static IQueryable<T> IncludeIf<T, TProperty>(this IQueryable<T> source, bool condition, Expression<Func<T, TProperty>> path) where T : class
        {
            if (condition)
                return source.Include(path);
            else
                return source;
        }

        public static IQueryable<T> OrderByIf<T>(this IQueryable<T> source, bool condition, string sortString, Type type = null)
        {
            if (condition)
            {
                if (source == null || string.IsNullOrWhiteSpace(sortString))
                    return source;

                if (type != null)
                    sortString = CleanSortBy(type, sortString);

                var sortBy = sortString.Split(','); ;

                string sortExpression = string.Empty;

                foreach (var sortOption in sortBy)
                {
                    if (sortOption.StartsWith("-"))
                        sortExpression = sortExpression + sortOption.Remove(0, 1) + " descending,";
                    else
                        sortExpression = sortExpression + sortOption + ",";
                }

                if (!string.IsNullOrWhiteSpace(sortExpression))
                {
                    try
                    {
                        return source.OrderBy(sortExpression.Remove(sortExpression.Count() - 1));
                    }
                    catch
                    {
                        throw new ValidationException($"Invalid SortBy: {sortString}");
                    }
                }
            }

            return source;
        }

        private static string CleanSortBy(Type type, string sortBy)
        {
            List<string> cleanedExpressions = new List<string>();

            if (string.IsNullOrWhiteSpace(sortBy))
                return sortBy;

            var sortExpressions = sortBy.Split(',');

            foreach (string expression in sortExpressions)
            {
                bool isDescending = false;
                var test = expression;

                if (expression.StartsWith("-"))
                {
                    isDescending = true;
                    test = expression.Substring(1);
                }

                var property = type.GetProperty(test, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property == null)
                {
                    cleanedExpressions.Add(expression);
                    continue;
                }

                var propertyAddress = property.GetCustomAttributes(typeof(OrderByPropertyAddressAttribute), false).FirstOrDefault();

                if (propertyAddress != null)
                {
                    var address = (propertyAddress as OrderByPropertyAddressAttribute).PropertyAddress;

                    if (!string.IsNullOrEmpty(address))
                        cleanedExpressions.Add(isDescending ? $"-{address}" : address);
                }
                else
                {
                    cleanedExpressions.Add(expression);
                }
            }

            return string.Join(",", cleanedExpressions);
        }
    }
}