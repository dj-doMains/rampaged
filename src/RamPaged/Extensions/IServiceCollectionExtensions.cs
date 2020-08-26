using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static RamPagedPolicyBuilder AddRamPaged(this IServiceCollection services, Action<RamPagedPolicyBuilderOptions> policyBuilderOptions = null)
        {
            services.Configure<RamPagedPolicyBuilderOptions>(policyBuilderOptions ??
                new Action<RamPagedPolicyBuilderOptions>(delegate (RamPagedPolicyBuilderOptions options) { }));

            return new RamPagedPolicyBuilder(services);
        }
    }
}