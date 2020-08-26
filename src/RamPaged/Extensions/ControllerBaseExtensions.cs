using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using RamPaged;

namespace Microsoft.AspNetCore.Mvc
{
    public static class ControllerBaseExtensions
    {
        public static void CreatePageableHeader<T>(this ControllerBase controller, string routeName, PagedList<T> list, Pageable query)
        {
            var previousPageLink = list?.HasPrevious == true ?
                 CreateResourceUri(controller, routeName, ResourceUriType.PreviousPage, query) : null;

            var nextPageLink = list?.HasNext == true ?
                CreateResourceUri(controller, routeName, ResourceUriType.NextPage, query) : null;

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

        public static string CreateResourceUri(this ControllerBase controller, string routeName, ResourceUriType type, Pageable query)
        {
            if (query == null || string.IsNullOrWhiteSpace(routeName))
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

            return GetLink(controller.Request, query);
        }

        private static string GetLink(HttpRequest request, Pageable query)
        {
            var queryStringData = GetQueryStringData(query);
            var baseUri = string.Concat(request.Scheme, "://", request.Host.ToUriComponent());
            var endpointUri = new Uri(string.Concat(baseUri, request.Path.Value));

            var link = endpointUri.ToString();

            foreach (KeyValuePair<string, object> kvp in queryStringData)
            {
                if (!string.IsNullOrEmpty(kvp.Key) && !string.IsNullOrEmpty(kvp.Value?.ToString()))
                    link = QueryHelpers.AddQueryString(link, kvp.Key, kvp.Value.ToString());
            }

            return link;
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