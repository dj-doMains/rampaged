using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RamPaged;

namespace Microsoft.AspNetCore.Mvc
{
    public static class ControllerBaseExtensions
    {
        public static void CreatePageableHeader<T>(this ControllerBase controller, string routeName, PagedList<T> list, Pageable query, IUrlHelper urlHelper)
        {
            var previousPageLink = list?.HasPrevious == true ?
                CreateResourceUri(controller, routeName, ResourceUriType.PreviousPage, query, urlHelper) : null;

            var nextPageLink = list?.HasNext == true ?
                CreateResourceUri(controller, routeName, ResourceUriType.NextPage, query, urlHelper) : null;

            var paginationMetadata = new
            {
                totalCount = list?.TotalCount,
                pageSize = list?.PageSize,
                currentPage = list?.CurrentPage,
                totalPages = list?.TotalPages,
                previousPageLink = previousPageLink,
                nextPageLink = nextPageLink
            };

            var parsedMetadata = JsonConvert.SerializeObject(paginationMetadata);
            controller.Response.Headers.Add("X-Pagination", parsedMetadata);
        }

        public static string CreateResourceUri(this ControllerBase controller, string routeName, ResourceUriType type, Pageable query, IUrlHelper urlHelper)
        {
            if (query == null || string.IsNullOrWhiteSpace(routeName) || urlHelper == null)
                return string.Empty;

            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    {
                        query.PageNumber -= 1;
                        break;
                    }
                case ResourceUriType.NextPage:
                    {
                        query.PageNumber += 1;
                        break;
                    }
                default: break;
            }

            var queryStringData = GetQueryStringData(query);

            return urlHelper.Link(routeName, queryStringData);
        }

        private static dynamic GetQueryStringData(object query)
        {
            dynamic expando = new ExpandoObject();
            var result = expando as IDictionary<string, object>;

            foreach (System.Reflection.PropertyInfo prop in query.GetType().GetProperties())
            {
                object[] attrs = prop.GetCustomAttributes(true);

                var hasIgnoreAttribute = attrs
                    .Where(x => x is IgnorePagedQueryStringAttribute)
                    .Any();

                if (!hasIgnoreAttribute)
                    result[prop.Name] = prop.GetValue(query, null);
            }

            return result;
        }
    }
}