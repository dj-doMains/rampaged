using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static RamPagedPolicyBuilder AddRamPaged(this IServiceCollection services, Action<RamPagedPolicyBuilderOptions> policyBuilderOptions = null)
        {
            services.Configure<RamPagedPolicyBuilderOptions>(policyBuilderOptions ??
                new Action<RamPagedPolicyBuilderOptions>(delegate (RamPagedPolicyBuilderOptions options) { }));

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper>(implementationFactory =>
            {
                var actionContext = implementationFactory.GetService<IActionContextAccessor>().ActionContext;
                return new UrlHelper(actionContext);
            });

            return new RamPagedPolicyBuilder(services);
        }
    }
}